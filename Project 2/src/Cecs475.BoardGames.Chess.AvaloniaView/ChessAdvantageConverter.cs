using Cecs475.BoardGames.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data.Converters;

namespace Cecs475.BoardGames.Chess.AvaloniaView {
    public class ChessAdvantageConverter : IValueConverter {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is not GameAdvantage v) {
                throw new ArgumentException();
            }
            if (v.Player == 0)
                return "Tie game";
            if (v.Player == 1)
                return $"White has a +{v.Advantage} advantage";
            return $"Black has a +{v.Advantage} advantage";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
