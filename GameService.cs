using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GameLauncher.Models;
using System.Diagnostics;

namespace GameLauncher.Services
{
    public class GameService
    {

        public List<Game> Games { get; private set; } = new();
        public List<LaunchEnvironment> Environments { get; private set; } = new();
        public List<LaunchType> LaunchTypes { get; private set; } = new();

        public async Task LoadAllAsync()
        {
            var gamesPath = AppPaths.GamesJsonPath;
            var envPath = AppPaths.EnvironmentsJsonPath;
            var typesPath = AppPaths.LaunchTypesJsonPath;

            Debug.WriteLine($"[GameService] Loading games from: {gamesPath}");
            Debug.WriteLine($"[GameService] Loading environments from: {envPath}");
            Debug.WriteLine($"[GameService] Loading launch types from: {typesPath}");

            // Load games and ensure they are ordered by Order value
            Games = (await JsonRepository.LoadAsync<Game>(gamesPath))
                        .OrderBy(g => g.Order)
                        .ToList();

            Environments = await JsonRepository.LoadAsync<LaunchEnvironment>(envPath);
            LaunchTypes = await JsonRepository.LoadAsync<LaunchType>(typesPath);
        }

        public LaunchType GetLaunchTypeById(string id)
        {
            return LaunchTypes.FirstOrDefault(t => t.Id.ToString() == id);
        }

        public async Task SaveGamesAsync()
        {
            var gamesPath = AppPaths.GamesJsonPath;
            // Ensure the service list is written as-is to disk.
            await JsonRepository.SaveAsync<Game>(gamesPath, Games);
        }
    }
}
