using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Cecs475.BoardGames.AvaloniaView;
using Cecs475.BoardGames.Chess.AvaloniaView;
using Cecs475.BoardGames.Othello.AvaloniaView;
using Cecs475.BoardGames.TicTacToe.AvaloniaView;
using SkiaSharp;

namespace Cecs475.BoardGames.AvaloniaApp;

public partial class GameChoiceWindow : Window
{
    public GameChoiceWindow()
    {
        InitializeComponent();
        this.GamesList.ItemsSource = new IAvaloniaGameFactory[] {
			new ChessGameFactory(),
			new OthelloGameFactory(),
            new TicTacToeGameFactory()
        };
    }

	private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        // Retrieve the factory bound to the button.
        IAvaloniaGameFactory factory = (IAvaloniaGameFactory)((Button)sender!).DataContext!;

        // Create a new game window with the factory.
        var gameWindow = new MainWindow(factory);

        // Hide this window, and reveal it when the game window is closed.
        gameWindow.Closed += (_, _) => {
            this.IsVisible = true;
        };
        this.IsVisible = false;
        gameWindow.Show();
	}
}