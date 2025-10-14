// ApiService.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using NV22SpectralInteg.Data;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace NV22SpectralInteg.Services;

public static class ApiService
{
    private static readonly HttpClient client = new HttpClient();
    internal const string BaseUrl = "https://uat.pocketmint.ai/api/kiosks";
    internal const string AuthToken = "a55cf4p6-e57a-3w20-8ag4-33s55d27ev78";
    private static string Status = "live";
    private static string DbPath = Path.Combine(Application.StartupPath, "App_Data", "KioskTransactions.db");

    static ApiService()
    {
        // Initialize HttpClient headers once
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd("PocketMint-KioskApp/1.0");
        client.DefaultRequestHeaders.Add("Authorization", AuthToken);
    }

    // New method to initialize the service with your config
    public static void Initialize(AppConfig config)
    {
        // Use a ternary operator to set the status string in one line.
        Status = config.IsDevelopment ? "development" : "live";
        Logger.Log($"ApiService initialized in '{Status}' mode.");
    }


    // NOTE: This is a simplified example. In a real app, you would have proper response models
    // instead of 'dynamic' to ensure type safety.

    public static async Task<(bool Success, string ErrorMessage)> ValidateAndSetKioskSessionAsync(string kioskId)
    {
        if (Status == "live")
        {
            string apiUrl = $"{BaseUrl}/get/kiosks/details";
            try
            {
                var requestBody = new { kioskId = int.Parse(kioskId) };
                string jsonPayload = JsonConvert.SerializeObject(requestBody);
                Logger.Log($"📦 Payload: {jsonPayload}");

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string responseText = await response.Content.ReadAsStringAsync();
                Logger.Log($"📬 API Response: {responseText}");

                if (!response.IsSuccessStatusCode)
                {
                    return (false, $"API Error: {response.StatusCode}");
                }

                var result = JsonConvert.DeserializeObject<dynamic>(responseText);
                if (result == null || result?.isSucceed != true || result?.data == null)
                {
                    return (false, "Invalid Kiosk ID or API response.");
                }

                AppSession.KioskId = result?.data.KIOSKID;
                AppSession.KioskRegId = result?.data.REGID;
                AppSession.StoreName = result?.data.KIOSKNAME;
                AppSession.StoreAddress = $"{result?.data.ADDRESS}, {result?.data.CITY}, {result?.data.LOCATION}, {result?.data.ZIPCODE}";

                return (true, string.Empty); // Updated to return a non-null string
            }
            catch (Exception ex)
            {
                Logger.LogError("Error validating Kiosk ID", ex);
                return (false, $"Exception: {ex.Message}");
            }
        }
        else
        {
            return (true, string.Empty); // Updated to return a non-null string
        }
    }


    // In your API/Service class

    public static async Task<(bool Success, string? message, dynamic result)> SubmitSettlementReportAsync(string settlementCode)
    {
        // Assuming these are available from your session/config
        string apiUrl = $"{BaseUrl}/validate/settlement"; // Endpoint for submission

        // --- 1. Determine Date Range ---
        // StartTime is the EndDate of the LAST successful report (or DateTime.MinValue for the first run)
        if (string.IsNullOrEmpty(AppSession.KioskId))
        {
            Logger.Log("KioskId is null or empty. Cannot retrieve last report end date.");
            return (false, "KioskId is null or empty", new { }); // Or handle this case appropriately based on your application's logic
        }
        var data = TransactionRepository.GetLastReportEndDate(AppSession.KioskId);
        DateTime startTime = data.startTime;

        TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternZone);

        // EndTime is the current moment (inclusive)
        DateTime endTime = easternTime;

        // If the difference is zero or negative (e.g., clock drift or immediate re-run), exit.
        if (startTime >= endTime)
        {
            Logger.Log($"No new transactions since last report at {startTime}. Skipping submission.");
            return (false , $"No new transactions since last report at {startTime}.", new { });
        }

        // --- 2. Gather Aggregated Data from DB ---
        dynamic aggregatedData = TransactionRepository.GetAggregatedSettlementData(AppSession.KioskId, startTime, endTime);

        if (aggregatedData.totalSettlementAmount <= 0)
        {
            Logger.Log("No financial transactions found in the specified period. Skipping submission.");
            // Optional: If you skip, you can choose NOT to update the KioskReport table, 
            // ensuring the next run checks the same period until money is deposited.
            return (false, $"No new transactions since last report at {startTime}.", new { });
        }
        string currentLocalIp = SystemInfo.GetActiveLocalIpAddress();

        // --- 3. Construct API Payload ---
        var requestBody = new
        {
            storeRegId = AppSession.KioskRegId,
            kioskId = AppSession.KioskId,
            settlementCode = settlementCode,
            startTime = startTime.ToString("yyyy-MM-ddTHH:mm:00"),
            endTime = endTime.ToString("yyyy-MM-ddTHH:mm:00"),
            totalDenominationDepository = aggregatedData.totalDenominationDepository,
            totalSettlementAmount = aggregatedData.totalSettlementAmount,
            kioskIpAddress = currentLocalIp
        };

