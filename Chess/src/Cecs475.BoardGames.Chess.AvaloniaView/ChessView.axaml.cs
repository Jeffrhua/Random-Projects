using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Cecs475.BoardGames.AvaloniaView;
using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cecs475.BoardGames.Chess.AvaloniaView;

public partial class ChessView : UserControl, IAvaloniaGameView {
	public ChessView() {
		InitializeComponent();
	}
    public ChessViewModel ChessViewModel => (ChessViewModel)ViewModel;

    public Control ViewControl => this;

	public IGameViewModel ViewModel => (IGameViewModel)this.FindResource("vm")!;

    // Add the highlight when a piece is selected (Hovered over)
    private void Panel_PointerEntered(object? sender, Avalonia.Input.PointerEventArgs e) {
        if (sender is not Control b) {
            throw new ArgumentException(nameof(sender));
        }
        var square = (ChessSquare)b.DataContext!;
        var vm = (ChessViewModel)Resources["vm"]!;

        vm.HighlightValidMove(square); //Highlights if square contains a valid move position from the selected piece
        vm.HighlightValidPiece(square); //Highlights if no piece has been selected and square contains a valid piece with moves
    }

    // Remove the highlight when a panel is exited
    private void Panel_PointerExited(object? sender, Avalonia.Input.PointerEventArgs e) {
        if (sender is not Control b) { throw new ArgumentException(nameof(sender)); }
        var square = (ChessSquare)b.DataContext!;
        var vm = (ChessViewModel)Resources["vm"]!;

        // Reset which square is being hovered over
        if (square != null) {
            square.IsHighlighted = false;
            square.IsValidSelectPiece = false;
        }
    }

    // Used to select a square
    private void Panel_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e) {
        if (sender is not Control b) {
            throw new ArgumentException(nameof(sender));
        }
        var square = (ChessSquare)b.DataContext!;
        var vm = (ChessViewModel)Resources["vm"]!;

        // Check if there is a selected square
        if (vm.SelectedSquare == null) {
            if (square.Piece.Player == vm.CurrentPlayer) {
                vm.SelectedSquare = square;
                vm.SelectedSquare.IsSelected = true;
            }
        }
        // Else, we have selected a piece
        else {
            // Check if we're just changing piece selection
            if (square.Piece.Player == vm.CurrentPlayer) {
                vm.SelectedSquare.IsSelected = false;
                vm.SelectedSquare = square;
                vm.SelectedSquare.IsSelected = true;
            }
            // Else, that means we're applying a move
            else {
                // There's only going to be one valid move at the given end pos
                var move = vm.PossibleMoves.FirstOrDefault(m => m.StartPosition == vm.SelectedSquare.Position && m.EndPosition == square.Position);

                // If a move was found, we can apply it
                if (move != null) {
                    vm.ApplyMove(move);
                }
                // Then we reset selected square
                vm.SelectedSquare.IsSelected = false;
                vm.SelectedSquare = null;
            }
        }

        //Remove any highlighting
        square.IsHighlighted = false;

        if (!vm.PossibleMoves.Any()) {
            //MessageBoxManager
        }

    }
}