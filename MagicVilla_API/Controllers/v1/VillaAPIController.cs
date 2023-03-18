using AutoMapper;
using MagicVilla_API.Models;
using MagicVilla_API.Models.Dto;
using MagicVilla_API.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Text.Json;

namespace MagicVilla_API.Controllers.v1
{
    //[Route("api/VillaAPI")]

    [ApiController]
    [Route("api/v{version:apiversion}/[controller]")]
    [ApiVersion("1")]
    public class VillaAPIController : ControllerBase
    {
        public readonly IVillaRepository villaRepository;
        public readonly IMapper mapper;
        protected APIResponse response;



        public VillaAPIController(IVillaRepository villaRepository, IMapper mapper)
        {
            this.villaRepository = villaRepository;
            this.mapper = mapper;
            response = new();
        }



        [HttpGet]
        [Authorize]
        [ResponseCache(Duration = 30)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<APIResponse>>> GetVillas([FromQuery(Name = "filterOccupancy")] int? occupancy
                                                                            , [FromQuery] string? search, int pageSize = 0, int pageIndex = 1)
        {
            try
            {
                //logging.Log("get all villas .","info");
                IEnumerable<Villa> villaList;

                if (occupancy != null)
                {
                    villaList = await villaRepository.GetAllAsync(u => u.Occupancy == occupancy, pageSize: pageSize, pageIndex: pageIndex);
                }
                else
                {
                    villaList = await villaRepository.GetAllAsync(pageSize: pageSize, pageIndex: pageIndex);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    villaList = villaList.Where(u => u.Name.ToLower().Contains(search));
                }

                Pagination pagination = new() { Pagesize = pageSize, PageIndex = pageIndex};
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(pagination));

                response.Result = mapper.Map<List<VillaDTO>>(villaList);
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessages = new List<string> { ex.Message };
            }

            return Ok(response);
        }




        [HttpGet("{id:int}", Name = "GetVilla")]
        [Authorize(Roles = "admin")]
        [ResponseCache(CacheProfileName = "Default30")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status200OK,Type = typeof(VillaDTO))]
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    //logging.Log("error in Get villa , id is ZERO .","error");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    return BadRequest(response);
                }

                var villa = await villaRepository.GetAsync(x => x.Id == id);

                if (villa == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    return NotFound(response);
                }

                response.Result = mapper.Map<VillaDTO>(villa);
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessages = new List<string> { ex.Message };
            }

            return Ok(response);
        }




        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaCreateDTO villaDTO)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    return BadRequest(ModelState);
                //}

                if (await villaRepository.GetAsync(x => x.Name.ToLower() == villaDTO.Name.ToLower()) != null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa already Exists!");
                    response.IsSuccess = false;
                    return BadRequest(ModelState);
                }

                if (villaDTO == null)
                {
                    response.IsSuccess = false;
                    return BadRequest(villaDTO);
                }

                Villa villa = mapper.Map<Villa>(villaDTO);
                await villaRepository.CreateAsync(villa);
                response.Result = mapper.Map<VillaDTO>(villa);
                response.StatusCode = HttpStatusCode.Created;
                response.IsSuccess = true;

                return CreatedAtRoute("GetVilla", new { id = villa.Id }, response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessages = new List<string> { ex.Message };
            }

            return response;
        }




        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [Authorize(Roles = "CUSTOM")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    response.IsSuccess = false;
                    return BadRequest();
                }

                var villa = await villaRepository.GetAsync(u => u.Id == id);

                if (villa == null)
                {
                    response.IsSuccess = false;
                    return NotFound();
                }

                await villaRepository.RemoveAsync(villa);
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




        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO villaDTO)
        {
            try
            {
                if (villaDTO == null || id != villaDTO.Id)
                {
                    response.IsSuccess = false;
                    return BadRequest();
                }

                Villa model = mapper.Map<Villa>(villaDTO);
                await villaRepository.UpdateAsync(model);
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




        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> jsonPatchDocument)
        {
            if (jsonPatchDocument == null || id == 0)
            {
                response.IsSuccess = false;
                return BadRequest();
            }

            var villa = await villaRepository.GetAsync(u => u.Id == id, tracked: false);
            VillaUpdateDTO villaDTO = mapper.Map<VillaUpdateDTO>(villa);

            if (villa == null)
            {
                response.IsSuccess = false;
                return BadRequest();
            }

            jsonPatchDocument.ApplyTo(villaDTO, ModelState);
            Villa model = mapper.Map<Villa>(villaDTO);
            await villaRepository.UpdateAsync(model);

            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                return BadRequest(ModelState);
            }

            response.IsSuccess = true;
            return NoContent();
        }
    }
}
