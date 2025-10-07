using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
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
                    KioskId TEXT PRIMARY KEY,
                    SettlementCode TEXT,
                    StartDate DATETIME,
                    ReportGeneratedDate DATETIME,
                    ReportUrl TEXT
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

        public static void SaveSettlementReport(string kioskId, string settlementCode, DateTime startDate, string ReportUrl)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            string insertReport = @"
            INSERT INTO KioskReport (KioskId, SettlementCode, StartDate, ReportGeneratedDate, ReportUrl)
            VALUES (@KioskId, @SettlementCode, @StartDate, @ReportGeneratedDate, @ReportUrl);";

            using (var cmd = new SqliteCommand(insertReport, connection))
            {
                cmd.Parameters.AddWithValue("@KioskId", kioskId);
                cmd.Parameters.AddWithValue("@SettlementCode", settlementCode);
                cmd.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                cmd.Parameters.AddWithValue("@ReportGeneratedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                cmd.Parameters.AddWithValue("@ReportUrl", ReportUrl);

                cmd.ExecuteNonQuery();
            }
        }


        public static dynamic GetAggregatedSettlementData(string kioskId, DateTime startTime, DateTime endTime)
        {
            string dbPath = Path.Combine(Application.StartupPath, "App_Data", "KioskTransactions.db");
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            // Determine the START operator and the date value based on the start time.
            string startOperator;
            DateTime effectiveStartTime;

            if (string.IsNullOrEmpty(AppSession.KioskId))
            {
                Logger.Log("KioskId is null or empty. Cannot retrieve last report end date.");
                return false; // Or handle this case appropriately based on your application's logic
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
            cmd.Parameters.AddWithValue("@StartTimeParam", effectiveStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            cmd.Parameters.AddWithValue("@EndTime", endTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            var details = new List<object>();
            decimal grandTotalAmount = 0;

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    decimal total = reader.GetDecimal(2);
                    details.Add(new
                    {
                        denomination = reader.GetInt64(0), // Denomination type is flexible
                        count = reader.GetInt64(1),
                        total = total
                    });
                    grandTotalAmount += total;
                }
            }

            return new
            {
                totalDenominationDepository = details,
                totalSettlementAmount = grandTotalAmount
            };
        }


       
    }
}
