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
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Cecs475.BoardGames.Chess.Model;

namespace Cecs475.BoardGames.Chess.AvaloniaView {
    internal class ChessSquarePieceConverter : IValueConverter {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            // Check if we're dealing with a chess piece first and not an empty tile
            if (value is ChessPiece piece && piece.Player != 0) {
                string chessPiece = piece.PieceType.ToString();
                string player = piece.Player.ToString();
                return new Bitmap(AssetLoader.Open(new Uri($"avares://Cecs475.BoardGames.Chess.AvaloniaView/Resources/{player}{chessPiece}.png")));
            }
            else {
                return new Bitmap(AssetLoader.Open(new Uri("avares://Cecs475.BoardGames.Chess.AvaloniaView/Resources/Blank.png")));
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
