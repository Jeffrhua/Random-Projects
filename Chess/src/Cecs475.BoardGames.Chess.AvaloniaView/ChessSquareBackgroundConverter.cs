using Cecs475.BoardGames.Model;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Collections.Generic;
using System;

namespace Cecs475.BoardGames.Chess.AvaloniaView {
    class ChessSquareBackgroundConverter : IMultiValueConverter {
        private static readonly IBrush LIGHTTILEBRUSH = new SolidColorBrush(Color.Parse("#D9B7A5"));//BG Tile
        private static readonly IBrush DARKTILEBRUSH = new SolidColorBrush(Color.Parse("#AF7C74")); //BG Tile
        private static readonly IBrush HIGHLIGHT_BRUSH = new SolidColorBrush(Color.Parse("#90EE90")); //For highlighting valid moves & pieces with valid moves
        private static readonly IBrush HIGHLIGHT_KING_BRUSH = new SolidColorBrush(Color.Parse("#FFFAA0")); //Highlights if king in check
        private static readonly IBrush SELECTED_BRUSH = new SolidColorBrush(Color.Parse("#FA5F76")); //Piece selection tile

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
            // This converter will receive three properties: the Position of the square, whether it
            // is being selected, and if it part of the possible moves
            if (values.Count < 5 || values[0] is not BoardPosition pos) {
                return null;
                throw new ArgumentException("Converter must be bound to a BoardPosition and 2 booleans");
            }
            if (values[1] is not bool IsHighlighted) {
                return null;
                throw new ArgumentException("Converter must be bound to a BoardPosition and 4 booleans");
            }
            if (values[2] is not bool IsSelected) {
                return null;
                throw new ArgumentException("Converter must be bound to a BoardPosition and 4 boolean");
            }
            if (values[3] is not bool IsKingCheck) {
                return null;
                throw new ArgumentException("Converter must be bound to a BoardPosition and 4 boolean");
            }
            if (values[4] is not bool IsValidSelectPiece) {
                return null;
                throw new ArgumentException("Converter must be bound to a BoardPosition and 4 boolean");
            }

            // If the king is in check
            if (IsKingCheck) {
                return HIGHLIGHT_KING_BRUSH;
            }
            // If a piece contains valid moves
            if (IsValidSelectPiece) {
                return HIGHLIGHT_BRUSH;
            }
            // If a piece is selected
            if (IsSelected) {
                return SELECTED_BRUSH;
            }
            // Hovered squares have a specific color.
            if (IsHighlighted) {
                return HIGHLIGHT_BRUSH;
            }
            // First tile color
            if ((pos.Col % 2 == pos.Row % 2)) {
                return LIGHTTILEBRUSH;
            }
            // Second tile color
            else {
                return DARKTILEBRUSH;
            }
        }
    }
}