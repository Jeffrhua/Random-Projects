using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Cecs475.BoardGames.Othello.AvaloniaView {
	public class OthelloSquarePlayerConverter : IValueConverter {
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
			if (value is not int player) {
				throw new ArgumentException(nameof(value));
			}
			if (player == 0) {
				return null;
			}

			Ellipse token = new Ellipse() {
				Fill = GetFillBrush(player)
			};
			return token;
		}

		private static IBrush GetFillBrush(int player) =>
			player == 1 ? Brushes.Black : Brushes.White;

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
