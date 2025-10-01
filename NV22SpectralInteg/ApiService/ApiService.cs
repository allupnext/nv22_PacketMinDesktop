// ApiService.cs
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NV22SpectralInteg.Services;

public static class ApiService
{
    private static readonly HttpClient client = new HttpClient();
    internal const string BaseUrl = "https://uat.pocketmint.ai/api/kiosks";
    internal const string AuthToken = "a55cf4p6-e57a-3w20-8ag4-33s55d27ev78";
    private static string Status = "live";

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
        if(Status == "live")
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
                if (result == null || result.isSucceed != true || result.data == null)
                {
                    return (false, "Invalid Kiosk ID or API response.");
                }

                AppSession.KioskId = result.data.KIOSKID;
                AppSession.KioskRegId = result.data.REGID;
                AppSession.StoreName = result.data.KIOSKNAME;
                AppSession.StoreAddress = $"{result.data.ADDRESS}, {result.data.CITY}, {result.data.LOCATION}, {result.data.ZIPCODE}";

                return (true, null);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error validating Kiosk ID", ex);
                return (false, $"Exception: {ex.Message}");
            }
        }
        else
        {
            return (true, null);
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
                var payload = new { mobileNo, kioskId = int.Parse(AppSession.KioskId) };
                string jsonPayload = JsonConvert.SerializeObject(payload);
                Logger.Log($"📦 Payload: {jsonPayload}");

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string responseText = await response.Content.ReadAsStringAsync();
                Logger.Log($"📬 API Response: {responseText}");

                var result = JsonConvert.DeserializeObject<dynamic>(responseText);
                if (result != null && result.isSucceed == true)
                {
                    AppSession.smsId = result.smsId;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception in SendOtpAsync", ex);
                return false;
            }

            return await Task.FromResult(true); // Mock success
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
                if (result != null && result.isSucceed == true && result.data != null)
                {
                    AppSession.CustomerRegId = result.data.REGID;
                    AppSession.CustomerName = result.data.NAME;
                    AppSession.CustomerBALANCE = result.data.BALANCE;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception in VerifyOtpAsync", ex);
                return false;
            }

            return await Task.FromResult(true); // Mock success
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

            return JsonConvert.DeserializeObject<dynamic>(responseText);
        }
        catch (Exception ex)
        {
            Logger.LogError("🚨 Exception in PersistTransactionAsync", ex);
            // Return a failed response object so the UI can handle it gracefully
            return JsonConvert.DeserializeObject<dynamic>($"{{ 'isSucceed': false, 'message': 'Error: {ex.Message}' }}");
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
                if (result == null || result.isSucceed != true)
                {
                    Logger.MachineLog("🚨 Failed to log machine availability: Invalid API response.");
                    return false;
                }

                Logger.MachineLog("✅ Machine availability logged successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.MachineLog("🚨 Exception in LogMachineAvailabilityAsync is:\n" + ex);
                return false;
            }
        }
        else
        {
            return true;
        }
    }
}