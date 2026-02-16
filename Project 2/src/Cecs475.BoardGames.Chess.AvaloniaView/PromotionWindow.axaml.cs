using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Cecs475.BoardGames.AvaloniaView;
using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Model;
using System;
using System.Numerics;

namespace Cecs475.BoardGames.Chess.AvaloniaView;

public partial class PromotionWindow : Window {
    private readonly ChessViewModel mChessViewModel;
    private readonly BoardPosition mStartPos;
    private readonly BoardPosition mEndPos;

    public int player { get; set; }

    public PromotionWindow(ChessViewModel chessViewModel, BoardPosition startPos, BoardPosition endPos) {
        InitializeComponent();

        mChessViewModel = chessViewModel;
        mStartPos = startPos;
        mEndPos = endPos;

        player = mChessViewModel.getPieceAtPos(startPos).Player;

        this.DataContext = this;

        loadImages();
    }

    // Load the relavent images
    private void loadImages() {
        Knight.Source = loadBitmap("Knight");
        Bishop.Source = loadBitmap("Bishop");
        Rook.Source = loadBitmap("Rook");
        Queen.Source = loadBitmap("Queen");
    }

    // Grab the relavent image to load
    private Bitmap loadBitmap(string piece) {
        return new Bitmap(AssetLoader.Open(new Uri($"avares://Cecs475.BoardGames.Chess.AvaloniaView/Resources/{player}{piece}.png")));
    }

    // Apply the relavent promotion move
    private void OnPieceSelected(object? sender, PointerPressedEventArgs e) {
        if (sender is not Image img)
            return;

        // Just set a default move type (Queen by default)
        PawnPromotionChessMove promotion = new PawnPromotionChessMove(mStartPos, mEndPos, ChessPieceType.Queen);

        if (img == Knight) promotion = new PawnPromotionChessMove(mStartPos, mEndPos, ChessPieceType.Knight);
        else if (img == Bishop) promotion = new PawnPromotionChessMove(mStartPos, mEndPos, ChessPieceType.Bishop);
        else if (img == Rook) promotion = new PawnPromotionChessMove(mStartPos, mEndPos, ChessPieceType.Rook);
        else if (img == Queen) promotion = new PawnPromotionChessMove(mStartPos, mEndPos, ChessPieceType.Queen);

        mChessViewModel.PromotionApplyMove(promotion);

        Close();
    }

    // Defining the highlighting backgrounds
    private void Panel_PointerEntered(object? sender, Avalonia.Input.PointerEventArgs e) {
        if (sender is Panel panel) {
            panel.Background = new SolidColorBrush(Color.Parse("#A8C5E8"));
        }
    }

    private void Panel_PointerExited(object sender, PointerEventArgs e) {
        if (sender is Panel panel) {
            panel.Background = new SolidColorBrush(Colors.Transparent);
        }
    }
}