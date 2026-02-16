using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data.Converters;

namespace Cecs475.BoardGames.Othello.AvaloniaView {
	public class OthelloCurrentPlayerConverter : IValueConverter {
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
			if (value is not int player) {
				throw new ArgumentException();
			}
			return player == 1 ? "Black" : "White";
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

}
