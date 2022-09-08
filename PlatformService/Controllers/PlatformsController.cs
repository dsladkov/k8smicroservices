using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo _repository;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _commandDataClient;
        private readonly IMessageBusClient _messageBusClient;

        public PlatformsController(IPlatformRepo repository, IMapper mapper, ICommandDataClient commandDataClient, IMessageBusClient messageBusClient)
        {
            _repository = repository;
            _mapper = mapper;
            _commandDataClient = commandDataClient;
            _messageBusClient = messageBusClient;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            var platformItems = _repository.GetAllPlatforms();

            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platformItems));
        }

        [HttpGet("{id}", Name = "GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int id)
        {
            var platform = _repository.GetPlatformById(id);
            if (platform != null)
            {
                return Ok(_mapper.Map<Platform>(platform));
            }
            else return NotFound();
        }
        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platformCreateDto)
        {
            var platformModel = _mapper.Map<Platform>(platformCreateDto);
            _repository.CreatePlatform(platformModel);
            var result = _repository.SaveChanges();

            var platformReadDto = _mapper.Map<PlatformReadDto>(platformModel);
            //Send sync message
            try
            {
                await _commandDataClient.SendPlatformToCommand(platformReadDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not  send synchronously: {ex.Message}");
            }
            //Send Async Message
            try
            {
                var platformPublushedDto = _mapper.Map<PlatformPublishedDto>(platformReadDto);
                platformPublushedDto.Event = "Platform_Published";
                _messageBusClient.PublishNewPlatform(platformPublushedDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not  send asynchronously: {ex.Message}");
            }
            if (result == false) return BadRequest();
            return CreatedAtRoute(nameof(GetPlatformById), new { Id = platformReadDto.Id }, platformReadDto);
        }
    }
}