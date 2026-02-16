using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Cecs475.BoardGames.AvaloniaView;

namespace Cecs475.BoardGames.Othello.AvaloniaView;

public partial class OthelloView : UserControl, IAvaloniaGameView {
	public OthelloView() {
		InitializeComponent();

	}

	public OthelloViewModel OthelloViewModel => (OthelloViewModel)Resources["vm"]!;

	public Control ViewControl => this;

	public IGameViewModel ViewModel => OthelloViewModel;

	private void Panel_PointerEntered(object? sender, Avalonia.Input.PointerEventArgs e) {
		if (sender is not Control b) {
			throw new ArgumentException(nameof(sender));
		}
		var square = (OthelloSquare)b.DataContext!;
		var vm = (OthelloViewModel)Resources["vm"]!;
		if (vm.PossibleMoves.Contains(square.Position)) {
			square.IsHighlighted = true;
		}
	}

	private void Panel_PointerExited(object? sender, Avalonia.Input.PointerEventArgs e) {
		if (sender is not Control b) { throw new ArgumentException(nameof(sender)); }
		var square = (OthelloSquare)b.DataContext!;
		square.IsHighlighted = false;
	}

	private void Panel_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e) {
		if (sender is not Control b) {
			throw new ArgumentException(nameof(sender));
		}
		var square = (OthelloSquare)b.DataContext!;
		var vm = (OthelloViewModel)Resources["vm"]!;
		if (vm.PossibleMoves.Contains(square.Position)) {
			vm.ApplyMove(square.Position);
			square.IsHighlighted = false;
		}

		if (!vm.PossibleMoves.Any()) {
			//MessageBoxManager
		}
	}
}