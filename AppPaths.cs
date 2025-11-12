using System;
using System.IO;
using System.Linq;

namespace GameLauncher.Services
{
    public static class AppPaths
    {
        // Public, static, resolved paths for use across the app
        public static string GamesJsonPath { get; } = ResolvePath("games.json");
        public static string EnvironmentsJsonPath { get; } = ResolvePath("environments.json");
        public static string LaunchTypesJsonPath { get; } = ResolvePath("launchTypes.json");

        // Public helper in case other callers need to resolve custom filenames
        public static string ResolvePath(string filename)
        {
            var baseDir = AppContext.BaseDirectory ?? AppDomain.CurrentDomain.BaseDirectory;
            var currentDir = Directory.GetCurrentDirectory();

            var solutionRootFromBase = FindSolutionRoot(baseDir);
            var solutionRootFromCurrent = FindSolutionRoot(currentDir);

            var candidates = new[]
            {
                Path.Combine(baseDir, filename),
                Path.Combine(baseDir, "Data", filename),
                Path.Combine(currentDir, filename),
                solutionRootFromBase != null ? Path.Combine(solutionRootFromBase, filename) : null,
                solutionRootFromCurrent != null ? Path.Combine(solutionRootFromCurrent, filename) : null,
                ""
            }.Where(p => !string.IsNullOrEmpty(p)).ToArray();

            var found = candidates.FirstOrDefault(File.Exists);
            return found ?? candidates[0]; // if none exists, return primary location (app base) so Save will write there
        }

        private static string? FindSolutionRoot(string startDirectory)
        {
            try
            {
                var dir = new DirectoryInfo(startDirectory);
                while (dir != null)
                {
                    var slnFiles = dir.GetFiles("*.sln");
                    if (slnFiles != null && slnFiles.Length > 0)
                        return dir.FullName;

                    dir = dir.Parent;
                }
            }
            catch
            {
                // ignore IO errors and return null so other candidates are used
            }

            return null;
        }
    }
}