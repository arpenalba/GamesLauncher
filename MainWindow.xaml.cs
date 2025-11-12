using GameLauncher.Models;
using GameLauncher.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GameLauncher.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm = new();

        private Point _dragStartPoint;

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

        // Handle events for the drag handle placed in the DataTemplate
        private void Handle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        private void Handle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            var currentPos = e.GetPosition(null);
            var diff = currentPos - _dragStartPoint;
            if (Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance &&
                Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance)
                return;

            // Sender is the handle inside the item template; its DataContext is the Game.
            if (sender is FrameworkElement fe && fe.DataContext is Game game)
            {
                // Start drag/drop
                DragDrop.DoDragDrop(GamesList, new DataObject(typeof(Game), game), DragDropEffects.Move);
            }
        }

        private void GamesList_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(Game)))
                e.Effects = DragDropEffects.None;
            else
                e.Effects = DragDropEffects.Move;

            e.Handled = true;
        }

        private async void GamesList_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(Game))) return;

            var sourceGame = e.Data.GetData(typeof(Game)) as Game;
            if (sourceGame == null) return;

            // Find target item under mouse
            var pos = e.GetPosition(GamesList);
            var element = GamesList.InputHitTest(pos) as DependencyObject;
            var targetItem = FindAncestor<ListBoxItem>(element);
            Game targetGame = targetItem?.DataContext as Game;

            var oldIndex = _vm.Games.IndexOf(sourceGame);
            var newIndex = targetGame != null ? _vm.Games.IndexOf(targetGame) : _vm.Games.Count - 1;

            if (oldIndex < 0) return;

            await _vm.MoveGameAsync(oldIndex, newIndex);
        }

        // Utility helper to find ancestor of a given type in visual tree
        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T t) return t;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
