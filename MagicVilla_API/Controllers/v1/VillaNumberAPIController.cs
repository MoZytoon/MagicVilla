using AutoMapper;
using MagicVilla_API.Models;
using MagicVilla_API.Models.Dto;
using MagicVilla_API.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_API.Controllers.v1
{
    //[Route("api/VillaNumberAPI")]

    [ApiController]
    [Route("api/v{version:apiversion}/[controller]")]
    [ApiVersion("1")]
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
            response = new();
            this.villaRepository = villaRepository;
        }




        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetVillaNumbers()
        {
            try
            {
                //logging.Log("get all villas .","info");
                IEnumerable<VillaNumber> villaNumberList = await villaNumberRepository.GetAllAsync(includes: "Villa");
                response.Result = mapper.Map<List<VillaNumberDTO>>(villaNumberList);
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessages = new List<string> { ex.Message };
            }

            return response;
        }




        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> CreateVillaNumber([FromBody] VillaNumberCreateDTO villaNumberCreateDTO)
        {
            try
            {
                if (await villaNumberRepository.GetAsync(u => u.VillaNo == villaNumberCreateDTO.VillaNo) != null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa Number already Exists!");
                    response.IsSuccess = false;
                    return BadRequest(ModelState);
                }

                if (await villaRepository.GetAsync(u => u.Id == villaNumberCreateDTO.VillaId) == null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa ID is Invalid!");
                    response.IsSuccess = false;
                    return BadRequest(ModelState);
                }

                if (villaNumberCreateDTO == null)
                {
                    response.IsSuccess = false;
                    return BadRequest(villaNumberCreateDTO);
                }

                if (villaNumberCreateDTO.VillaNo < 1)
                {
                    ModelState.AddModelError("ErrorMessages", "VillaNo must be greater than 0!");
                    response.IsSuccess = false;
                    return BadRequest(ModelState);
                }

                VillaNumber villaNumber = mapper.Map<VillaNumber>(villaNumberCreateDTO);

                await villaNumberRepository.CreateAsync(villaNumber);
                response.Result = mapper.Map<VillaNumberDTO>(villaNumber);
                response.StatusCode = HttpStatusCode.Created;
                response.IsSuccess = true;

                return CreatedAtRoute("GetVilla", new { id = villaNumber.VillaId }, response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return response;
        }




        [HttpDelete("{villaNo:int}", Name = "DeleteVillaNumber")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> DeleteVilla(int villaNo)
        {
            try
            {
                if (villaNo == 0)
                {
                    response.IsSuccess = false;
                    return BadRequest();
                }

                var villaNumber = await villaNumberRepository.GetAsync(u => u.VillaNo == villaNo);

                if (villaNumber == null)
                {
                    response.IsSuccess = false;
                    return NotFound();
                }

                await villaNumberRepository.RemoveAsync(villaNumber);
                response.StatusCode = HttpStatusCode.NoContent;
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




        [HttpPut("{villaNo:int}", Name = "UpdateVillaNumber")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> UpdateVillaNumber(int villaNo, [FromBody] VillaNumberUpdateDTO villaNumberUpdateDTO)
        {
            try
            {
                if (villaNumberUpdateDTO == null || villaNo != villaNumberUpdateDTO.VillaNo)
                {
                    response.IsSuccess = false;
                    return BadRequest();
                }

                if (await villaRepository.GetAsync(u => u.Id == villaNumberUpdateDTO.VillaId) == null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa ID is Invalid!");
                    response.IsSuccess = false;
                    return BadRequest(ModelState);
                }

                VillaNumber model = mapper.Map<VillaNumber>(villaNumberUpdateDTO);

                await villaNumberRepository.UpdateAsync(model);
                response.StatusCode = HttpStatusCode.NoContent;
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




        [HttpPatch("{villaNo:int}", Name = "UpdatePartialVillaNumber")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePartialVillaNumber(int villaNo, JsonPatchDocument<VillaNumberUpdateDTO> jsonPatchDocument)
        {
            try
            {
                if (jsonPatchDocument == null || villaNo == 0)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    response.ErrorMessages = new List<string>() { "jsonPatchDocument or id is null" };
                    return BadRequest(response);
                }

                if (!ModelState.IsValid)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    response.ErrorMessages = new List<string>() { "Model is not valid" };
                    return BadRequest(ModelState);
                }

                var villaNumber = await villaNumberRepository.GetAsync(x => x.VillaNo == villaNo, false);

                if (villaNumber == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    return NotFound(response);
                }

                if (villaNumber != null)
                {
                    if (await villaRepository.GetAsync(x => x.Id == villaNumber.VillaId) == null)
                    {
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.IsSuccess = false;
                        response.ErrorMessages = new List<string> { "Villa is not Exists!" };
                        return BadRequest(response);
                    }
                }

                VillaNumberUpdateDTO villaNumberUpdateDTO = mapper.Map<VillaNumberUpdateDTO>(villaNumber);
                jsonPatchDocument.ApplyTo(villaNumberUpdateDTO, ModelState);
                villaNumber = mapper.Map<VillaNumber>(villaNumberUpdateDTO);
                await villaNumberRepository.UpdateAsync(villaNumber);
                response.StatusCode = HttpStatusCode.NoContent;
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessages = new List<string> { ex.Message };
            }

            return Ok(response);
        }
    }
}
