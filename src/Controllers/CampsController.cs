using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _campRepository;
        private readonly LinkGenerator _linkGenerator;
        private readonly IMapper _mapper;
        public CampsController(ICampRepository campRepository, LinkGenerator linkGenerator, IMapper mapper)
        {
            _campRepository = campRepository;
            _linkGenerator = linkGenerator;
            _mapper = mapper;
            
        }


        [HttpGet]
        public async Task<IActionResult> GetCamps(bool includeTalks = false)
        {
            try
            {
                var results = await _campRepository.GetAllCampsAsync(includeTalks);

                return Ok(_mapper.Map<CampModel[]>(results));
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

        }


        [HttpGet("{moniker}")]
        public async Task<IActionResult> GetCamp(string moniker)
        {
            try
            {
                var result = await _campRepository.GetCampAsync(moniker);

                if (result == null) return NotFound();

                return Ok(_mapper.Map<CampModel>(result));
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchByDate(DateTime eventDate, bool includeTalks = false)
        {
            try
            {
                var results = await _campRepository.GetAllCampsByEventDate(eventDate, includeTalks);

                if (!results.Any()) { return NotFound(); }

                return Ok(_mapper.Map<CampModel[]>(results));

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Post(CampModel model)
        {
            try
            {
                var location = _linkGenerator.GetPathByAction("GetCamp", "Camps", new { moniker = model.Moniker });

                if (string.IsNullOrWhiteSpace(location)) { return BadRequest("Could not use current moniker."); }

                
                var camp = _mapper.Map<Camp>(model);

                _campRepository.Add(camp);
                
                var saved = await _campRepository.SaveChangesAsync();


                if (!saved) return BadRequest();

                return Created(location,_mapper.Map<CampModel>(camp));
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
    }
}
