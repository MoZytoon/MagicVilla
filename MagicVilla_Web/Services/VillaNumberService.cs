using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;
using Newtonsoft.Json.Linq;

namespace MagicVilla_Web.Services
{
    public class VillaNumberService : BaseService , IVillaNumberService
    {
        private readonly IHttpClientFactory clientFactory;
        private string villaUrl;

        public VillaNumberService(IHttpClientFactory clientFactory, IConfiguration configuration) : base(clientFactory)
        {
            this.clientFactory = clientFactory;
            this.villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");
        }

        public Task<T> CreateAsync<T>(VillaNumberCreateDTO villaNumberCreateDTO, string token)
        {
            return SendAsync<T>(new APIRequest
            {
                ApiType = SD.ApiType.POST,
                Data = villaNumberCreateDTO,
                Url = this.villaUrl + "/api/v1/VillaNumberAPI",
                Token = token
            });
        }

        public Task<T> DeleteAsync<T>(int id, string token)
        {
            return SendAsync<T>(new APIRequest
            {
                ApiType = SD.ApiType.DELETE,
                Url = this.villaUrl + "/api/v1/VillaNumberAPI/" + id,
                Token = token
            });
        }

        public Task<T> GetAllAsync<T>(string token)
        {
            return SendAsync<T>(new APIRequest
            {
                ApiType = SD.ApiType.GET,
                Url = this.villaUrl + "/api/v1/VillaNumberAPI",
                Token = token
            });
        }

        public Task<T> GetAsync<T>(int id, string token)
        {
            return SendAsync<T>(new APIRequest
            {
                ApiType = SD.ApiType.GET,
                Url = this.villaUrl + "/api/v2/VillaNumberAPI/" + id,
                Token = token
            });
        }

        public Task<T> UpdateAsync<T>(VillaNumberUpdateDTO villaNumberUpdateDTO, string token)
        {
            return SendAsync<T>(new APIRequest
            {
                ApiType = SD.ApiType.PUT,
                Data = villaNumberUpdateDTO,
                Url = this.villaUrl + "/api/v1/VillaNumberAPI/" + villaNumberUpdateDTO.VillaNo,
                Token = token
            });
        }
    }
}
