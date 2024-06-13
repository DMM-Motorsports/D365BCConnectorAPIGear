using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using D365BCConnectorAPIGear.Entities;
using D365BCConnectorAPIGear.Infrastructure;
using D365BCConnectorAPIGear.StatusCodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace D365BCConnectorAPIGear;

public abstract class BusinessCentralService : IBusinessCentralService
{
    public enum FilterOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        And,
        Or
    }

    protected static readonly IHttpClientFactory HttpClientFactory = new InternalHttpClientFactory();
    protected readonly HttpClient _client;
    protected readonly string _clientId;
    protected readonly string _clientSecret;
    protected readonly string _companyId;
    protected readonly string _tenantId;


    public BusinessCentralService(string clientId, string clientSecret, string tenantId, string companyId)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _tenantId = tenantId;
        _companyId = companyId;
        _client = HttpClientFactory.CreateClient();
    }


    // Properties to access token and expiration from the singleton
    protected DateTime _tokenExpirationTime => TokenManager.Instance.TokenExpirationTime;
    protected string _bearerToken => TokenManager.Instance.BearerToken;

    public DateTime TokenExpirationTime => _tokenExpirationTime;
    public string BearerToken => _bearerToken;

    
    
    public async Task UpdateAccessTokenAsync(bool forceUpdate = false)
    {
        if (!string.IsNullOrEmpty(_bearerToken) &&
            DateTime.Now < _tokenExpirationTime.Subtract(TimeSpan.FromMinutes(2)) && forceUpdate != true)
            return;

        var url =
            $"https://login.microsoftonline.com/{_tenantId}/oauth2/v2.0/token?resource=https://api.businesscentral.dynamics.com";

        var content = new StringContent(
            $"grant_type=client_credentials&scope=https://api.businesscentral.dynamics.com/.default&client_id={_clientId}&client_secret={_clientSecret}",
            Encoding.UTF8,
            "application/x-www-form-urlencoded"
        );

        var response = await _client.PostAsync(url, content);
        if (response.IsSuccessStatusCode)
        {
            var result = JObject.Parse(await response.Content.ReadAsStringAsync());
            TokenManager.Instance.TokenExpirationTime = DateTime.Now.AddSeconds(result["expires_in"]!.ToObject<int>());
            TokenManager.Instance.BearerToken = result["access_token"]!.ToString();
            Log.Information("Bearer Token acquired: " + _bearerToken);
        }
        else
        {
            Log.Error(
                $"Failed to acquire bearer token. Status Code: {response.StatusCode}. Message: {response.ReasonPhrase}");
        }
    }


    public async Task<ApiResponse<T>?> GetDataAsync<T>(string url)
    {
        await UpdateAccessTokenAsync();

        if (string.IsNullOrEmpty(BearerToken))
        {
            Log.Error("Bearer Token is null or empty.");
            return null;
        }

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

        var response = await _client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            try
            {
                return JsonConvert.DeserializeObject<ApiResponse<T>>(json);
            }
            catch (JsonException ex)
            {
                Log.Error($"JSON parsing error: {ex.Message}");
                return null;
            }
        }

        Log.Error($"Failed to retrieve data. Status Code: {response.StatusCode}. Message: {response.ReasonPhrase}");
        return null;
    }



    public async Task<bool> UpdateDataAsync(string url, object obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        Dictionary<string, object> propertyDictionary = new Dictionary<string, object>();

        PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo property in properties)
        {
            if (property.CanRead)
            {
                propertyDictionary[property.Name] = property.GetValue(obj);
            }
        }
        var oDataEtag = (string)propertyDictionary["ODataEtag"];

   
        propertyDictionary.Remove("ODataEtag");

        if (propertyDictionary.ContainsKey("LastModifiedDateTime"))
        {
            propertyDictionary.Remove("LastModifiedDateTime");
        }
        if (propertyDictionary.ContainsKey("Id"))
        {
            propertyDictionary.Remove("Id");
        }
        if (propertyDictionary.ContainsKey("No"))
        {
            propertyDictionary.Remove("No");
        }


        return await  UpdateDataAsync(url, propertyDictionary, oDataEtag);
    }



    protected async Task<bool> UpdateDataAsync(string url, Dictionary<string, object> keyValuePairs, string eTag)
    {
        await UpdateAccessTokenAsync();

        if (string.IsNullOrEmpty(BearerToken))
        {
            Log.Error("Bearer Token is null or empty.");
            return false;
        }

        var jsonContent = JsonConvert.SerializeObject(keyValuePairs);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = content
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
        request.Headers.Add("If-Match", eTag);

        try
        {
            var response = await _client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Log.Information("Data successfully updated.");
                return true;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            if (responseContent != null)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>((string)responseContent);
                string errorMessage = errorResponse?.Error?.Message;
                Log.Error($"Failed to update data. Status Code: {response.StatusCode}. Message: {errorMessage}");
                // todo: improve error handeling // 
            }
            return false;
        }
        catch (Exception ex)
        {
            Log.Error($"Exception occurred while updating data. Exception: {ex.Message}");
            return false;
        }
    }





    protected async Task<bool> AddAsync(string url, Dictionary<string, object> keyValuePairs)
    {
        await UpdateAccessTokenAsync();

        if (string.IsNullOrEmpty(BearerToken))
        {
            Log.Error("Bearer Token is null or empty.");
            return false;
        }

        var jsonContent = JsonConvert.SerializeObject(keyValuePairs);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

        try
        {
            var response = await _client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Log.Information("Data successfully added.");
                return true;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            if (responseContent != null)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>((string) responseContent);
                string errorMessage = errorResponse?.Error?.Message;
                Log.Error($"Failed to add data. Status Code: {response.StatusCode}. Message: {errorMessage}");
                // todo: improve error handeling // 
            }
            return false;
        }
        catch (Exception ex)
        {
            Log.Error($"Exception occurred while Adding data. Exception: {ex.Message}");
            return false;
        }
    }



    public string FormatFilterValue(object value)
    {
        // Use pattern matching to determine the type of 'value' and format accordingly
        return value switch
        {
            // Convert boolean to lower case string
            bool boolValue => boolValue.ToString().ToLower(),

            // Enclose string values in single quotes
            string stringValue => $"'{stringValue}'",

            // Format DateTime values to ISO 8601 string format and enclose in single quotes
            DateTime dateValue => $"'{dateValue:yyyy-MM-ddTHH:mm:ssZ}'",

            // For any other type, call ToString(), handle null values gracefully
            _ => value?.ToString() ?? string.Empty
        };
    }
}