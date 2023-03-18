using AutoMapper;
using MagicVilla_API.Models;
using MagicVilla_API.Models.Dto;
using MagicVilla_API.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers.v2
{
    //[Route("api/VillaNumberAPI")]

    [ApiController]
    [Route("api/v{version:apiversion}/[controller]")]
    [ApiVersion("2")]
    public class VillaNumberAPIController : ControllerBase
    {
        public readonly IVillaNumberRepository villaNumberRepository;
        public readonly IVillaRepository villaRepository;
        public readonly IMapper mapper;
        protected APIResponse response;



        public VillaNumberAPIController(IVillaNumberRepository villaNumberRepository, IMapper mapper, IVillaRepository villaRepository)
        {
            this.villaNumberRepository = villaNumberRepository;
            this.mapper = mapper;
            this.response = new();
            this.villaRepository = villaRepository;
        }


        [HttpGet("{villaNo:int}", Name = "GetVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> GetVillaNumber(int villaNo)
        {
            try
            {
                if (villaNo == 0)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    return BadRequest(response);
                }

                var villaNumber = await villaNumberRepository.GetAsync(u => u.VillaNo == villaNo);

                if (villaNumber == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    return NotFound(response);
                }

                response.Result = mapper.Map<VillaNumberDTO>(villaNumber);
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return response;
        }
    }
}
