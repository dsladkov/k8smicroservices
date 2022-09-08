using System;
using System.Text.Json;
using AutoMapper;
using CommandService.Data;
using CommandService.Dto;
using CommandService.Dtos;
using CommandService.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CommandService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFatory;
        private readonly IMapper _mapper;

        public EventProcessor(IServiceScopeFactory scopeFactory, IMapper mapper)
        {
            _scopeFatory = scopeFactory;
            _mapper = mapper;
        }
        public void ProcessEvent(string message)
        {
            var eventType = DetermeneEvent(message);
            switch (eventType)
            {
                case EventType.PlatformPublished:
                    AddPlatform(message);
                    break;
                default:
                    break;
            }
        }

        private EventType DetermeneEvent(string notificationMessage)
        {
            System.Console.WriteLine($"--> Determining Event");
            //var result = _mapper.Map<GenericEventDto>(notificationMessage);
            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);
            switch (eventType.Event)
            {
                case "Platform_Published":
                    System.Console.WriteLine($"--> Platform Published Event Detected");
                    return EventType.PlatformPublished;
                default:
                    System.Console.WriteLine($"--> Could not determine evenet type");
                    return EventType.Undetermined;
            }
        }
        private void AddPlatform(string platformPublishedMessage)
        {
            using (var scope = _scopeFatory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();
                var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);
                try
                {
                    var plat = _mapper.Map<Platform>(platformPublishedDto);
                    if (!repo.ExternalPlatformExists(plat.ExternalID))
                    {
                        repo.CreatePlatform(plat);
                        repo.SaveChanges();
                        System.Console.WriteLine($"--> Platform added");
                    }
                    else
                    {
                        System.Console.WriteLine($"--> Platform already exists");
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"--> Could not add Platform to Db {ex.Message}");
                }
            }
        }
    }
    enum EventType
    {
        PlatformPublished,
        Undetermined
    }
}