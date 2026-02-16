using Avalonia.Data.Converters;
using Cecs475.BoardGames.AvaloniaView;

namespace Cecs475.BoardGames.Othello.AvaloniaView {
	/// <summary>
	/// Represents an Othello game that can be played in an Avalonia app.
	/// </summary>
	public class OthelloGameFactory : IAvaloniaGameFactory {
		public string GameName {
			get {
				return "Othello";
			}
		}

		public IValueConverter CreateBoardAdvantageConverter() {
			return new OthelloAdvantageConverter();
		}

		public IValueConverter CreateCurrentPlayerConverter() {
			return new OthelloCurrentPlayerConverter();
		}

		public IAvaloniaGameView CreateGameView() {
			return new OthelloView();
		}
	}
}
