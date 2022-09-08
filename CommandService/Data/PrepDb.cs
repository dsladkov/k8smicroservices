using System;
using System.Collections.Generic;
using CommandService.Data;
using CommandService.Models;
using CommandService.SyncDataService.Grpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CommanService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var grpcClient = serviceScope.ServiceProvider.GetService<IPlatformDataClient>();
                var platforms = grpcClient.ReturnAllPlatform();

                SeedData(serviceScope.ServiceProvider.GetService<ICommandRepo>(), platforms);
            }
        }

        private static void SeedData(ICommandRepo repo, IEnumerable<Platform> platforms)
        {
            Console.WriteLine($"--> Seeding new platforms...");
            foreach (var platform in platforms)
            {
                if (!repo.ExternalPlatformExists(platform.ExternalID)) 
                    repo.CreatePlatform(platform);
                repo.SaveChanges();
            }
        }
    }
}