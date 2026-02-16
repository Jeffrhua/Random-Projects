using Avalonia.Data.Converters;
using System.Globalization;

namespace Cecs475.BoardGames.TicTacToe.AvaloniaView {
	public class TicTacToeCurrentPlayerConverter : IValueConverter {
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
			int player = (int)value!;
			return player == 1 ? "X" : "O";
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
