using Avalonia.Controls;
using Avalonia.Data;
using Cecs475.BoardGames.AvaloniaView;
using MsBox.Avalonia;

namespace Cecs475.BoardGames.AvaloniaApp;

public partial class MainView : UserControl
{
	public IAvaloniaGameFactory GameFactory {
		set {
			var ov = value.CreateGameView();
			Resources.Add("GameView", ov);
			Resources.Add("ViewModel", ov.ViewModel);

			ov.ViewModel.GameFinished += ViewModel_GameFinished;

			// Set up bindings manually -- there are ways to do this in XAML, but I want to demonstrate the C# equivalent. 
			mAdvantageLabel.Bind(Label.ContentProperty,
				new Binding() {
					Path = "BoardAdvantage",
					Converter = value.CreateBoardAdvantageConverter()
				}
			);

			mPlayerLabel.Bind(Label.ContentProperty,
				new Binding() {
					Path = "CurrentPlayer",
					Converter = value.CreateCurrentPlayerConverter()
				}
			);
		}
	}

    public MainView()
    {
        InitializeComponent();
		
	}

	private void ViewModel_GameFinished(object? sender, System.EventArgs e) {
		var message = MessageBoxManager.GetMessageBoxStandard("Game over!", "Game over!");
		message.ShowAsPopupAsync(this);
	}

	private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        var vm = (IGameViewModel)Resources["ViewModel"]!;
        vm.UndoMove();
	}
}
