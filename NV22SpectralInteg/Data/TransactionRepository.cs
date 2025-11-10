using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using NV22SpectralInteg.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace NV22SpectralInteg.Data
{
    public static class TransactionRepository
    {
        private static readonly string DbPath = Path.Combine(Application.StartupPath, "App_Data", "KioskTransactions.db");

        public static void EnsureDatabaseAndTable()
        {
            string directoryPath = Path.GetDirectoryName(DbPath)!;

            // Create directory if it doesn't exist
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            // Create DB file if it doesn't exist
            if (!File.Exists(DbPath))
            {
                using (var fs = File.Create(DbPath))
                {
                    // just create empty file, SQLite will initialize
                }
            }

            using (var connection = new SqliteConnection($"Data Source={DbPath};"))
            {
                connection.Open();

                // Create Transactions table with TransactionId as PK
                string createTransactionsTable = @"
                CREATE TABLE IF NOT EXISTS Transactions (
                    TransactionId TEXT PRIMARY KEY,
                    Timestamp DATETIME,
                    KioskId TEXT,
                    KioskRegId TEXT,
                    CustomerRegId TEXT, 
                    KioskTotalAmount REAL
                );";

                // Create TransactionDetails table
                string createTransactionDetailsTable = @"
                CREATE TABLE IF NOT EXISTS TransactionDetails (
                    TransactionId TEXT,
                    Denomination INTEGER,
                    Count INTEGER,
                    Total REAL,
                    FOREIGN KEY (TransactionId) REFERENCES Transactions(TransactionId)
                );";

                // NEW: Create KioskMetadata table to store sync state
                string createKioskMetadataTable = @"
                CREATE TABLE IF NOT EXISTS KioskReport (
                    KioskId TEXT,
                    SettlementCode TEXT,
                    StartDate DATETIME,
                    ReportGeneratedDate DATETIME,
                    ReportUrl TEXT
                );";

                // Create KioskInfo table to store basic kiosk info
                string createKioskInfoTable = @"
                CREATE TABLE IF NOT EXISTS KioskInfo (
                    Id INTEGER PRIMARY KEY CHECK (Id = 1),
                    KioskId TEXT,
                    KioskRegId TEXT,
                    StoreName TEXT,
                    KioskName TEXT,
                    StoreAddress TEXT
                );";

                // Create penddingTxnInfo table to store basic kiosk info
                string TxnInfoTable = @"
                    CREATE TABLE IF NOT EXISTS RemainingTxnInfo (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CustomerMobile TEXT NOT NULL,
                        Denomination TEXT NOT NULL,
                        Count INTEGER NOT NULL,
                        UNIQUE(CustomerMobile, Denomination)
                    );";

                using (var cmd = new SqliteCommand(createKioskMetadataTable, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SqliteCommand(createTransactionsTable, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SqliteCommand(createTransactionDetailsTable, connection))
                {
                    cmd.ExecuteNonQuery();
                }


                using (var cmd = new SqliteCommand(createKioskInfoTable, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SqliteCommand(TxnInfoTable, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                //string insertKioskMetadata = @"
                //    INSERT INTO KioskReport (KioskId, SettlementCode, StartDate, ReportGeneratedDate, ReportUrl)
                //    VALUES (@KioskId, @SettlementCode, @StartDate, @ReportGeneratedDate, @ReportUrl);";

                //using (var cmd = new SqliteCommand(insertKioskMetadata, connection))
                //{
                //    cmd.Parameters.AddWithValue("@KioskId", 3);
                //    cmd.Parameters.AddWithValue("@SettlementCode", 229467);
                //    cmd.Parameters.AddWithValue("@StartDate", "2025-10-06 08:00:00");
                //    cmd.Parameters.AddWithValue("@ReportGeneratedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                //    cmd.Parameters.AddWithValue("@ReportUrl", "");

                //    cmd.ExecuteNonQuery();
                //}



                // Insert dummy transaction data (only if table is empty)
                string checkTransactionCount = "SELECT COUNT(*) FROM Transactions;";
                using (var checkCmd = new SqliteCommand(checkTransactionCount, connection))
                {
                    var result = checkCmd.ExecuteScalar();
                    long count = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt64(result);

                    if (count == 0)
                    {
                        string insertTransactions = @"
                        INSERT INTO Transactions (TransactionId, Timestamp, KioskId, KioskRegId, CustomerRegId, KioskTotalAmount)
                        VALUES 
                            ('TXN001', '2025-11-07 04:31:00', '3', '63', '27', 10.00);";

                        using (var insertCmd = new SqliteCommand(insertTransactions, connection))
                        {
                            insertCmd.ExecuteNonQuery();
                        }

                        // Insert dummy transaction details
                        string insertTransactionDetails = @"
                        INSERT INTO TransactionDetails (TransactionId, Denomination, Count, Total)
                        VALUES 
                            ('TXN001', 10, 1, 10.00);";

                        using (var detailsCmd = new SqliteCommand(insertTransactionDetails, connection))
                        {
                            detailsCmd.ExecuteNonQuery();
                        }
                    }
                }

            }
        }



        public static (DateTime startTime, bool isFirstReport) GetLastReportEndDate(string kioskId)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            // 1. Check for the last successful report date in KioskReport
            string selectLastReportDate = "SELECT ReportGeneratedDate FROM KioskReport WHERE KioskId = @KioskId ORDER BY ReportGeneratedDate DESC LIMIT 1;";

            using (var cmd = new SqliteCommand(selectLastReportDate, connection))
            {
                cmd.Parameters.AddWithValue("@KioskId", kioskId);

                var result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value && DateTime.TryParse(result.ToString(), out DateTime lastDate))
                {
                    // Scenario 1: Previous report found. Return its end date.
                    return (lastDate, false);
                }
            }

            // 2. Scenario 2: No previous report found. Find the oldest transaction timestamp.
            string selectOldestTransaction = "SELECT MIN(Timestamp) FROM Transactions WHERE KioskId = @KioskId;";

            using (var cmd = new SqliteCommand(selectOldestTransaction, connection))
            {
                cmd.Parameters.AddWithValue("@KioskId", kioskId);

                var result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value && DateTime.TryParse(result.ToString(), out DateTime oldestTimestamp))
                {
                    // If the oldest transaction is found, return that date.
                    // This will be the true starting point for the first ever report.
                    return (oldestTimestamp, true);
                }
            }

            // Scenario 3: No transactions or reports found at all. Use a safe default (like "now" minus 1 minute or MinValue).
            // Using MinValue is safest to ensure the next query gets all data, though the previous step should catch it.
            return (DateTime.MinValue, true);
        }

        public static void SaveSettlementReport(string kioskId, string settlementCode, DateTime startDate, DateTime ReportGeneratedDate, string ReportUrl)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var transaction = connection.BeginTransaction();

            string insertReport = @"
                INSERT INTO KioskReport (KioskId, SettlementCode, StartDate, ReportGeneratedDate, ReportUrl)
                VALUES (@KioskId, @SettlementCode, @StartDate, @ReportGeneratedDate, @ReportUrl);";

            using (var cmd = new SqliteCommand(insertReport, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@KioskId", kioskId);
                cmd.Parameters.AddWithValue("@SettlementCode", settlementCode);
                cmd.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                cmd.Parameters.AddWithValue("@ReportGeneratedDate", ReportGeneratedDate.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                cmd.Parameters.AddWithValue("@ReportUrl", ReportUrl);

                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }


        public static AggregatedSettlementData GetAggregatedSettlementData(string kioskId, DateTime startTime, DateTime endTime)
        {
            string dbPath = Path.Combine(Application.StartupPath, "App_Data", "KioskTransactions.db");
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            // Determine the START operator and the date value based on the start time.
            string startOperator;
            DateTime effectiveStartTime;

            var defaultResult = new AggregatedSettlementData
            {
                totalDenominationDepository = new List<DenominationSettlement>(),
                totalSettlementAmount = 0m
            };

            if (string.IsNullOrEmpty(AppSession.KioskId))
            {
                Logger.Log("KioskId is null or empty. Cannot retrieve last report end date.");
                // ✅ Return the valid, empty data structure
                return defaultResult;
            }

            var data = TransactionRepository.GetLastReportEndDate(AppSession.KioskId);

            if (data.isFirstReport == false)
            {
                startOperator = ">";
                effectiveStartTime = data.startTime;
            }
            else
            {
                // No report entry, but Transactions exist. startTime is the oldest Timestamp. Use >=.
                startOperator = ">=";
                effectiveStartTime = (data.startTime == DateTime.MinValue)
                                             ? new DateTime(1900, 1, 1) // Ultimate safety floor
                                             : data.startTime;
            }

            string sql = $@"
            SELECT 
                T1.Denomination, 
                SUM(T1.Count) AS TotalCount, 
                SUM(T1.Total) AS GrandTotal
            FROM TransactionDetails T1
            INNER JOIN Transactions T2 ON T1.TransactionId = T2.TransactionId
            WHERE T2.KioskId = @KioskId
            AND T2.Timestamp {startOperator} @StartTimeParam
            AND T2.Timestamp <= @EndTime
            GROUP BY T1.Denomination
            ORDER BY T1.Denomination;";

            using var cmd = new SqliteCommand(sql, connection);

            cmd.Parameters.AddWithValue("@KioskId", kioskId);

            // Always add the StartTime parameter (passing the effective/calculated time)
            cmd.Parameters.AddWithValue("@StartTimeParam", effectiveStartTime.ToString("yyyy-MM-dd HH:mm:ss"));

            cmd.Parameters.AddWithValue("@EndTime", endTime.ToString("yyyy-MM-dd HH:mm:ss"));

            var details = new List<DenominationSettlement>(); // <-- Use strong type here
            decimal grandTotalAmount = 0;

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    decimal total = reader.GetDecimal(2);
                    grandTotalAmount += total;

                    // Create the strongly-typed DenominationSettlement object
                    details.Add(new DenominationSettlement
                    {
                        denomination = reader.GetInt64(0),
                        count = reader.GetInt64(1),
                        total = total
                    });
                }
            }

            return new AggregatedSettlementData
            {
                totalDenominationDepository = details,
                totalSettlementAmount = grandTotalAmount
            };
        }











        // Kiosk detail storage methods would go here

        public static void SaveKioskInfo(string kioskId, string kioskRegId, string storeName, string kioskName, string storeAddress)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            string checkQuery = "SELECT COUNT(*) FROM KioskInfo;";
            using var checkCmd = new SqliteCommand(checkQuery, connection);
            var result = checkCmd.ExecuteScalar();
            long count = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt64(result);

            string sql;

            if (count == 0)
            {
                sql = "INSERT INTO KioskInfo (Id, KioskId, KioskRegId, StoreName, KioskName, StoreAddress) VALUES (1, @KioskId, @KioskRegId, @StoreName, @KioskName, @StoreAddress);";
            }
            else
            {
                sql = "UPDATE KioskInfo SET KioskId = @KioskId, KioskRegId = @KioskRegId, StoreName = @StoreName, KioskName = @KioskName, StoreAddress = @StoreAddress WHERE Id = 1;";
            }

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@KioskId", kioskId);
            cmd.Parameters.AddWithValue("@KioskRegId", kioskRegId);
            cmd.Parameters.AddWithValue("@StoreName", storeName);
            cmd.Parameters.AddWithValue("@KioskName", kioskName);
            cmd.Parameters.AddWithValue("@StoreAddress", storeAddress);
            cmd.ExecuteNonQuery();
        }


        public static (string? kioskId, string? kioskRegId, string? storeName, string? kioskName, string? storeAddress) GetKioskInfo()
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            string sql = "SELECT KioskId, KioskRegId, StoreName, KioskName, StoreAddress FROM KioskInfo WHERE Id = 1;";
            using var cmd = new SqliteCommand(sql, connection);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (
                    reader["KioskId"]?.ToString(),
                    reader["KioskRegId"]?.ToString(),
                    reader["StoreName"]?.ToString(),
                    reader["KioskName"]?.ToString(),
                    reader["StoreAddress"]?.ToString()
                );
            }

            return (null, null, null, null, null);
        }












        // Txn Save

        public static void SaveTxn(string customerMobile, string denomination, int count)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            // Ensure full durability for sudden power loss
            using (var pragmaCmd = new SqliteCommand("PRAGMA synchronous=FULL;", connection))
            {
                pragmaCmd.ExecuteNonQuery();
            }

            using var transaction = connection.BeginTransaction();

            string insertOrUpdateQuery = @"
                INSERT INTO RemainingTxnInfo (CustomerMobile, Denomination, Count)
                VALUES (@CustomerMobile, @Denomination, @Count)
                ON CONFLICT(CustomerMobile, Denomination)
                DO UPDATE SET Count = Count + @Count
                WHERE CustomerMobile = @CustomerMobile;";

            using (var cmd = new SqliteCommand(insertOrUpdateQuery, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@CustomerMobile", customerMobile);
                cmd.Parameters.AddWithValue("@Denomination", denomination);
                cmd.Parameters.AddWithValue("@Count", count);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }



        // 📤 Get dictionary data back from database (grouped by Denomination)
        public static Dictionary<string, int> GetTxnInfo(string customerMobile)
        {
            var result = new Dictionary<string, int>();

            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            // Group by Denomination and sum the counts
            string selectQuery = @"
                    SELECT Denomination, SUM(Count) AS TotalCount
                    FROM RemainingTxnInfo
                    WHERE CustomerMobile = @CustomerMobile
                    GROUP BY Denomination;";

            using var cmd = new SqliteCommand(selectQuery, connection);
            cmd.Parameters.AddWithValue("@CustomerMobile", customerMobile);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string denomination = reader.GetString(0);   // Denomination
                int totalCount = reader.GetInt32(1);     // SUM(Count)
                result[denomination] = totalCount;
            }

            return result;
        }



        public static void ClearTxnInfo(string customerMobile)
        {
            if (string.IsNullOrEmpty(customerMobile))
            {
                Logger.Log("⚠️ Cannot clear TxnInfo: CustomerMobile is null or empty.");
                return;
            }

            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            // Use a transaction for the DELETE operation for safety
            using var transaction = connection.BeginTransaction();

            string deleteQuery = @"
                DELETE FROM RemainingTxnInfo 
                WHERE CustomerMobile = @CustomerMobile;";

            using (var cmd = new SqliteCommand(deleteQuery, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@CustomerMobile", customerMobile);
                int rowsAffected = cmd.ExecuteNonQuery();
                Logger.Log($"🗑️ Cleared {rowsAffected} transaction row(s) for mobile: {customerMobile}.");
            }

            transaction.Commit();
        }
    }

}