        string jsonPayload = JsonConvert.SerializeObject(requestBody, Formatting.Indented);
        Logger.Log($"📦 Settlement Payload: {jsonPayload}");

        // --- 4. Send API Request ---
        try
        {
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            string responseText = await response.Content.ReadAsStringAsync();

            Logger.Log($"📬 API Response: {responseText}");

            dynamic result = JsonConvert.DeserializeObject<dynamic>(responseText);

            if (result != null && result?.isSucceed == true)
            {
                string receiptUrl = result?.data?.RECEIPTURL ?? "";

                TransactionRepository.SaveSettlementReport(
                    AppSession.KioskId, settlementCode, startTime, endTime, receiptUrl);

                Logger.Log("✅ Settlement report successful. Report marker saved in DB.");
                return (true, result?.message?.ToString() ?? "Success", result);
            }

            // Log both HTTP status and application-level failure reason
            Logger.Log($"❌ Settlement submission failed. " +
                       $"HTTP Status: {(int)response.StatusCode} {response.StatusCode}. " +
                       $"App Message: {result?.message?.ToString() ?? "Unknown error"}");

            return (false, result?.message?.ToString() ?? "Request failed", new { });
        }
        catch (Exception ex)
        {
            Logger.LogError("❌ Error submitting settlement report", ex);
            return (false, "Error submitting settlement report", new { });
        }

    }


    public static async Task<bool> SendOtpAsync(string mobileNo)
    {
        // For demonstration, returning true. Uncomment your real logic here.
        if (Status == "live")
        {

            string apiUrl = $"{BaseUrl}/send/user/mobileno/otp";
            try
            {
                if (string.IsNullOrEmpty(AppSession.KioskId))
                {
                    Logger.Log("KioskId is null or empty. Cannot retrieve last report end date.");
                    return false; // Or handle this case appropriately based on your application's logic
                }
                var payload = new { mobileNo, kioskId = int.Parse(AppSession.KioskId) };
                string jsonPayload = JsonConvert.SerializeObject(payload);
                Logger.Log($"📦 Payload: {jsonPayload}");

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string responseText = await response.Content.ReadAsStringAsync();
                Logger.Log($"📬 API Response: {responseText}");

                var result = JsonConvert.DeserializeObject<dynamic>(responseText);
                if (result != null && result?.isSucceed == true)
                {
                    AppSession.smsId = result?.smsId;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception in SendOtpAsync", ex);
                return false;
            }
        }
        else
        {
            return true;
        }
    }

    public static async Task<bool> VerifyOtpAsync(string mobileNo, string otp)
    {
        // For demonstration, returning true. Uncomment your real logic here.

        if (Status == "live")
        {
            string apiUrl = $"{BaseUrl}/validate/user/mobileno/otp";
            try
            {
                if (string.IsNullOrEmpty(AppSession.KioskId))
                {
                    Logger.Log("KioskId is null or empty. Cannot retrieve last report end date.");
                    return false; // Or handle this case appropriately based on your application's logic
                }
                var payload = new
                {
                    mobileNo,
                    kioskId = int.Parse(AppSession.KioskId),
                    otp,
                    smsId = AppSession.smsId
                };
                string jsonPayload = JsonConvert.SerializeObject(payload);
                Logger.Log($"📦 Payload: {jsonPayload}");

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string responseText = await response.Content.ReadAsStringAsync();
                Logger.Log($"📬 API Response: {responseText}");

                var result = JsonConvert.DeserializeObject<dynamic>(responseText);
                if (result != null && result?.isSucceed == true && result?.data != null)
                {
                    AppSession.CustomerRegId = result?.data.REGID;
                    AppSession.CustomerName = result?.data.NAME;
                    AppSession.CustomerBALANCE = result?.data.BALANCE;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception in VerifyOtpAsync", ex);
                return false;
            }
        }
        else
        {
            return true;
        }
    }

    public static async Task<dynamic> PersistTransactionAsync(IReadOnlyDictionary<string, int> noteCounts)
    {

        string apiUrl = $"{BaseUrl}/user/transaction/persist";

        try
        {
            var amountDetails = noteCounts
                .Select(kvp =>
                {
                    string key = kvp.Key;
                    int count = kvp.Value;
                    // This logic correctly extracts the denomination number from strings like "100 INR"
                    var denominationMatch = System.Text.RegularExpressions.Regex.Match(key, @"\d+");
                    int denomination = denominationMatch.Success && int.TryParse(denominationMatch.Value, out var d) ? d : 0;
                    return new { denomination, count, total = denomination * count };
                })
                .ToList();

            decimal kioskTotalAmount = amountDetails.Sum(a => a.total);

            var requestBody = new
            {
                kioskId = AppSession.KioskId,
                kioskRegId = AppSession.KioskRegId,
                customerRegId = AppSession.CustomerRegId,
                kioskTotalAmount, 
                amountDetails
            };


            Logger.Log("📤 Sending transaction request to API via ApiService...");
            string jsonPayload = JsonConvert.SerializeObject(requestBody);
            Logger.Log($"📦 Payload: {jsonPayload}");

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(apiUrl, content);
            string responseText = await response.Content.ReadAsStringAsync();
            Logger.Log($"📬 API Response: {responseText}");

            if (response.IsSuccessStatusCode)
            {
                SaveTransactionWithDetails(requestBody);
            }

            // Updated the DeserializeObject calls to handle potential null values explicitly
            return JsonConvert.DeserializeObject<dynamic>(responseText) ?? new { isSucceed = false, message = "Deserialization returned null" };
        }
        catch (Exception ex)
        {
            Logger.LogError("🚨 Exception in PersistTransactionAsync", ex);
            // Return a failed response object so the UI can handle it gracefully
            return JsonConvert.DeserializeObject<dynamic>($"{{ 'isSucceed': false, 'message': 'Error: {ex.Message}' }}") ?? new { isSucceed = false, message = "Deserialization returned null" };
        }
    }

    public static void SaveTransactionWithDetails(dynamic requestBody)
    {
        string transactionId = Guid.NewGuid().ToString();

        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {

            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternZone);

            // Insert master transaction
            string insertTransaction = @"
            INSERT INTO Transactions (TransactionId, Timestamp, KioskId, KioskRegId, CustomerRegId, KioskTotalAmount)
            VALUES (@TransactionId, @Timestamp, @KioskId, @KioskRegId, @CustomerRegId, @KioskTotalAmount);";

            using (var cmd = new SqliteCommand(insertTransaction, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@TransactionId", transactionId);
                cmd.Parameters.AddWithValue("@Timestamp", easternTime);
                cmd.Parameters.AddWithValue("@KioskId", (string)requestBody.kioskId ?? "");
                cmd.Parameters.AddWithValue("@KioskRegId", (string)requestBody.kioskRegId ?? "");
                cmd.Parameters.AddWithValue("@CustomerRegId", (string)requestBody.customerRegId ?? "");
                cmd.Parameters.AddWithValue("@KioskTotalAmount", (decimal)requestBody.kioskTotalAmount);

                cmd.ExecuteNonQuery();
            }

            // Insert each detail row
            string insertDetail = @"
            INSERT INTO TransactionDetails (TransactionId, Denomination, Count, Total)
            VALUES (@TransactionId, @Denomination, @Count, @Total);";

            foreach (var detail in requestBody.amountDetails)
            {
                using var cmd = new SqliteCommand(insertDetail, connection, transaction);
                cmd.Parameters.AddWithValue("@TransactionId", transactionId);
                cmd.Parameters.AddWithValue("@Denomination", (int)detail.denomination);
                cmd.Parameters.AddWithValue("@Count", (int)detail.count);
                cmd.Parameters.AddWithValue("@Total", (decimal)detail.total);

                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            Logger.Log($"✅ Transaction {transactionId} saved with {requestBody.amountDetails.Count} details.");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Logger.LogError($"❌ Failed to save transaction {transactionId}", ex);
            throw;
        }
    }




    public static async Task<bool> LogMachineAvailabilityAsync()
    {
        if (Status == "live")
        {
            string apiUrl = $"{BaseUrl}/machine/availability/log";
            try
            {
                string currentLocalIp = SystemInfo.GetActiveLocalIpAddress();

                if (currentLocalIp == "Not Found" || currentLocalIp == "Error")
                {
                    Logger.MachineLog($"🚨 Skipping availability log: Could not determine local IP address (Result: {currentLocalIp}).");
                    return false;
                }

                var requestBody = new
                {
                    kioskId = AppSession.KioskId,
                    ipAddress = currentLocalIp
                };

                string jsonPayload = JsonConvert.SerializeObject(requestBody);
                Logger.MachineLog($"📦 Payload: {jsonPayload}");
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string responseText = await response.Content.ReadAsStringAsync();
                Logger.MachineLog($"📬 API Response: {responseText}");

                if (!response.IsSuccessStatusCode)
                {
                    Logger.MachineLog($"🚨 API Error while logging availability: {response.StatusCode}");
                    return false;
                }

                var result = JsonConvert.DeserializeObject<dynamic>(responseText);
                if (result == null || result?.isSucceed != true)
                {
                    Logger.MachineLog("🚨 Failed to log machine availability: Invalid API response.");
                    return false;
                }

                Logger.MachineLog("✅ Machine availability logged successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.MachineLog($"🚨 Exception in LogMachineAvailabilityAsync is:\n{ex}");
                return false;
            }
        }
        else
        {
            return true;
        }
    }
}