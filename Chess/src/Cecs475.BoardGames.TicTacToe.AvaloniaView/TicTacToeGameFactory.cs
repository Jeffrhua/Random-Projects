
using Avalonia.Data.Converters;
using Cecs475.BoardGames.AvaloniaView;

namespace Cecs475.BoardGames.TicTacToe.AvaloniaView {
	public class TicTacToeGameFactory : IAvaloniaGameFactory {
		public string GameName {
			get {
				return "Tic Tac Toe";
			}
		}

		public IValueConverter CreateBoardAdvantageConverter() {
			return new TicTacToeAdvantageConverter();
		}

		public IValueConverter CreateCurrentPlayerConverter() {
			return new TicTacToeCurrentPlayerConverter();
		}

		public IAvaloniaGameView CreateGameView() {
			return new TicTacToeView();
		}
	}
}
