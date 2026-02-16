using Cecs475.BoardGames;
using Cecs475.BoardGames.AvaloniaView;
using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Cecs475.BoardGames.Chess.AvaloniaView {

	public class ChessSquare: INotifyPropertyChanged {
        public ChessSquare Self => this;

        private ChessPiece mChessPiece;

        public ChessPiece Piece
        {
            get { return mChessPiece; }
            set
            {
                if (!value.Equals(mChessPiece))
                {
                    mChessPiece = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The position of the square.
        /// </summary>
        public BoardPosition Position
        {
            get; set;
        }

        private bool mIsHighlighted;
        /// <summary>
        /// Whether the square should be highlighted because of a user action.
        /// </summary>
        public bool IsHighlighted
        {
            get { return mIsHighlighted; }
            set
            {
                if (value != mIsHighlighted)
                {
                    mIsHighlighted = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool mIsSelected;
        /// <summary>
        /// Whether the square should be highlighted because of a user selection.
        /// </summary>
        public bool IsSelected {
            get { return mIsSelected; }
            set {
                if (value != mIsSelected) {
                    mIsSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool mIsKingCheck;
        /// <summary>
        /// Whether the square should be highlighted because of a king is in check
        /// </summary>
        public bool IsKingCheck {
            get { return mIsKingCheck; }
            set {
                if (value != mIsKingCheck) {
                    mIsKingCheck = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool mIsValidSelectPiece;
        /// <summary>
        /// Whether the square should be highlighted because it contains valid moves
        /// </summary>
        public bool IsValidSelectPiece {
            get { return mIsValidSelectPiece; }
            set {
                if (value != mIsValidSelectPiece) {
                    mIsValidSelectPiece = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public override string ToString()
        {
            return $"{Piece.Player} {Piece.PieceType}";
        }

    } 

	/// <summary>
	/// Composes a 
	/// </summary>
	public class ChessViewModel : INotifyPropertyChanged, IGameViewModel {
        private readonly ChessBoard mBoard;
        private readonly ObservableCollection<ChessSquare> mSquares;
        private ChessSquare? mSelectedSquare;

        public ChessSquare SelectedSquare {
            get { return mSelectedSquare; }
            set {
                if (mSelectedSquare != value) {
                    mSelectedSquare = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool CanUndo => mBoard.MoveHistory.Any();
        public GameAdvantage BoardAdvantage => mBoard.CurrentAdvantage;

        public IEnumerable<ChessMove> PossibleMoves {
            get; private set;
        }

        public int CurrentPlayer {
            get { return mBoard.CurrentPlayer; }
        }

        public NumberOfPlayers Players { get; set; }

        public ChessViewModel() {
            mBoard = new ChessBoard();

            // Initialize the squares objects based on the board's initial state.
            mSquares = new ObservableCollection<ChessSquare>(
                BoardPosition.GetRectangularPositions(8, 8)
                .Select(pos => new ChessSquare() {
                    Position = pos,
                    Piece = mBoard.GetPieceAtPosition(pos)
                })
            );

            PossibleMoves = mBoard.GetPossibleMoves();
        }

        // Highlights whatever the selected square is
        public void HighlightValidMove(ChessSquare square) {
            if (mSelectedSquare != null) {
                // Check if the hovered square is a valid move from the selected square (Check the start pos & then end pos)
                var validMove = PossibleMoves.Any(m => m.StartPosition.Equals(mSelectedSquare.Position) && m.EndPosition.Equals(square.Position));

                // If it's a valid move, we highlight it
                square.IsHighlighted = validMove;
            }
        }

        // Highlights if a piece has not been selected and a piece contains valid moves
        public void HighlightValidPiece(ChessSquare square) {
            if (mSelectedSquare == null) {
                // Check if the hovered square contains valid moves that can be played
                var validMove = PossibleMoves.Any(m => m.StartPosition.Equals(square.Position));

                // If it's a valid move, we highlight it
                square.IsValidSelectPiece = validMove;
            }
        }

        // Highlights if a square contains a king and current player is in check (Also removes prior player highlighting)
        public void HighlightCheck(ChessMove enemyMove) {
            //Only one king should be returned
            BoardPosition kingPos = mBoard.GetPositionsOfPiece(ChessPieceType.King, mBoard.CurrentPlayer).First();
            BoardPosition enemyKingPos = mBoard.GetPositionsOfPiece(ChessPieceType.King, 3 - mBoard.CurrentPlayer).First(); //Grab the opposite king

            // Switch highlighting for enemy king off
            // This means the king has moved, so we need to grab the prior tile to switch
            ChessSquare? enemySquare; //Setting this as nullable to avoid warnings (This will never be null however...)
            if (enemyMove.EndPosition.Equals(enemyKingPos)) {
                enemySquare = mSquares.FirstOrDefault(p => p.Position == enemyMove.StartPosition);
            }
            // Else, we just find where the enemy king is and switch it off
            else {
                enemySquare = mSquares.FirstOrDefault(p => p.Position == enemyKingPos);
            }

            if (enemySquare != null) {
                enemySquare.IsKingCheck = false;
            }

            //Grab the tile that currently hosts the square and check if we're in check
            var square = mSquares.FirstOrDefault(p => p.Position == kingPos);
            if (square != null) {
                if (mBoard.IsCheck) {
                    square.IsKingCheck = true;
                }
                else {
                    square.IsKingCheck = false;
                }
            }
        }

        public void ApplyMove(ChessMove move) {
            IEnumerable<ChessMove> possMoves = mBoard.GetPossibleMoves();

            // Validate the move as possible.
            foreach (var m in possMoves) {
                if (m == move) {
                    // If we're promoting, pull up the window
                    if (move.MoveType == ChessMoveType.PawnPromote) {
                        var promotionWindow = new PromotionWindow(this, move.StartPosition, move.EndPosition);
                        promotionWindow.Show();
                        break;
                    }
                    // Else, continue as normal & see if that move put the king in check
                    mBoard.ApplyMove(move);
                    HighlightCheck(move);
                    break;
                }
            }

            RebindState();
            if (mBoard.IsFinished) {
                GameFinished?.Invoke(this, new EventArgs());
            }
        }

        // Separate promotion apply move to avoid looping through the same window
        public void PromotionApplyMove(ChessMove move) {
            mBoard.ApplyMove(move);
            HighlightCheck(move);

            RebindState();
            if (mBoard.IsFinished) {
                GameFinished?.Invoke(this, new EventArgs());
            }
        }

        // Grab the piece at a position
        public ChessPiece getPieceAtPos(BoardPosition pos) {
            return mBoard.GetPieceAtPosition(pos);
        }

        private void RebindState() {
            // Rebind the possible moves, now that the board has changed.
            PossibleMoves = mBoard.GetPossibleMoves();

            // Update the collection of squares by examining the new board state.
            var newSquares = BoardPosition.GetRectangularPositions(8, 8);
            int i = 0;
            foreach (var pos in newSquares) {
                mSquares[i].Piece = mBoard.GetPieceAtPosition(pos);
                i++;
            }

            OnPropertyChanged(nameof(BoardAdvantage));
            OnPropertyChanged(nameof(CurrentPlayer));
            OnPropertyChanged(nameof(CanUndo));
        }

        public void UndoMove() {
			if (CanUndo) {
                mBoard.UndoLastMove();
                // In one-player mode, Undo has to remove an additional move to return to the
                // human player's turn.
                if (Players == NumberOfPlayers.One && CanUndo) {
                    mBoard.UndoLastMove();
                }

                if (mBoard.MoveHistory.Count > 0) {
                    HighlightCheck(mBoard.MoveHistory.Last());
                }
            }
            RebindState();
        }

        public ObservableCollection<ChessSquare> Squares {
            get { return mSquares; }
        }

        // Invoke this event after applying a move if the game is now finished.
        public event EventHandler? GameFinished;
		public event PropertyChangedEventHandler? PropertyChanged;

		private void OnPropertyChanged([CallerMemberName]string? name = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

	}
}
