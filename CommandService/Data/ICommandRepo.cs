using System.Collections.Generic;
using CommandService.Models;

namespace CommandService.Data
{
    public interface ICommandRepo
    {
        bool SaveChanges();

        //Platforms
        IEnumerable<Platform> GetAllPlatforms();
        void CreatePlatform(Platform plat);
        bool PlatformExists(int platformId);
        bool ExternalPlatformExists(int externalPlatformId);

        //Commands
        IEnumerable<Command> GetCommadsForPlatform(int platformId);
        Command GetCommand(int platformId, int command);
        void CreateCommand(int platformId, Command command);

    }
}