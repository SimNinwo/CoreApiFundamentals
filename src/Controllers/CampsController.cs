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
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
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
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
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
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Post(CampModel model)
        {
            try
            {
                var itExists = await _campRepository.GetCampAsync(model.Moniker);
                if (itExists != null) { return BadRequest("Moniker already in use."); }

                var location = _linkGenerator.GetPathByAction("GetCamp", "Camps", new { moniker = model.Moniker });
                if (string.IsNullOrWhiteSpace(location)) { return BadRequest("Could not use current moniker."); }
                
                var camp = _mapper.Map<Camp>(model);

                _campRepository.Add(camp);
                
                var saved = await _campRepository.SaveChangesAsync();

                if (!saved) return BadRequest();

                return Created(location, _mapper.Map<CampModel>(camp));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }


        [HttpPut]
        public async Task<IActionResult> Put(string moniker, CampModel model)
        {
            try
            {
                var oldCamp = await _campRepository.GetCampAsync(model.Moniker);
                if (oldCamp == null) { return NotFound($"Could not find camp with Moniker {moniker}."); }

                _mapper.Map(model, oldCamp);

                var saved = await _campRepository.SaveChangesAsync();

                if (!saved) return BadRequest();

                return Ok(_mapper.Map<CampModel>(oldCamp));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
    }
}
