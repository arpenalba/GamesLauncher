using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameLauncher.Services
{
    public static class JsonRepository
    {
        public static async Task<List<T>> LoadAsync<T>(string path)
        {
            if (!File.Exists(path))
                return new List<T>();

            using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<List<T>>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<T>();
        }

        public static async Task SaveAsync<T>(string path, List<T> data)
        {
            using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, data, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
