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
    [Route("api/camps/{moniker}/talks")]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _campRepository;
        private readonly LinkGenerator _linkGenerator;
        private readonly IMapper _mapper;

        public TalksController(ICampRepository campRepository, LinkGenerator linkGenerator, IMapper mapper)
        {
            _campRepository = campRepository;
            _linkGenerator = linkGenerator;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string moniker)
        {
            try
            {
                var result = await _campRepository.GetTalksByMonikerAsync(moniker, true);

                return Ok(_mapper.Map<TalkModel[]>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{talkId:int}")]
        public async Task<IActionResult> GetTalk(string moniker, int talkId)
        {
            try
            {
                var result = await _campRepository.GetTalkByMonikerAsync(moniker, talkId, true);

                return Ok(_mapper.Map<TalkModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Post(string moniker, TalkModel model)
        {
            try
            {
                var camp = await _campRepository.GetCampAsync(moniker);
                if (camp == null) return BadRequest("camp does not exist.");

                if (model.Speaker == null) return BadRequest("Speaker Id is required.");
                var speaker = await _campRepository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null) return BadRequest("Speaker could not be found.");

                var talk = _mapper.Map<Talk>(model);
                talk.Camp = camp;
                talk.Speaker = speaker;
                
                _campRepository.Add(talk);
                var saved = await _campRepository.SaveChangesAsync();

                if (!saved) return BadRequest("Failed to Create Camp.");

                var location = _linkGenerator.GetPathByAction("GetTalk", "Talks", new { moniker, talkId = talk.TalkId });

                return Created(location, _mapper.Map<TalkModel>(talk));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
    }
}
