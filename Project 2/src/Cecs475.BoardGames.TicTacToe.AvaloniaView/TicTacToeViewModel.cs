using Cecs475.BoardGames.Model;
using Cecs475.BoardGames.TicTacToe.Model;
using Cecs475.BoardGames.AvaloniaView;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia;
using System.Runtime.CompilerServices;

namespace Cecs475.BoardGames.TicTacToe.AvaloniaView {
	public class TicTacToeSquare : INotifyPropertyChanged {
		private int mPlayer;
		public int Player {
			get { return mPlayer; }
			set {
				if (value != mPlayer) {
					mPlayer = value;
					OnPropertyChanged();
				}
			}
		}

		public BoardPosition Position {
			get; set;
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		private void OnPropertyChanged([CallerMemberName]string? name = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}

	public class TicTacToeViewModel : IGameViewModel, INotifyPropertyChanged {
		private TicTacToeBoard mBoard;
		private ObservableCollection<TicTacToeSquare> mSquares;

		public event EventHandler? GameFinished;
		public event PropertyChangedEventHandler? PropertyChanged;

		private void OnPropertyChanged([CallerMemberName]string? name = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public TicTacToeViewModel() {
			mBoard = new TicTacToeBoard();
			mSquares = new ObservableCollection<TicTacToeSquare>(
				BoardPosition.GetRectangularPositions(3, 3)
				.Select(pos => new TicTacToeSquare() {
					Position = pos,
					Player = mBoard.GetPieceAtPosition(pos)
				})
			);

			PossibleMoves = new HashSet<BoardPosition>(
				mBoard.GetPossibleMoves().Select(m => m.Position)
			);
		}

		public void ApplyMove(BoardPosition position) {
			var possMoves = mBoard.GetPossibleMoves() as IEnumerable<TicTacToeMove>;
			foreach (var move in possMoves) {
				if (move.Position.Equals(position)) {
					mBoard.ApplyMove(move);
					break;
				}
			}

			RebindState();
			if (mBoard.IsFinished)
				GameFinished?.Invoke(this, new EventArgs());
		}

		private void RebindState() {
			PossibleMoves = new HashSet<BoardPosition>(mBoard.GetPossibleMoves().Select(m => m.Position));
			foreach (var square in mSquares) {
				square.Player = mBoard.GetPieceAtPosition(square.Position);
			}
			OnPropertyChanged(nameof(BoardAdvantage));
			OnPropertyChanged(nameof(CurrentPlayer));
		}

		public void UndoMove() {
			if (CanUndo) {
				mBoard.UndoLastMove();
				RebindState();
			}
		}

		public ObservableCollection<TicTacToeSquare> Squares {
			get { return mSquares; }
		}

		public HashSet<BoardPosition> PossibleMoves {
			get; private set;
		}

		public GameAdvantage BoardAdvantage => mBoard.CurrentAdvantage;

		public int CurrentPlayer => mBoard.CurrentPlayer;

		public bool CanUndo => mBoard.MoveHistory.Any();
	}

	/// <summary>
	/// An ugly hack for converting a Rect (size of a Panel) into one of the four corners 
	/// of an "X" that spans the four corners of the Panel.
	/// </summary>
	public class TicTacToeXConverter(int width, int height) : IValueConverter {
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
			Rect b = (Rect)value!;
			return new Avalonia.Point(width * b.Width, height * b.Height);
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
	/// <summary>
	/// Converts from an integer player number to an Ellipse representing that player's token.
	/// </summary>
	public class TicTacToeSquarePlayerConverter : IValueConverter {
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
			int player = (int)value!;
			if (player == 0) {
				return null;
			}
			if (player == 2) {
				return new Ellipse() {
					Stroke = Brushes.Black,
					StrokeThickness = 5,
					Fill = null
				};
			}
			Canvas c = new Canvas();
			Line l1 = new Line() {
				Stroke = Brushes.Black,
				StrokeThickness = 5,
			};
			l1.Bind(Line.EndPointProperty,
				new Binding("$parent.Bounds") {
					Converter = new TicTacToeXConverter(1, 1)
				}
			);
			c.Children.Add(l1);

			Line l2 = new Line() {
				Stroke = Brushes.Black,
				StrokeThickness = 5,
			};
			l2.Bind(Line.StartPointProperty,
				new Binding("$parent.Bounds") {
					Converter = new TicTacToeXConverter(1, 0)
				}
			);
			l2.Bind(Line.EndPointProperty,
				new Binding("$parent.Bounds") {
					Converter = new TicTacToeXConverter(0, 1)
				}
			);
			c.Children.Add(l2);
			return c;
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
