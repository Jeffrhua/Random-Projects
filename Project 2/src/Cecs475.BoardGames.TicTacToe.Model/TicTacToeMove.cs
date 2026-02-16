using Cecs475.BoardGames.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cecs475.BoardGames.TicTacToe.Model {
	public class TicTacToeMove : IGameMove, IEquatable<TicTacToeMove> {
		public BoardPosition Position { get; private set; }

		public int Player { get; set; }

		public TicTacToeMove(BoardPosition position) {
			Position = position;
		}

		public bool Equals(IGameMove? other) {
			return other is TicTacToeMove tm && Equals(tm);
		}

		public bool Equals(TicTacToeMove? other) {
			return other is not null && Position.Equals(other.Position);
		}
	}
}
