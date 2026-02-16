using Avalonia.Data.Converters;
using System;

namespace Cecs475.BoardGames.AvaloniaView {
	/// <summary>
	/// Represents a game type that can be played in our WPF application, providing factory methods
	/// to construct views and converters for the game.
	/// </summary>
	public interface IAvaloniaGameFactory {
		/// <summary>
		/// The name of the game.
		/// </summary>
		string GameName { get; }

		/// <summary>
		/// Returns a new Avalonia UserControl for playing the game.
		/// </summary>
		IAvaloniaGameView CreateGameView();

		/// <summary>
		/// Returns a value converter for converting a GameAdvantage to a string to be 
		/// displayed in the application.
		/// </summary>
		IValueConverter CreateBoardAdvantageConverter();

		/// <summary>
		/// Returns a value converter for converting the game's current player to a string to be 
		/// displayed in the application.
		/// </summary>
		IValueConverter CreateCurrentPlayerConverter();
	}
}
