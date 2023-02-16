using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace MagicVilla_Web.Services
{
    public class BaseService : IBaseServices
    {
        public APIResponse APIResponse { get; set; }
        public IHttpClientFactory httpClient { get; set; }

        public BaseService(IHttpClientFactory httpClient)
        {
            this.APIResponse = new();
            this.httpClient = httpClient;
        }

        public async Task<T> SendAsync<T>(APIRequest apiRequest)
        {
            try
            {
                var client = httpClient.CreateClient("MagicAPI");
                HttpRequestMessage request = new HttpRequestMessage();
                request.Headers.Add("Accept", "application/json");
                request.RequestUri = new Uri(apiRequest.Url);

                if (apiRequest.Data != null)
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(apiRequest.Data), Encoding.UTF8, "application/json");
                }

                switch (apiRequest.ApiType)
                {
                    case SD.ApiType.POST:
                        request.Method = HttpMethod.Post;
                        break;
                    case SD.ApiType.PUT:
                        request.Method = HttpMethod.Put;
                        break;
                    case SD.ApiType.DELETE:
                        request.Method = HttpMethod.Delete;
                        break;
                    default:
                        request.Method = HttpMethod.Get;
                        break;
                }

                HttpResponseMessage response = null;

                if (!string.IsNullOrEmpty(apiRequest.Token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiRequest.Token);
                }

                response = await client.SendAsync(request);
                var apiContent = await response.Content.ReadAsStringAsync();

                try
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(apiContent);

                    if ((response.StatusCode == System.Net.HttpStatusCode.BadRequest || response.StatusCode == System.Net.HttpStatusCode.NotFound))
                    {
                        apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                        apiResponse.IsSuccess = false;
                        var apiResponseSerialized = JsonConvert.SerializeObject(apiResponse);
                        var apiResponseDeserialized = JsonConvert.DeserializeObject<T>(apiResponseSerialized);

                        return apiResponseDeserialized;
                    }
                }
                catch (Exception e)
                {
                    var exceptionResponse = JsonConvert.DeserializeObject<T>(apiContent);
                    return exceptionResponse;
                }

                var APIResponse = JsonConvert.DeserializeObject<T>(apiContent);

                return APIResponse;
            }
            catch (Exception e)
            {
                var dto = new APIResponse
                {
                    ErrorMessages = new List<string> { Convert.ToString(e.Message) },
                    IsSuccess = false
                };

                var res = JsonConvert.SerializeObject(dto);
                var apiResponse = JsonConvert.DeserializeObject<T>(res);

                return apiResponse;
            }
        }
    }
}
