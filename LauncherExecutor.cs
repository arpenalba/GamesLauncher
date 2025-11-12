using GameLauncher.Models;
using IWshRuntimeLibrary;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace GameLauncher.Services
{
    public static class LauncherExecutor
    {
        public static void Launch(Game game, LaunchEnvironmentLink envLink, LaunchType launchType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(envLink?.Path))
                    throw new InvalidOperationException("Launch path is empty.");

                string command = launchType.LaunchCommand?.Trim() ?? string.Empty;
                string parameters = launchType.LaunchParams?.Trim() ?? string.Empty;

                // Replace placeholders
                command = command.Replace("<path>", envLink.Path);
                parameters = parameters.Replace("<path>", envLink.Path);

                var psi = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = parameters,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LauncherExecutor] Launch failed for {game?.Title}: {ex.Message}");
                System.Windows.MessageBox.Show($"Failed to launch {game?.Title}\n{ex.Message}",
                    "Launch Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string ResolveShortcut(string path)
        {
            if (!path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                return path;

            var shell = new WshShell();
            var shortcut = (IWshShortcut)shell.CreateShortcut(path);
            return shortcut.TargetPath;
        }
    }
}
