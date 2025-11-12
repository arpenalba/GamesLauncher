using GameLauncher.Models;
using GameLauncher.ViewModels;
using System;
using System.Windows;

namespace GameLauncher.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm;
            Loaded += async (_, _) => await _vm.InitializeAsync();
        }
                
        private async void OpenAddGames_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AddGamesDialog
            {
                Owner = this
            };
            dlg.ShowDialog();

            // After dialog closes, reload to reflect changes persisted to games.json
            await _vm.InitializeAsync();
        }

        private void GamesList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (GamesList.SelectedItem is Game selected)
                _vm.Launch(selected); // pick env dynamically later
        }
    }
}
