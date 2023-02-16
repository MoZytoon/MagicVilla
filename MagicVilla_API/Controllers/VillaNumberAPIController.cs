using AutoMapper;
using Azure;
using MagicVilla_API.Models;
using MagicVilla_API.Models.Dto;
using MagicVilla_API.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/[controller]")]
    //[Route("api/VillaNumberAPI")]
    [ApiController]
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




        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetVillaNumbers()
        {
            try
            {
                //logging.Log("get all villas .","info");
                IEnumerable<VillaNumber> villaNumberList = await villaNumberRepository.GetAllAsync(includes:"Villa");
                response.Result = mapper.Map<List<VillaNumberDTO>>(villaNumberList);
                response.StatusCode = System.Net.HttpStatusCode.OK;

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessages = new List<string> { ex.Message };
            }

            return response;
        }




        [HttpGet("{villaNo:int}", Name = "GetVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetVillaNumber(int villaNo)
        {
            try
            {
                if (villaNo == 0)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(response);
                }

                var villaNumber = await villaNumberRepository.GetAsync(u => u.VillaNo == villaNo);

                if (villaNumber == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(response);
                }

                response.Result = mapper.Map<VillaNumberDTO>(villaNumber);
                response.StatusCode = HttpStatusCode.OK;

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessages = new List<string>() { ex.ToString() };
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
                    return BadRequest(ModelState);
                }

                if (await villaRepository.GetAsync(u => u.Id == villaNumberCreateDTO.VillaId) == null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa ID is Invalid!");
                    return BadRequest(ModelState);
                }

                if (villaNumberCreateDTO == null)
                {
                    return BadRequest(villaNumberCreateDTO);
                }

                if (villaNumberCreateDTO.VillaNo < 1)
                {
                    ModelState.AddModelError("ErrorMessages", "VillaNo must be greater than 0!");
                    return BadRequest(ModelState);
                }

                VillaNumber villaNumber = mapper.Map<VillaNumber>(villaNumberCreateDTO);

                await villaNumberRepository.CreateAsync(villaNumber);
                response.Result = mapper.Map<VillaNumberDTO>(villaNumber);
                response.StatusCode = HttpStatusCode.Created;

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
                    return BadRequest();
                }

                var villaNumber = await villaNumberRepository.GetAsync(u => u.VillaNo == villaNo);

                if (villaNumber == null)
                {
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
                    return BadRequest();
                }

                if (await villaRepository.GetAsync(u => u.Id == villaNumberUpdateDTO.VillaId) == null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa ID is Invalid!");
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
                    response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    response.ErrorMessages = new List<string>() { "jsonPatchDocument or id is null" };
                    return BadRequest(response);
                }

                if (!ModelState.IsValid)
                {
                    response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    response.ErrorMessages = new List<string>() { "Model is not valid" };
                    return BadRequest(ModelState);
                }

                var villaNumber = await villaNumberRepository.GetAsync(x => x.VillaNo == villaNo,false);

                if (villaNumber == null)
                {
                    response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    return NotFound(response);
                }

                if (villaNumber != null)
                {
                    if (await villaRepository.GetAsync(x => x.Id == villaNumber.VillaId) == null)
                    {
                        response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                        response.IsSuccess = false;
                        response.ErrorMessages = new List<string> { "Villa is not Exists!" };
                        return BadRequest(response);
                    }
                }

                VillaNumberUpdateDTO villaNumberUpdateDTO = mapper.Map<VillaNumberUpdateDTO>(villaNumber);
                jsonPatchDocument.ApplyTo(villaNumberUpdateDTO, ModelState);
                villaNumber = mapper.Map<VillaNumber>(villaNumberUpdateDTO);
                await villaNumberRepository.UpdateAsync(villaNumber);
                response.StatusCode = System.Net.HttpStatusCode.NoContent;
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
