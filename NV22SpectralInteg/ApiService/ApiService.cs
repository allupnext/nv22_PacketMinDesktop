// ApiService.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using NV22SpectralInteg.Data;
using NV22SpectralInteg.Model;
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

    private static async Task<ApiResult<T>> ProcessPostRequest<T>(string apiUrl, object requestBody, Action<string> logFunction = null)
    {
        Action<string> logger = logFunction ?? Logger.Log;
        try
        {
            string jsonPayload = JsonConvert.SerializeObject(requestBody);
            logger($"📦 Payload: {jsonPayload}");

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(apiUrl, content);
            string responseText = await response.Content.ReadAsStringAsync();
            logger($"📬 API Response: {responseText}");

            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(responseText);

            if (apiResponse == null || !apiResponse.isSucceed)
            {
                return new ApiResult<T>(false, apiResponse?.message ?? "API call failed with no specific message.", default);
            }

            return new ApiResult<T>(true, apiResponse.message, apiResponse.data);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Exception during API call to {apiUrl}", ex);
            return new ApiResult<T>(false, $"Exception: {ex.Message}", default);
        }
    }


    // NOTE: This is a simplified example. In a real app, you would have proper response models
    // instead of 'dynamic' to ensure type safety.

    public static async Task<(bool Success, string Message)> ValidateAndSetKioskSessionAsync(string kioskId)
    {
        if (Status != "live") return (true, string.Empty);

        string apiUrl = $"{BaseUrl}/get/kiosks/details";

        // 1. Prepare Request
        var requestBody = new KioskSessionRequest { KioskId = kioskId };

        // 2. Process API Call using helper
        var result = await ProcessPostRequest<KioskSessionData>(apiUrl, requestBody);

        if (!result.Success)
        {
            return (false, result.Message);
        }

        // 3. Map Data to Session
        var data = result.Data;
        if (data == null)
        {
            return (false, "Invalid Kiosk ID or API response data was null.");
        }

        AppSession.KioskId = data.KIOSKID;
        AppSession.KioskRegId = data.REGID;
        AppSession.StoreName = data.KIOSKNAME;
        AppSession.StoreAddress = $"{data.ADDRESS}, {data.CITY}, {data.LOCATION}, {data.ZIPCODE}";

        return (true, string.Empty);
    }

    // In your API/Service class

    public static async Task<ApiResult<SettlementReportData>> SubmitSettlementReportAsync(string settlementCode)
    {
        string apiUrl = $"{BaseUrl}/validate/settlement";

        // --- 1. Determine Date Range ---
        if (string.IsNullOrEmpty(AppSession.KioskId))
        {
            Logger.Log("KioskId is null or empty. Cannot retrieve last report end date.");
            return new ApiResult<SettlementReportData>(false, "KioskId is null or empty", default);
        }

        // NOTE: Assuming TransactionRepository.GetLastReportEndDate now returns a structure 
        // where startTime can be accessed. (e.g., a tuple or a custom object)
        var data = TransactionRepository.GetLastReportEndDate(AppSession.KioskId);
        DateTime startTime = data.startTime;

        TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternZone);
        DateTime endTime = easternTime;

        if (startTime >= endTime)
        {
            Logger.Log($"No new transactions since last report at {startTime}. Skipping submission.");
            return new ApiResult<SettlementReportData>(false, $"No new transactions since last report at {startTime}.", default);
        }
                
        // --- 2. Gather Aggregated Data from DB ---
        // NOTE: Cast the result from dynamic to the specific DTO you use for aggregation
        // This assumes TransactionRepository.GetAggregatedSettlementData returns a type 
        // that can be cast to AggregatedSettlementData (or you update the repo to return it directly)
        AggregatedSettlementData aggregatedData = TransactionRepository.GetAggregatedSettlementData(AppSession.KioskId, startTime, endTime);

        if (aggregatedData.totalSettlementAmount <= 0)
        {
            Logger.Log("No financial transactions found in the specified period. Skipping submission.");
            return new ApiResult<SettlementReportData>(false, $"No new transactions since last report at {startTime}.", default);
        }

        string currentLocalIp = SystemInfo.GetActiveLocalIpAddress();

        // --- 3. Construct API Payload using the type-safe DTO ---
        var requestBody = new SettlementReportRequest
        {
            storeRegId = AppSession.KioskRegId,
            kioskId = AppSession.KioskId,
            settlementCode = settlementCode,
            startTime = startTime.ToString("yyyy-MM-ddTHH:mm"),
            endTime = endTime.ToString("yyyy-MM-ddTHH:mm"),
            totalDenominationDepository = aggregatedData.totalDenominationDepository,
            totalSettlementAmount = aggregatedData.totalSettlementAmount,
            kioskIpAddress = currentLocalIp
        };

        // --- 4. Send API Request using generic helper ---
        var apiResult = await ProcessPostRequest<SettlementReportData>(apiUrl, requestBody);

        if (apiResult.Success)
        {
            // Data is already deserialized and checked for success
            string receiptUrl = apiResult.Data?.RECEIPTURL ?? "";

            TransactionRepository.SaveSettlementReport(
                AppSession.KioskId, settlementCode, startTime, endTime, receiptUrl);

            Logger.Log("✅ Settlement report successful. Report marker saved in DB.");
        }
        else
        {
            Logger.Log($"❌ Settlement submission failed. App Message: {apiResult.Message}");
        }

        // Return the clean ApiResult object
        return apiResult;
    }

    public static async Task<bool> SendOtpAsync(string mobileNo)
    {
        // For demonstration, keep non-live path simple
        if (Status != "live")
        {
            return true;
        }

        string apiUrl = $"{BaseUrl}/send/user/mobileno/otp";

        if (string.IsNullOrEmpty(AppSession.KioskId))
        {
            Logger.Log("KioskId is null or empty. Cannot send OTP.");
            return false;
        }

        try
        {
            // 1. Use the type-safe Request DTO (OtpSendRequest)
            var requestBody = new OtpSendRequest
            {
                mobileNo = mobileNo,
                kioskId = AppSession.KioskId
            };

            // 2. Process API Call using generic helper
            // Expected Data Type T is OtpSendResponseData
            var result = await ProcessPostRequest<OtpSendResponseData>(apiUrl, requestBody);

            if (!result.Success)
            {
                // Log the error message from the result object
                Logger.Log($"❌ SendOtp failed: {result.Message}");
                return false;
            }

            // 3. Map type-safe Data to Session
            // result.Data is of type OtpSendResponseData
            AppSession.smsId = result.Data?.smsId;

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError("Exception in SendOtpAsync", ex);
            return false;
        }
    }

    // Ensure you have 'using NV22SpectralInteg.Models;' at the top of ApiService.cs

    public static async Task<bool> VerifyOtpAsync(string mobileNo, string otp)
    {
        // For demonstration, keep non-live path simple
        if (Status != "live")
        {
            return true;
        }

        string apiUrl = $"{BaseUrl}/validate/user/mobileno/otp";

        if (string.IsNullOrEmpty(AppSession.KioskId))
        {
            Logger.Log("KioskId is null or empty. Cannot verify OTP.");
            return false;
        }

        try
        {
            // 1. Use the type-safe Request DTO (OtpVerifyRequest)
            var requestBody = new OtpVerifyRequest
            {
                mobileNo = mobileNo,
                kioskId = AppSession.KioskId,
                otp = otp,
                smsId = AppSession.smsId
            };

            // 2. Process API Call using generic helper
            // Expected Data Type T is OtpVerifyData
            var result = await ProcessPostRequest<OtpVerifyData>(apiUrl, requestBody);

            if (!result.Success)
            {
                // Log the error message from the result object
                Logger.Log($"❌ VerifyOtp failed: {result.Message}");
                return false;
            }

            // 3. Map type-safe Data to Session
            // result.Data is of type OtpVerifyData
            var data = result.Data;

            // Ensure data is not null (though the helper typically covers this)
            if (data == null)
            {
                Logger.Log("VerifyOtp succeeded but returned null data.");
                return false;
            }

            AppSession.CustomerRegId = data.REGID;
            AppSession.CustomerName = data.NAME;
            AppSession.CustomerBALANCE = data.BALANCE;

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError("Exception in VerifyOtpAsync", ex);
            return false;
        }
    }

    public static async Task<ApiResult<TransactionPersistData>> PersistTransactionAsync(IReadOnlyDictionary<string, int> noteCounts)
    {
        string apiUrl = $"{BaseUrl}/user/transaction/persist";

        try
        {
            // --- 1. Map input to type-safe DTOs ---
            var amountDetails = noteCounts
                .Select(kvp =>
                {
                    // This original logic for parsing denomination remains
                    var denominationMatch = System.Text.RegularExpressions.Regex.Match(kvp.Key, @"\d+");
                    int denomination = denominationMatch.Success && int.TryParse(denominationMatch.Value, out var d) ? d : 0;

                    return new DenominationDetail
                    {
                        denomination = denomination,
                        count = kvp.Value,
                        total = denomination * kvp.Value
                    };
                })
                .ToList();

            decimal kioskTotalAmount = amountDetails.Sum(a => a.total);

            // 2. Construct type-safe Request Body
            var requestBody = new TransactionPersistRequest
            {
                kioskId = AppSession.KioskId,
                kioskRegId = AppSession.KioskRegId,
                customerRegId = AppSession.CustomerRegId,
                kioskTotalAmount = kioskTotalAmount,
                amountDetails = amountDetails
            };

            Logger.Log("📤 Sending transaction request to API via ApiService...");

            // --- 3. Send API Request using generic helper ---
            // Expected Data Type T is TransactionPersistData
            var apiResult = await ProcessPostRequest<TransactionPersistData>(apiUrl, requestBody);

            // --- 4. Process Local DB Save ---
            if (apiResult.Success)
            {
                AppSession.CustomerBALANCE = apiResult.Data.userBalance;
                AppSession.StoreBalance = apiResult.Data.storeBalance;
                // IMPORTANT: SaveTransactionWithDetails must be updated to accept a type-safe DTO 
                // (TransactionPersistRequest) instead of 'dynamic' to maintain type-safety 
                // throughout your application.
                SaveTransactionWithDetails(requestBody);
                Logger.Log("✅ Transaction persisted successfully, saving locally.");
            }
            else
            {
                Logger.Log($"❌ Transaction persistence failed. App Message: {apiResult.Message}");
            }


            return apiResult;
        }
        catch (Exception ex)
        {
            Logger.LogError("🚨 Exception in PersistTransactionAsync", ex);
            // Return a failed result object gracefully
            return new ApiResult<TransactionPersistData>(false, $"Error: {ex.Message}", default);
        }
    }

    public static void SaveTransactionWithDetails(TransactionPersistRequest requestBody)
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
                // Access properties via the type-safe object:
                cmd.Parameters.AddWithValue("@KioskId", requestBody.kioskId ?? "");
                cmd.Parameters.AddWithValue("@KioskRegId", requestBody.kioskRegId ?? "");
                cmd.Parameters.AddWithValue("@CustomerRegId", requestBody.customerRegId ?? "");
                cmd.Parameters.AddWithValue("@KioskTotalAmount", requestBody.kioskTotalAmount);

                cmd.ExecuteNonQuery();
            }

            // Insert each detail row
            string insertDetail = @"
        INSERT INTO TransactionDetails (TransactionId, Denomination, Count, Total)
        VALUES (@TransactionId, @Denomination, @Count, @Total);";

            foreach (var detail in requestBody.amountDetails) // 'amountDetails' is now List<DenominationDetail>
            {
                using var cmd = new SqliteCommand(insertDetail, connection, transaction);
                cmd.Parameters.AddWithValue("@TransactionId", transactionId);
                cmd.Parameters.AddWithValue("@Denomination", detail.denomination); // No cast needed
                cmd.Parameters.AddWithValue("@Count", detail.count);             // No cast needed
                cmd.Parameters.AddWithValue("@Total", detail.total);             // No cast needed

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
        if (Status != "live")
        {
            return true;
        }

        string apiUrl = $"{BaseUrl}/machine/availability/log";

        try
        {
            string currentLocalIp = SystemInfo.GetActiveLocalIpAddress();

            if (currentLocalIp == "Not Found" || currentLocalIp == "Error")
            {
                Logger.MachineLog($"🚨 Skipping availability log: Could not determine local IP address (Result: {currentLocalIp}).");
                return false;
            }

            // 1. Use the type-safe Request DTO (AvailabilityRequest)
            var requestBody = new AvailabilityRequest
            {
                kioskId = AppSession.KioskId,
                ipAddress = currentLocalIp
            };

            // 2. Process API Call using generic helper
            // Expected Data Type T is AvailabilityResponseData (or an empty type if the 'data' is always null/empty)
            var result = await ProcessPostRequest<AvailabilityResponseData>(apiUrl, requestBody, Logger.MachineLog);

            if (!result.Success)
            {
                // Log the detailed error message from the helper/API response
                Logger.MachineLog($"🚨 Failed to log machine availability: {result.Message}");
                return false;
            }

            Logger.MachineLog("✅ Machine availability logged successfully.");
            return true;
        }
        catch (Exception ex)
        {
            // Catch exceptions not handled by ProcessPostRequest (e.g., network issues)
            Logger.MachineLog($"🚨 Exception in LogMachineAvailabilityAsync is:\n{ex}");
            return false;
        }
    }
}