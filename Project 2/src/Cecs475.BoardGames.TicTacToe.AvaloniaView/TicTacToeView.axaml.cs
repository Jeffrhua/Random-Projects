using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Cecs475.BoardGames.AvaloniaView;

namespace Cecs475.BoardGames.TicTacToe.AvaloniaView;

public partial class TicTacToeView : UserControl, IAvaloniaGameView {
    public TicTacToeView()
    {
        InitializeComponent();
    }

    public Control ViewControl => this;

    public IGameViewModel ViewModel => (IGameViewModel)this.FindResource("vm")!;

	private void Panel_PointerEntered(object? sender, Avalonia.Input.PointerEventArgs e) {
		Panel b = (Panel)sender!;
		var square = (TicTacToeSquare)b.DataContext!;
		var vm = (TicTacToeViewModel)this.FindResource("vm")!;
		if (vm.PossibleMoves.Contains(square.Position)) {
			b.Background = Brushes.Red;
		}
	}

	private void Panel_PointerExited(object? sender, Avalonia.Input.PointerEventArgs e) {
		if (sender is not Panel b) { throw new ArgumentException(nameof(sender)); }
		b.Background = Brushes.Green;
	}

	private void Panel_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e) {
		Panel b = (Panel)sender!;
		var square = (TicTacToeSquare)b.DataContext!;
		var vm = (TicTacToeViewModel)this.FindResource("vm")!;
		if (vm.PossibleMoves.Contains(square.Position)) {
			vm.ApplyMove(square.Position);
			b.Background = Brushes.Green;
		}
	}
}