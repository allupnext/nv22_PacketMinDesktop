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
    private const string BaseUrl = "https://uat.pocketmint.ai/api/kiosks";
    internal const string AuthToken = "a55cf4p6-e57a-3w20-8ag4-33s55d27ev78";

    static ApiService()
    {
        // Initialize HttpClient headers once
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd("PocketMint-KioskApp/1.0");
        client.DefaultRequestHeaders.Add("Authorization", AuthToken);
    }
    
    // NOTE: This is a simplified example. In a real app, you would have proper response models
    // instead of 'dynamic' to ensure type safety.

    public static async Task<(bool Success, string ErrorMessage)> ValidateAndSetKioskSessionAsync(string kioskId)
    {
        string apiUrl = $"{BaseUrl}/get/kiosks/details";
        try
        {
            var requestBody = new { kioskId = int.Parse(kioskId) };
            string jsonPayload = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(apiUrl, content);
            string responseText = await response.Content.ReadAsStringAsync();

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
            return (true, null);
        }
        catch (Exception ex)
        {
            Logger.LogError("Error validating Kiosk ID", ex);
            return (false, $"Exception: {ex.Message}");
        }
    }

    public static async Task<bool> SendOtpAsync(string mobileNo)
    {
        // For demonstration, returning true. Uncomment your real logic here.

        string apiUrl = $"{BaseUrl}/send/user/mobileno/otp";
        try
        {
            var payload = new { mobileNo, kioskId = int.Parse(AppSession.KioskId) };
            string jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(apiUrl, content);
            string responseText = await response.Content.ReadAsStringAsync();

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

    public static async Task<bool> VerifyOtpAsync(string mobileNo, string otp)
    {
        // For demonstration, returning true. Uncomment your real logic here.

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
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(apiUrl, content);
            string responseText = await response.Content.ReadAsStringAsync();

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


    public static async Task<(bool Success, string Message, dynamic Result)> PersistTransactionAsync(Dictionary<string, int> noteEscrowCounts, int grandTotal)
    {
        string apiUrl = $"{BaseUrl}/user/transaction/persist";

        try
        {
            var amountDetails = noteEscrowCounts
                .Select(kvp =>
                {
                    string key = kvp.Key;
                    int count = kvp.Value;
                    var denominationMatch = System.Text.RegularExpressions.Regex.Match(key, @"\d+");
                    int denomination = denominationMatch.Success && int.TryParse(denominationMatch.Value, out var d) ? d : 0;
                    return new { denomination, count, total = denomination * count };
                })
                .ToList();

            var requestBody = new
            {
                kioskId = AppSession.KioskId,
                kioskRegId = AppSession.KioskRegId,
                customerRegId = AppSession.CustomerRegId,
                kioskTotalAmount = grandTotal,
                amountDetails = amountDetails
            };

            string jsonPayload = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            Logger.Log("📤 Sending transaction request to API...");
            Logger.Log($"📦 Payload: {jsonPayload}");

            HttpResponseMessage response = await client.PostAsync(apiUrl, content);
            string responseText = await response.Content.ReadAsStringAsync();

            Logger.Log($"📬 API Response: {responseText}");

            var result = JsonConvert.DeserializeObject<dynamic>(responseText);
            if (result == null)
            {
                return (false, "API returned null response.", null);
            }

            if (result.isSucceed == true)
            {
                return (true, null, result);
            }
            else
            {
                return (false, (string)result.message, result);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("🚨 Error in PersistTransactionAsync", ex);
            return (false, $"Exception: {ex.Message}", null);
        }
    }





}