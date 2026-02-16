using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Numerics;
using System.Text;
using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Model;

#pragma warning disable 1591 // disable warning about missing XML documentation.

namespace Cecs475.BoardGames.Chess.Model {
	/// <summary>
	/// Represents the board state of a game of chess. Tracks which squares of the 8x8 board are occupied
	/// by which player's pieces.
	/// </summary>
	public class ChessBoard : IGameBoard {
		#region Member fields.

		public const int BoardSize = 8;
		// The history of moves applied to the board.
		private List<ChessMove> mMoveHistory = new List<ChessMove>();

        private List<GameAdvantage> mAdvantageHistory = new List<GameAdvantage>();

        private List<ChessPieceType> mCaptureHistory = new List<ChessPieceType>();

        private List<int> drawCounterHistory = new List<int>(); //Track draws

        // TODO: Add fields to implement bitboards for the black and white pieces.
        private byte[,] mBoard = new byte[8, 4];
        // TODO: Add a means of tracking miscellaneous board state, like captured pieces and the 50-move rule.
        
        private bool whiteKingSideCastle = true;
        private bool whiteQueenSideCastle = true;
        private bool blackQueenSideCastle = true;
        private bool blackKingSideCastle = true;

        private List<bool> blackKingSideCastleHistory = [];
        private List<bool> blackQueenSideCastleHistory = [];
        private List<bool> whiteKingSideCastleHistory = [];
        private List<bool> whiteQueenSideCastleHistory = [];

        public int drawCounter = 0;

        //Dictionary of capture values to reference
        private Dictionary<ChessPieceType, int> captureValues = new Dictionary<ChessPieceType, int> {
            { ChessPieceType.Empty, 0 },
            { ChessPieceType.Pawn, 1 },
            { ChessPieceType.Knight, 3 },
            { ChessPieceType.Bishop, 3 },
            { ChessPieceType.Rook, 5 },
            { ChessPieceType.Queen, 9 },
            { ChessPieceType.King, 0 } //Kings are counted as 0 here
        };
        #endregion

        #region Auto properties.
        public int CurrentPlayer { get; private set; }

		public GameAdvantage CurrentAdvantage { get; private set; }
		#endregion

		#region Computed properties
		public bool IsFinished {
			get { return (IsDraw || GetPossibleMoves().Count() == 0); }
		}

		public IReadOnlyList<ChessMove> MoveHistory => mMoveHistory;

        public bool IsCheck {
            get {
                BoardPosition curKing = GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer).ElementAt(0); //Only a single king position should be returned here
                return PositionIsAttacked(curKing, 3 - CurrentPlayer);
            }
        }

        public bool IsCheckmate {
			get { if (IsCheck) { return GetPossibleMoves().Count() == 0; }
                return false; }
		}

		public bool IsStalemate {
			get { if (IsCheck) { return false; }
                return (!GetPossibleMoves().Any());
            }
		}

		public bool IsDraw {
			get { return drawCounter == 100; }
		}
		#endregion


		#region Constructors.
		public ChessBoard() {
            mBoard[1, 0] = 153;
            mBoard[1, 1] = 153;
            mBoard[1, 2] = 153;
            mBoard[1, 3] = 153;

            mBoard[6, 0] = 17;
            mBoard[6, 1] = 17;
            mBoard[6, 2] = 17;
            mBoard[6, 3] = 17;

            mBoard[0, 0] = 171;
            mBoard[0, 1] = 205;
            mBoard[0, 2] = 236;
            mBoard[0, 3] = 186;

            mBoard[7, 0] = 35;
            mBoard[7, 1] = 69;
            mBoard[7, 2] = 100;
            mBoard[7, 3] = 50;

            CurrentPlayer = 1;
            CurrentAdvantage = new GameAdvantage(0, 0);
            mAdvantageHistory.Add(CurrentAdvantage);
        }

		public ChessBoard(IEnumerable<Tuple<BoardPosition, ChessPiece>> startingPositions)
			: this() {
			var king1 = startingPositions.Where(t => t.Item2.Player == 1 && t.Item2.PieceType == ChessPieceType.King);
			var king2 = startingPositions.Where(t => t.Item2.Player == 2 && t.Item2.PieceType == ChessPieceType.King);
			if (king1.Count() != 1 || king2.Count() != 1) {
				throw new ArgumentException("A chess board must have a single king for each player");
			}

            CurrentPlayer = 1;

            int whiteAdvantage = 0;
            int blackAdvantage = 0;

            //Wipe the board and then add pieces
            foreach (var position in BoardPosition.GetRectangularPositions(8, 8)) {
                SetPieceAtPosition(position, ChessPiece.Empty);
            }

            foreach (var startPos in startingPositions) 
            {
                if (startPos.Item2.Player == 1) { whiteAdvantage += captureValues[startPos.Item2.PieceType]; }
                else if (startPos.Item2.Player == 2) { blackAdvantage += captureValues[startPos.Item2.PieceType]; }

                SetPieceAtPosition(startPos.Item1, startPos.Item2);
            }

            //Wipe original advtange history -> No longer accurate
            mAdvantageHistory = new List<GameAdvantage>();
            int player;
            if (whiteAdvantage > blackAdvantage) { player = 1; }
            else if (blackAdvantage > whiteAdvantage) { player = 2; }
            else { player = 0; }
            CurrentAdvantage = new GameAdvantage(player, Math.Abs(whiteAdvantage - blackAdvantage));
            mAdvantageHistory.Add(CurrentAdvantage);

        }
		#endregion

		#region Public methods.
		/// <summary>
		/// Returns every position where the given piece owned by the given player can be found.
		/// </summary>
		public IEnumerable<BoardPosition> GetPositionsOfPiece(ChessPieceType pieceType, int forPlayer) {
            List<BoardPosition> positions = new();
            for (int row = 0; row < 8; row++) {
                for (int col = 0; col < 8; col++) {
                    ChessPiece piece = GetPieceAtPosition(new BoardPosition(row, col));
                    if (piece.Player == forPlayer && piece.PieceType == pieceType) {
                        positions.Add(new BoardPosition(row, col));
                    }
                }
            }
            return positions;
        }

		public IEnumerable<ChessMove> GetPossibleMoves() {
			HashSet<ChessMove> moves = new HashSet<ChessMove>();

            foreach (var move in GetPossiblePawnMoves())  
                moves.Add(move);
            foreach (var move in GetPossibleRookMoves())
                moves.Add(move);
            foreach (var move in GetPossibleBishopMoves())
                moves.Add(move);
            foreach (var move in GetPossibleKnightMoves())
                moves.Add(move);
            foreach (var move in GetPossibleQueenMoves())
                moves.Add(move);
            foreach (var move in GetPossibleKingMoves())
                moves.Add(move);

            return moves.ToList();
		}

        public void ApplyMove(ChessMove m) {
            ChessPieceType startPiece = GetPieceAtPosition(m.StartPosition).PieceType;
            drawCounterHistory.Add(drawCounter);

            //Update castling history
            whiteKingSideCastleHistory.Add(whiteKingSideCastle);
            whiteQueenSideCastleHistory.Add(whiteQueenSideCastle);
            blackKingSideCastleHistory.Add(blackKingSideCastle);
            blackQueenSideCastleHistory.Add(blackQueenSideCastle);

            ChessPieceType capturedPiece = GetPieceAtPosition(m.EndPosition).PieceType;
            if (m.MoveType == ChessMoveType.Normal) {
                NormalApplyMove(m);

                if (startPiece == ChessPieceType.Pawn || capturedPiece != ChessPieceType.Empty) {
                    drawCounter = 0;
                }
                else {
                    drawCounter += 1;
                }
            }

            if (m.MoveType == ChessMoveType.CastleQueenSide || m.MoveType == ChessMoveType.CastleKingSide) { 
                CastleApplyMove(m);
                drawCounter += 1;
            }

            if (m.MoveType == ChessMoveType.EnPassant) {
                capturedPiece = ChessPieceType.Pawn;
                EnPassantApplyMove(m);
                drawCounter = 0;
            }

            if (m.MoveType == ChessMoveType.PawnPromote) {
                drawCounter = 0;
                PawnPromotionChessMove pawnPromote = (PawnPromotionChessMove) m;
                if (pawnPromote.SelectedPromotion == ChessPieceType.Queen) {
                    SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Queen, CurrentPlayer));
                }
                else if (pawnPromote.SelectedPromotion == ChessPieceType.Rook) {
                    SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Rook, CurrentPlayer));
                }
                else if (pawnPromote.SelectedPromotion == ChessPieceType.Knight) {
                    SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Knight, CurrentPlayer));
                }
                else if (pawnPromote.SelectedPromotion == ChessPieceType.Bishop) {
                    SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Bishop, CurrentPlayer));
                }
                SetPieceAtPosition(m.StartPosition, ChessPiece.Empty);

                if (CurrentAdvantage.Player != CurrentPlayer) {
                    if (CurrentAdvantage.Player == 0) { mAdvantageHistory.Add(new GameAdvantage(CurrentPlayer, captureValues[pawnPromote.SelectedPromotion] - 1)); }
                    else {
                        GameAdvantage newGameAdvantage = new GameAdvantage(CurrentPlayer, Math.Abs(CurrentAdvantage.Advantage - captureValues[pawnPromote.SelectedPromotion] - 1 - captureValues[capturedPiece]));
                        CurrentAdvantage = newGameAdvantage;
                        mAdvantageHistory.Add(newGameAdvantage); 
                    }
                }
                else {
                    GameAdvantage newGameAdvantage = new GameAdvantage(CurrentPlayer, CurrentAdvantage.Advantage + captureValues[pawnPromote.SelectedPromotion] + captureValues[capturedPiece] - 1);
                    CurrentAdvantage = newGameAdvantage;
                    mAdvantageHistory.Add(newGameAdvantage); 
                }
            }

            //Swap player, add move history, check castling, update draw
            mCaptureHistory.Add(capturedPiece);
            mMoveHistory.Add(m);
            //CanCastle(CurrentPlayer);
            CurrentPlayer = 3 - CurrentPlayer;
        }

        public void UndoLastMove() {
            //Assuming that we never try to undo past the start of the game...
            GameAdvantage previousAdvantage;
            if (mMoveHistory.Count < 1) {
                previousAdvantage = CurrentAdvantage;
            }
            else {
                previousAdvantage = mAdvantageHistory[mAdvantageHistory.Count - 2];
            }
            ChessMove lastMove = mMoveHistory.Last();

            //If the move was normal...
            if (lastMove.MoveType == ChessMoveType.Normal) {
                NormalUndoMove(previousAdvantage, lastMove);
            }

            //Castling
            if (lastMove.MoveType == ChessMoveType.CastleQueenSide || lastMove.MoveType == ChessMoveType.CastleKingSide) {
                CastleUndoMove(lastMove);
            }

            if (lastMove.MoveType == ChessMoveType.EnPassant) {
                PassantUndoMove(lastMove);
            }

            if (lastMove.MoveType == ChessMoveType.PawnPromote) {
                int player = 3 - CurrentPlayer;
                ChessPiece pawn = new ChessPiece(ChessPieceType.Pawn, player);
                SetPieceAtPosition(lastMove.StartPosition, pawn);
                if (mCaptureHistory.Last() == ChessPieceType.Empty) { SetPieceAtPosition(lastMove.EndPosition, new ChessPiece(ChessPieceType.Empty, 0)); }
                else { SetPieceAtPosition(lastMove.EndPosition, new ChessPiece(mCaptureHistory.Last(), CurrentPlayer)); }
            }

            //Remove the last move, update advantage accordingly, change player, and update castle bools
            mMoveHistory.RemoveAt(mMoveHistory.Count - 1);
            drawCounter = drawCounterHistory.Last();
            drawCounterHistory.RemoveAt(drawCounterHistory.Count - 1);
            mCaptureHistory.RemoveAt(mCaptureHistory.Count - 1);
            //CanCastle(CurrentPlayer);
            CurrentPlayer = 3 - CurrentPlayer;
            mAdvantageHistory.RemoveAt(mAdvantageHistory.Count - 1);
            CurrentAdvantage = mAdvantageHistory.Last();

            //Update booleans for castling
            whiteKingSideCastle = whiteKingSideCastleHistory.Last();
            whiteQueenSideCastle = whiteQueenSideCastleHistory.Last();
            blackKingSideCastle = blackKingSideCastleHistory.Last();
            blackQueenSideCastle = blackQueenSideCastleHistory.Last();
            //Update castling history
            blackKingSideCastleHistory.RemoveAt(blackKingSideCastleHistory.Count - 1);
            blackQueenSideCastleHistory.RemoveAt(blackQueenSideCastleHistory.Count - 1);
            whiteKingSideCastleHistory.RemoveAt(whiteKingSideCastleHistory.Count - 1);
            whiteQueenSideCastleHistory.RemoveAt(whiteQueenSideCastleHistory.Count - 1);
        }

        /// <summary>
        /// Returns whatever chess piece is occupying the given position.
        /// </summary>
        public ChessPiece GetPieceAtPosition(BoardPosition pos) {
            int new_col = pos.Col / 2;
            bool isLeft = pos.Col % 2 == 0;

			if (isLeft)
			{
				int player = mBoard[pos.Row, new_col] >> 7;
				ChessPieceType piece = (ChessPieceType)((mBoard[pos.Row, new_col] >> 4) & 0x07);
                if (!(piece == ChessPieceType.Empty && player == 0)) player++;
                return new ChessPiece(piece, player);
			}
            else
			{
                int player = (mBoard[pos.Row, new_col] & 0x0F) >> 3;
                ChessPieceType piece = (ChessPieceType)(mBoard[pos.Row, new_col] & 0x07);
                if (!(piece == ChessPieceType.Empty && player == 0)) player++;
                return new ChessPiece(piece, player);
            }
        }

        /// <summary>
        /// Retruns whatever player is occupying the given position.
        /// </summary>
        public int GetPlayerAtPosition(BoardPosition pos) {
			int new_col = pos.Col / 2;
			bool isLeft = pos.Col % 2 == 0;

			if (isLeft)
				return (mBoard[pos.Row, new_col] >> 7) + 1;
			else
				return ((mBoard[pos.Row, new_col] & 0x0F) >> 3) + 1;
		}

		/// <summary>
		/// Returns all board positions where the given piece can be found.
		/// </summary>
		public IEnumerable<BoardPosition> GetPositionsOfPiece(ChessPiece piece) {
            List<BoardPosition> positions = new List<BoardPosition>();

            // Have to use GetLength because C# is different with 2D arrays so you can't just index for a whole list
            for (int i = 0; i < mBoard.GetLength(0); i++)
            {
                for (int j = 0; j < mBoard.GetLength(1); j++)
                {
					int leftPlayer = mBoard[i, j] >> 7;
					int rightPlayer = (mBoard[i, j] & 0x0F) >> 3;
					ChessPieceType leftPiece = (ChessPieceType)((mBoard[i, j] << 1) >> 5);
					ChessPieceType rightPiece = (ChessPieceType)((mBoard[i, j] << 0x07) >> 5);

                    ChessPiece newLeft = new ChessPiece(leftPiece, leftPlayer);
					ChessPiece newRight = new ChessPiece(rightPiece, rightPlayer);

					if (piece.Player == newLeft.Player & piece.PieceType == newLeft.PieceType)
                        positions.Add(new BoardPosition(i, j * 2));
                    if (piece.Player == newRight.Player & piece.PieceType == newRight.PieceType)
                        positions.Add(new BoardPosition(i, j * 2 + 1));
                }
            }
            return positions;
        }

		/// <summary>
		/// Returns true if the given position has no piece on it.
		/// </summary>
		public bool PositionIsEmpty(BoardPosition pos) {
            if (pos.Row < 0 || pos.Row > 7 || pos.Col < 0 || pos.Col > 7) return false;

            int new_col = pos.Col / 2;
            bool isLeft = pos.Col % 2 == 0;

            if (isLeft)
                return (mBoard[pos.Row, new_col] >> 4) == 0x00;
            else
                return (mBoard[pos.Row, new_col] & 0x0F) == 0x00;
        }

		/// <summary>
		/// Returns true if the given position contains a piece that is the enemy of the given player.
		/// </summary>
		/// <remarks>returns false if the position is not in bounds</remarks>
		public bool PositionIsEnemy(BoardPosition pos, int player) {
            player -= 1;

            if (pos.Row < 0 || pos.Row > 7 || pos.Col < 0 || pos.Col > 7) return false; 

			if (!PositionIsEmpty(pos))
			{
                int new_col = pos.Col / 2;
                bool isLeft = pos.Col % 2 == 0;

				if (isLeft)
					return (mBoard[pos.Row, new_col] >> 7) != player;
				else
					return ((mBoard[pos.Row, new_col] & 0x0F) >> 3) != player;
            }
			return false;
		}

        //Get all the positions a player is attacking
        public IEnumerable<BoardPosition> GetAttackededPositions(int player) {
            List<BoardPosition> positions = new List<BoardPosition>();
            //Adding pawn attacks
            foreach (BoardPosition pos in GetPawnAttackedPositions(player)) {
                if (!positions.Contains(pos)) { positions.Add(pos); }
            }

            //Adding knight attacks
            foreach (BoardPosition pos in GetKnightAttackedPositions(player)) {
                if (!positions.Contains(pos)) { positions.Add(pos); }
            }

            //Adding rook attacks
            foreach (BoardPosition pos in GetRookAttackedPositions(player)) {
                if (!positions.Contains(pos)) { positions.Add(pos); }
            }

            //Adding bishop attacks
            foreach (BoardPosition pos in GetBishopAttackedPositions(player)) {
                if (!positions.Contains(pos)) { positions.Add(pos); }
            }

            //Adding queen attacks
            foreach (BoardPosition pos in GetQueenAttackedPositions(player)) {
                if (!positions.Contains(pos)) { positions.Add(pos); }
            }

            //Adding king attacks
            foreach (BoardPosition pos in GetKingAttackedPositions(player)) {
                if (!positions.Contains(pos)) { positions.Add(pos); }
            }

            return positions;
        }

        public bool PositionIsAttacked(BoardPosition pos, int player)
        {
            foreach (var position in GetAttackededPositions(player))
            {
                if (position.Row == pos.Row && position.Col == pos.Col)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Private methods.
        /// <summary>
        /// Mutates the board state so that the given piece is at the given position.
        /// </summary>
        private void SetPieceAtPosition(BoardPosition pos, ChessPiece piece) {
			//Figure out if we're setting the left side or right side of a byte
			int column = pos.Col / 2;
			bool isLeft = (pos.Col % 2 == 0);
            int player;
            if (piece.Player > 1) { player = piece.Player - 1; }
            else { player = 0; }

            byte encodedPiece = (byte)((player << 3) | (int)piece.PieceType);
            //(&) with 0x0F or 0xF0 to clear left or right bit values, then set the cleared values to the piece
            if (isLeft)
			{
                mBoard[pos.Row, column] = (byte)((mBoard[pos.Row, column] & 0x0F) | (encodedPiece << 4));
            }
			else
			{
                mBoard[pos.Row, column] = (byte)((mBoard[pos.Row, column] & 0xF0) | (encodedPiece));
            }
		}

        //Pawn attack positions
        //TODO: En Passent
        private IEnumerable<BoardPosition> GetPawnAttackedPositions(int player) {
            List<BoardPosition> positions = new List<BoardPosition>();

            int direction = (player == 1) ? -1 : 1; //Get the direction, white is up, black is down

            //Loop through each piece that exists for a player and get attacks
            foreach (BoardPosition pawnPos in GetPositionsOfPiece(ChessPieceType.Pawn, player)) {
                BoardPosition pos1 = new BoardPosition(pawnPos.Row + direction, pawnPos.Col + 1);
                BoardPosition pos2 = new BoardPosition(pawnPos.Row + direction, pawnPos.Col - 1);

                if (PositionIsEnemy(pos1, player) || PositionIsEmpty(pos1)) { positions.Add(pos1); }
                if (PositionIsEnemy(pos2, player) || PositionIsEmpty(pos2)) { positions.Add(pos2); }
            }

            return positions;
        }

		//Knight attack positions
		private IEnumerable<BoardPosition> GetKnightAttackedPositions(int player) {
            List<BoardPosition> positions = new List<BoardPosition>();

            //Use a list of directions instead of hardcoding each position
            var directionList = new List<(int, int)> {
				(2, 1), //NE
				(1, 2),
				(-1, 2), //SE
				(-2, 1),
				(-2, -1), //SW
				(-1, -2),
				(1, -2), //NW
				(2, -1)
			};

            //Loop through each piece that exists for a player and get attacks
            foreach (BoardPosition knightPos in GetPositionsOfPiece(ChessPieceType.Knight, player)) {
				foreach ((int, int) direction in directionList) {
                    BoardPosition pos = new BoardPosition(knightPos.Row + direction.Item1, knightPos.Col + direction.Item2);

                    if (PositionIsEnemy(pos, player) || PositionIsEmpty(pos)) { positions.Add(pos); }
                }
			}

            return positions;
        }

		//Gets the cardinal attack positions for queen / rook / bishop
		private IEnumerable<BoardPosition> GetCardinalAttackedPositions(int player, ChessPieceType piece) {
            List<BoardPosition> positions = new List<BoardPosition>();
			IEnumerable<BoardPosition> posList;
			List<int> validDirections;

            //Cardinal directions
            var directionList = new List<(int, int)> {
				(1, 0), //S
				(1, 1), //SE
				(0, 1), //E
				(-1, 1), //NE
				(-1, 0), //N
				(-1, -1), //NW
				(0, -1), //W
				(1, -1) //SW
            };

			if (piece == ChessPieceType.Bishop) {
				posList = GetPositionsOfPiece(ChessPieceType.Bishop, player);
				validDirections = new List<int>{1, 3, 5, 7}; //Odd directions
            }
			else if (piece == ChessPieceType.Rook) {
                posList = GetPositionsOfPiece(ChessPieceType.Rook, player);
                validDirections = new List<int> {0, 2, 4, 6}; //Even directions
            }
			else {
                posList = GetPositionsOfPiece(ChessPieceType.Queen, player);
				validDirections = new List<int> {0, 1, 2, 3, 4, 5, 6, 7}; ; //All directions
            }

			foreach(BoardPosition piecePos in posList) {
                List<int> invalidDirections = new List<int>();
                for (int i = 1; i < mBoard.GetLength(0); i++) {
					foreach(int direction in validDirections) {
						if (invalidDirections.Contains(direction)){
							continue;
						}
						else {
							BoardPosition pos = new BoardPosition(piecePos.Row + (i * directionList[direction].Item1), piecePos.Col + (i * directionList[direction].Item2));
							if (PositionIsEnemy(pos, player)) {
								invalidDirections.Add(direction);
								positions.Add(pos);
							}
                            else if (PositionIsEmpty(pos)) {
                                positions.Add(pos);
                            }
							else{
								invalidDirections.Add(direction);
							}
                        }
					}
				}
			}
            return positions;
        }

		//Calls the GetCardinalAttackedPositions() for the relavent piece
        private IEnumerable<BoardPosition> GetRookAttackedPositions(int player)
		{
			return GetCardinalAttackedPositions(player, ChessPieceType.Rook);
		}

        //Calls the GetCardinalAttackedPositions() for the relavent piece
        private IEnumerable<BoardPosition> GetBishopAttackedPositions(int player)
        {
            return GetCardinalAttackedPositions(player, ChessPieceType.Bishop);
        }

        //Calls the GetCardinalAttackedPositions() for the relavent piece
        private IEnumerable<BoardPosition> GetQueenAttackedPositions(int player)
        {
            return GetCardinalAttackedPositions(player, ChessPieceType.Queen);
        }

		//Get king attacked positions
		private IEnumerable<BoardPosition> GetKingAttackedPositions(int player)
		{
			List<BoardPosition> positions = new List<BoardPosition>();

			foreach (var kingPos in GetPositionsOfPiece(ChessPieceType.King, player))
			{
				for (int i = -1; i <= 1; i++)
				{
					for (int j = -1; j <= 1; j++)
					{
						BoardPosition newPos = new BoardPosition(kingPos.Row + i, kingPos.Col + j);
						if (PositionIsEnemy(newPos, player) || PositionIsEmpty(newPos)) {  positions.Add(newPos); }
					}
				}
			}

			return positions;
        }

        //Get all possible knight moves
		private IEnumerable<ChessMove> GetPossibleKnightMoves() {
            List<ChessMove> positions = new List<ChessMove>();
            List<(BoardPosition, BoardPosition)> candidatePositions = new List<(BoardPosition, BoardPosition)>(); //Start, End
            int player = CurrentPlayer; //Used to check if we can safely make a move
            //Use a list of directions instead of hardcoding each position
            var directionList = new List<(int, int)> {
                (2, 1), //NE
				(1, 2),
                (-1, 2), //SE
				(-2, 1),
                (-2, -1), //SW
				(-1, -2),
                (1, -2), //NW
				(2, -1)
            };

			//Add each empty position into a list of possible candidates
            foreach (BoardPosition knightPos in GetPositionsOfPiece(ChessPieceType.Knight, CurrentPlayer)) {
                foreach ((int, int) direction in directionList) {
                    BoardPosition pos = new BoardPosition(knightPos.Row + direction.Item1, knightPos.Col + direction.Item2);

                    if (PositionIsEmpty(pos) || PositionIsEnemy(pos, CurrentPlayer)) { candidatePositions.Add((knightPos, pos)); }
                }
            }

			foreach ((BoardPosition, BoardPosition) candidate in candidatePositions) {
				ChessMove move = new ChessMove(candidate.Item1, candidate.Item2);

				ApplyMove(move); //Test a move, then check if we can add it to possible moves
				if (!IsCheckPlayer(player)) { positions.Add(move); }
                UndoLastMove();
			}

            return positions;
        }

        //Get all possible rook / bishop / queen moves
        private IEnumerable<ChessMove> GetPossibleCardinalMoves(ChessPieceType piece) {
            List<ChessMove> positions = new List<ChessMove>();
            List<(BoardPosition, BoardPosition)> candidatePositions = new List<(BoardPosition, BoardPosition)>();
            IEnumerable<BoardPosition> posList;
            List<int> validDirections;
            int player = CurrentPlayer; //Used to check if we can safely make a move

            //Cardinal directions
            var directionList = new List<(int, int)> {
                (1, 0),   // N
                (1, 1),   // NE
                (0, 1),   // E
                (-1, 1),  // SE
                (-1, 0),  // S
                (-1, -1), // SW
                (0, -1),  // W
                (1, -1)   // NW
            };

            if (piece == ChessPieceType.Bishop)
            {
                posList = GetPositionsOfPiece(ChessPieceType.Bishop, CurrentPlayer);
                validDirections = new List<int> { 1, 3, 5, 7 }; //Odd directions
            }
            else if (piece == ChessPieceType.Rook)
            {
                posList = GetPositionsOfPiece(ChessPieceType.Rook, CurrentPlayer);
                validDirections = new List<int> { 0, 2, 4, 6 }; //Even directions
            }
            else
            {
                posList = GetPositionsOfPiece(ChessPieceType.Queen, CurrentPlayer);
                validDirections = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 }; //All directions
            }

            foreach (BoardPosition piecePos in posList) {
                List<int> invalidDirections = new List<int>();
                for (int i = 1; i < mBoard.GetLength(0); i++) {
                    foreach (int direction in validDirections) {
                        if (invalidDirections.Contains(direction)) {
                            continue;
                        }
                        else {
                            BoardPosition pos = new BoardPosition(piecePos.Row + (i * directionList[direction].Item1), piecePos.Col + (i * directionList[direction].Item2));
                            if (PositionIsEmpty(pos)) { //Empty, simply add as a possible direction
                                candidatePositions.Add((piecePos, pos));
                            }
                            else if (PositionIsEnemy(pos, CurrentPlayer)) { //Enemy, add as a candidate and remove direction
                                invalidDirections.Add(direction);
                                candidatePositions.Add((piecePos, pos));
                            }
                            else { //Else, it must be your own piece and we remove direction
                                invalidDirections.Add(direction);
                            }
                        }
                    }
                }
            }

            //Check each candidate position
            foreach ((BoardPosition, BoardPosition) candidate in candidatePositions)
            {
                ChessMove move = new ChessMove(candidate.Item1, candidate.Item2);

                if (GetPieceAtPosition(candidate.Item2).PieceType != ChessPieceType.King)
                {
                    ApplyMove(move); //Test a move, then check if we can add it to possible moves
                    if (!IsCheckPlayer(player)) { positions.Add(move); }

                    UndoLastMove();
                }
            }

            return positions;
        }

        private IEnumerable<ChessMove> GetPossibleBishopMoves()
        {
            return GetPossibleCardinalMoves(ChessPieceType.Bishop);
        }
        
        private IEnumerable<ChessMove> GetPossibleRookMoves()
        {
            return GetPossibleCardinalMoves(ChessPieceType.Rook);
        }
        
        private IEnumerable<ChessMove> GetPossibleQueenMoves()
        {
            return GetPossibleCardinalMoves(ChessPieceType.Queen);
        }

        private IEnumerable<ChessMove> GetPossiblePawnMoves()
		{
			List<ChessMove> moves = new List<ChessMove>();
            foreach (BoardPosition pos in GetPositionsOfPiece(ChessPieceType.Pawn, CurrentPlayer))
            {
                int player = CurrentPlayer;
                if (CurrentPlayer == 1)
                {
                    bool canGoFoward = false;
                    // Checking for moving once foward and twice forward for white
                    BoardPosition forwardOne = new BoardPosition(pos.Row - 1, pos.Col);
                    if (PositionIsEmpty(forwardOne))
                    {
                        ChessMove newMove = new ChessMove(pos, forwardOne);
                        ApplyMove(newMove);
                        if (!IsCheckPlayer(player))
                        {
                            moves.Add(newMove);
                            canGoFoward = true;
                        }
                        UndoLastMove();

                        if (forwardOne.Row == 0 && canGoFoward)
                        {
                            moves.Remove(newMove);
                            moves.Add(new PawnPromotionChessMove(pos, forwardOne, ChessPieceType.Rook));
                            moves.Add(new PawnPromotionChessMove(pos, forwardOne, ChessPieceType.Knight));
                            moves.Add(new PawnPromotionChessMove(pos, forwardOne, ChessPieceType.Bishop));
                            moves.Add(new PawnPromotionChessMove(pos, forwardOne, ChessPieceType.Queen));
                        }


                        BoardPosition forwardTwo = new BoardPosition(pos.Row - 2, pos.Col);
                        // Maybe check to see if the pawn has never moved yet
                        if (PositionIsEmpty(forwardTwo) && pos.Row == 6)
                        {
                            newMove = new ChessMove(pos, forwardTwo);
                            ApplyMove(newMove);
                            if (!IsCheckPlayer(player))
                                moves.Add(new ChessMove(pos, forwardTwo));
                            UndoLastMove();
                        }
                    }

                    bool canTakeLeft = false;
                    BoardPosition forwardLeft = new BoardPosition(pos.Row - 1, pos.Col - 1);
                    if (PositionIsEnemy(forwardLeft, CurrentPlayer))
                    {
                        ChessMove newMove = new ChessMove(pos, forwardLeft);
                        if (GetPieceAtPosition(forwardLeft).PieceType != ChessPieceType.King)
                        {
                            ApplyMove(newMove);
                            if (!IsCheckPlayer(player))
                            {
                                moves.Add(newMove);
                                canTakeLeft = true;
                            }
                            UndoLastMove();

                            if (forwardLeft.Row == 0 && canTakeLeft)
                            {
                                moves.Remove(newMove);
                                moves.Add(new PawnPromotionChessMove(pos, forwardLeft, ChessPieceType.Rook));
                                moves.Add(new PawnPromotionChessMove(pos, forwardLeft, ChessPieceType.Knight));
                                moves.Add(new PawnPromotionChessMove(pos, forwardLeft, ChessPieceType.Bishop));
                                moves.Add(new PawnPromotionChessMove(pos, forwardLeft, ChessPieceType.Queen));
                            }
                        }
                    }
                    else if (PositionIsEmpty(forwardLeft) && EnPassantible(pos, forwardLeft))
                    {
                        ChessMove newMove = new ChessMove(pos, forwardLeft, ChessMoveType.EnPassant);
                        if (GetPieceAtPosition(forwardLeft).PieceType != ChessPieceType.King)
                        {
                            ApplyMove(newMove);
                            if (!IsCheckPlayer(player))
                                moves.Add(newMove);
                            UndoLastMove();
                        }
                    }

                    bool canTakeRight = false;  
                    BoardPosition forwardRight = new BoardPosition(pos.Row - 1, pos.Col + 1);
                    if (PositionIsEnemy(forwardRight, CurrentPlayer))
                    {
                        ChessMove newMove = new ChessMove(pos, forwardRight);
                        if (GetPieceAtPosition(forwardRight).PieceType != ChessPieceType.King)
                        {
                            ApplyMove(newMove);
                            if (!IsCheckPlayer(player))
                            {
                                moves.Add(new ChessMove(pos, forwardRight));
                                canTakeRight = true;
                            }
                            UndoLastMove();

                            if (forwardRight.Row == 0 && canTakeRight)
                            {
                                moves.Remove(newMove);
                                moves.Add(new PawnPromotionChessMove(pos, forwardRight, ChessPieceType.Rook));
                                moves.Add(new PawnPromotionChessMove(pos, forwardRight, ChessPieceType.Knight));
                                moves.Add(new PawnPromotionChessMove(pos, forwardRight, ChessPieceType.Bishop));
                                moves.Add(new PawnPromotionChessMove(pos, forwardRight, ChessPieceType.Queen));
                            }
                        }
                    }
                    if (PositionIsEmpty(forwardRight) && EnPassantible(pos, forwardRight))
                    {
                        ChessMove newMove = new ChessMove(pos, forwardRight, ChessMoveType.EnPassant);
                        if (GetPieceAtPosition(forwardRight).PieceType != ChessPieceType.King)
                        {
                            ApplyMove(newMove);
                            if (!IsCheckPlayer(player))
                                moves.Add(newMove);
                            UndoLastMove();
                        }
                    }
                }
                else if (CurrentPlayer == 2)
                {
                    bool canGoFoward = false;
                    BoardPosition forwardOne = new BoardPosition(pos.Row + 1, pos.Col);
                    if (PositionIsEmpty(forwardOne))
                    {
                        ChessMove newMove = new ChessMove(pos, forwardOne);
                        ApplyMove(newMove);
                        if (!IsCheckPlayer(player))
                        {
                            moves.Add(newMove);
                            canGoFoward = true;
                        }
                        UndoLastMove();

                        if (forwardOne.Row == 7 && canGoFoward)
                        {
                            moves.Remove(newMove);
                            moves.Add(new PawnPromotionChessMove(pos, forwardOne, ChessPieceType.Rook));
                            moves.Add(new PawnPromotionChessMove(pos, forwardOne, ChessPieceType.Knight));
                            moves.Add(new PawnPromotionChessMove(pos, forwardOne, ChessPieceType.Bishop));
                            moves.Add(new PawnPromotionChessMove(pos, forwardOne, ChessPieceType.Queen));
                        }

                        BoardPosition forwardTwo = new BoardPosition(pos.Row + 2, pos.Col);
                        // Maybe check to see if the pawn has never moved yet
                        if (PositionIsEmpty(forwardTwo) && pos.Row == 1)
                        {
                            newMove = new ChessMove(pos, forwardTwo);
                            ApplyMove(newMove);
                            if (!IsCheckPlayer(player)) 
                                moves.Add(new ChessMove(pos, forwardTwo));
                            UndoLastMove();
                        }


                    }

                    bool canTakeLeft = false;
                    BoardPosition forwardLeft = new BoardPosition(pos.Row + 1, pos.Col - 1);
                    if (PositionIsEnemy(forwardLeft, CurrentPlayer))
                    {
                        ChessMove newMove = new ChessMove(pos, forwardLeft);
                        if (GetPieceAtPosition(forwardLeft).PieceType != ChessPieceType.King)
                        {
                            ApplyMove(newMove);
                            if (!IsCheckPlayer(player))
                            {
                                moves.Add(newMove);
                                canTakeLeft = true;
                            }
                            UndoLastMove();

                            if (forwardLeft.Row == 7 && canTakeLeft)
                            {
                                moves.Remove(newMove);
                                moves.Add(new PawnPromotionChessMove(pos, forwardLeft, ChessPieceType.Rook));
                                moves.Add(new PawnPromotionChessMove(pos, forwardLeft, ChessPieceType.Knight));
                                moves.Add(new PawnPromotionChessMove(pos, forwardLeft, ChessPieceType.Bishop));
                                moves.Add(new PawnPromotionChessMove(pos, forwardLeft, ChessPieceType.Queen));
                            }
                        }
                    }

                    else if (PositionIsEmpty(forwardLeft) && EnPassantible(pos, forwardLeft))
                    {
                        ChessMove newMove = new ChessMove(pos, forwardLeft, ChessMoveType.EnPassant);
                        if (GetPieceAtPosition(forwardLeft).PieceType != ChessPieceType.King)
                        {
                            ApplyMove(newMove);
                            if (!IsCheckPlayer(player))
                                moves.Add(newMove);
                            UndoLastMove();
                        }
                    }

                    bool canTakeRight = false;
                    BoardPosition forwardRight = new BoardPosition(pos.Row + 1, pos.Col + 1);
                    if (PositionIsEnemy(forwardRight, CurrentPlayer))
                    {
                        ChessMove newMove = new ChessMove(pos, forwardRight);
                        if (GetPieceAtPosition(forwardRight).PieceType != ChessPieceType.King)
                        {
                            ApplyMove(newMove);
                            if (!IsCheckPlayer(player))
                            {
                                moves.Add(newMove);
                                canTakeRight = true;
                            }
                            UndoLastMove();

                            if (forwardRight.Row == 7 && canTakeRight)
                            {
                                moves.Remove(newMove);
                                moves.Add(new PawnPromotionChessMove(pos, forwardRight, ChessPieceType.Rook));
                                moves.Add(new PawnPromotionChessMove(pos, forwardRight, ChessPieceType.Knight));
                                moves.Add(new PawnPromotionChessMove(pos, forwardRight, ChessPieceType.Bishop));
                                moves.Add(new PawnPromotionChessMove(pos, forwardRight, ChessPieceType.Queen));
                            }
                        }
                    }
                    if (PositionIsEmpty(forwardRight) && EnPassantible(pos, forwardRight))
                    {
                        ChessMove newMove = new ChessMove(pos, forwardRight, ChessMoveType.EnPassant);
                        if (GetPieceAtPosition(forwardRight).PieceType != ChessPieceType.King)
                        {
                            ApplyMove(newMove);
                            if (!IsCheckPlayer(player))
                                moves.Add(newMove);
                            UndoLastMove();
                        }
                    }
                }
            }
            return moves;
        }

        private bool EnPassantible(BoardPosition startPos, BoardPosition endPos)
        {
            if (mMoveHistory.Count <= 0) return false;

            int direction = endPos.Row - startPos.Row;
            ChessMove mostRecentMove = mMoveHistory.Last();
            ChessPiece piece = GetPieceAtPosition(mostRecentMove.EndPosition);
            if (piece.PieceType != ChessPieceType.Pawn || piece.Player == CurrentPlayer) return false;
            else if (Math.Abs(mostRecentMove.EndPosition.Row - mostRecentMove.StartPosition.Row) != 2) return false;
            else if (startPos.Col ==  endPos.Col) return false;
            else if (mostRecentMove.EndPosition.Col != endPos.Col) return false;
            else if (mostRecentMove.EndPosition.Row != startPos.Row) return false;

            return true;
        }

        //Get all king moves
        private IEnumerable<ChessMove> GetPossibleKingMoves() {
            List<ChessMove> moves = new List<ChessMove>();
            int player = CurrentPlayer;

            var directionList = new List<(int, int)> {
                (1, 0),   // N
                (1, 1),   // NE
                (0, 1),   // E
                (-1, 1),  // SE
                (-1, 0),  // S
                (-1, -1), // SW
                (0, -1),  // W
                (1, -1)   // NW
            };

            //Checking 8 cardinal directions
            foreach (BoardPosition pos in GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer)) {
                foreach (var move in directionList) {
                    BoardPosition newPos = new BoardPosition(pos.Row + move.Item1, pos.Col + move.Item2);

                    if (PositionIsEmpty(newPos) || PositionIsEnemy(newPos, CurrentPlayer)) {
                        ChessMove nextMove = new ChessMove(pos, newPos, ChessMoveType.Normal);
                        
                        if (GetPieceAtPosition(newPos).PieceType != ChessPieceType.King)
                        {
                            ApplyMove(nextMove);

                            if (!IsCheckPlayer(player)) { moves.Add(nextMove); }

                            UndoLastMove();
                        }
                    }
                }
            }

            BoardPosition kingPos = GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer).Last();
            var castleList = new List<(int, int)> {
                //Queen Side
                (0, -1),
                (0, -2),
                (0, -3),
                //King Side
                (0, 1),
                (0, 2),
            };

            //King should be in the original position to castle
            if ((player == 1 && kingPos != new BoardPosition(7, 4)) ||
                (player == 2 && kingPos != new BoardPosition(0, 4))) {
                return moves;
            }

            bool longCastle = true;
            bool shortCastle = true;
            //Checking for castling - we can only castle if we are not in check
            if (!IsCheckPlayer(player)) {
                BoardPosition longRook;
                BoardPosition shortRook;
                if (player == 1) {
                    longRook = new BoardPosition(7, 0); //Queen Side
                    shortRook = new BoardPosition(7, 7); //King side
                }
                else {
                    longRook = new BoardPosition(0, 0); //Queen Side
                    shortRook = new BoardPosition(0, 7); //King side
                }

                //Check queen side castling
                if (whiteQueenSideCastle && player == 1 || blackQueenSideCastle && player == 2) {
                    //Check that spaces in between king and rook are empty and if moving to any of those positions would place it in check
                    //Long castle check
                    if (GetPieceAtPosition(longRook).PieceType == ChessPieceType.Rook && GetPieceAtPosition(longRook).Player == player) {
                        //Check queen side spaces
                        for (int i = 0; i <= 2; i++) {
                            BoardPosition checkSpace = new BoardPosition(kingPos.Row, kingPos.Col + castleList[i].Item2);
                            if (!PositionIsEmpty(checkSpace) || i < 2 && PositionIsAttacked(checkSpace, 3 - player)) {
                                longCastle = false;
                                break;
                            }
                        }
                    }
                    else { longCastle = false; }
                }
                else { longCastle = false; }

                if (whiteKingSideCastle && player == 1 || blackKingSideCastle && player == 2) {
                    //Short castle check
                    if (GetPieceAtPosition(shortRook).PieceType == ChessPieceType.Rook && GetPieceAtPosition(shortRook).Player == player) {
                        for (int i = 3; i <= 4; i++) {
                            BoardPosition checkSpace = new BoardPosition(kingPos.Row, kingPos.Col + castleList[i].Item2);
                            if (!PositionIsEmpty(checkSpace) || i < 4 && PositionIsAttacked(checkSpace, 3 - player)) {
                                shortCastle = false;
                                break;
                            }
                        }
                    }
                    else { shortCastle = false; }
                }
                else { shortCastle = false; }


                //Now we check if castling does not put us in check
                ChessMove longCastleMove;
                ChessMove shortCastleMove;
                if (player == 1) {
                    longCastleMove = new ChessMove(kingPos, new BoardPosition(7, 2), ChessMoveType.CastleQueenSide);
                    shortCastleMove = new ChessMove(kingPos, new BoardPosition(7, 6), ChessMoveType.CastleKingSide);
                }
                else {
                    longCastleMove = new ChessMove(kingPos, new BoardPosition(0, 2), ChessMoveType.CastleQueenSide);
                    shortCastleMove = new ChessMove(kingPos, new BoardPosition(0, 6), ChessMoveType.CastleKingSide);
                }

                if (longCastle) {
                    ApplyMove(longCastleMove);
                    if (!IsCheckPlayer(player)) { moves.Add(longCastleMove); }
                    UndoLastMove();
                }
                if (shortCastle) {
                    ApplyMove(shortCastleMove);
                    if (!IsCheckPlayer(player)) { moves.Add(shortCastleMove); }
                    UndoLastMove();
                }
            }
            return moves;
        }

        //Used to check if a move is safe or not for a given player
        private bool IsCheckPlayer(int player) {
            BoardPosition curKing = GetPositionsOfPiece(ChessPieceType.King, player).ElementAt(0); //Only a single king position should be returned here
            return PositionIsAttacked(curKing, 3 - player);
        }
        
        //Helper functions for apply / undo move
        private void NormalApplyMove(ChessMove m) {
            //Castling check
            ChessPiece startPiece = GetPieceAtPosition(m.StartPosition);
            ChessPiece endPiece = GetPieceAtPosition(m.EndPosition);
            if (startPiece.PieceType == ChessPieceType.Rook && m.StartPosition == new BoardPosition(7, 7) || m.EndPosition == new BoardPosition(7, 7)) {
                whiteKingSideCastle = false;
            }
            if (startPiece.PieceType == ChessPieceType.Rook && m.StartPosition == new BoardPosition(7, 0) || m.EndPosition == new BoardPosition(7, 0)) {
                whiteQueenSideCastle = false;
            }
            if (startPiece.PieceType == ChessPieceType.Rook && m.StartPosition == new BoardPosition(0, 0) || m.EndPosition == new BoardPosition(0, 0)) {
                blackQueenSideCastle = false;
            }
            if (startPiece.PieceType == ChessPieceType.Rook && m.StartPosition == new BoardPosition(0, 7) || m.EndPosition == new BoardPosition(0, 7)) {
                blackKingSideCastle = false;
            }

            //If a king has moved, castling is no longer available
            if (startPiece.PieceType == ChessPieceType.King) {
                if (CurrentPlayer == 1) {
                    whiteKingSideCastle = false;
                    whiteQueenSideCastle = false;
                }
                else {
                    blackQueenSideCastle = false;
                    blackKingSideCastle = false;
                }
            }

            //If a piece is simply moving, game advantage does not change
            if (endPiece.Equals(ChessPiece.Empty)) {
                mAdvantageHistory.Add(CurrentAdvantage);
            }
            //This means that some piece was captured
            else {
                //This means the current player in advantage is swapped (Or placed in neutral)
                if (CurrentAdvantage.Advantage <= captureValues[endPiece.PieceType] && CurrentAdvantage.Player == GetPlayerAtPosition(m.EndPosition)) {
                    GameAdvantage newAdvantage;
                    if (Math.Abs(CurrentAdvantage.Advantage - captureValues[endPiece.PieceType]) == 0) { newAdvantage = new GameAdvantage(0, 0); }

                    else {
                        int temp = 3 - CurrentAdvantage.Player;
                        if (CurrentAdvantage.Player == 0) { temp = CurrentPlayer; }
                        newAdvantage = new GameAdvantage(temp, Math.Abs(CurrentAdvantage.Advantage - captureValues[endPiece.PieceType]));
                    }

                    CurrentAdvantage = newAdvantage;
                    mAdvantageHistory.Add(newAdvantage);
                }
                //Advantage is not swapped and we just add normally
                else {
                    GameAdvantage newAdvantage;
                    if (CurrentAdvantage.Player == 0) { newAdvantage = new GameAdvantage(CurrentPlayer, CurrentAdvantage.Advantage + captureValues[endPiece.PieceType]); }
                    else {
                        if (CurrentAdvantage.Player != CurrentPlayer) { newAdvantage = new GameAdvantage(CurrentAdvantage.Player, CurrentAdvantage.Advantage - captureValues[endPiece.PieceType]); }
                        else { newAdvantage = new GameAdvantage(CurrentAdvantage.Player, CurrentAdvantage.Advantage + captureValues[endPiece.PieceType]); }
                    }
                    CurrentAdvantage = newAdvantage;
                    mAdvantageHistory.Add(newAdvantage);
                }
            }

            SetPieceAtPosition(m.EndPosition, GetPieceAtPosition(m.StartPosition));
            SetPieceAtPosition(m.StartPosition, ChessPiece.Empty);
        }

        private void EnPassantApplyMove(ChessMove m) {
            int player = CurrentPlayer;
            ChessPiece pawn = new ChessPiece(ChessPieceType.Pawn, player);

            GameAdvantage newAdvantage;

            if (CurrentAdvantage.Player != player)
            {
                int advantageValue = Math.Abs(CurrentAdvantage.Advantage - captureValues[ChessPieceType.Pawn]);
                if (advantageValue == 0) { newAdvantage = new GameAdvantage(0, 0); }
                if (CurrentAdvantage.Player == 0) { newAdvantage = new GameAdvantage(player, advantageValue); }
                else { newAdvantage = new GameAdvantage(CurrentAdvantage.Player, advantageValue); }
            }
            else
            {
                newAdvantage = new GameAdvantage(CurrentAdvantage.Player, CurrentAdvantage.Advantage + captureValues[ChessPieceType.Pawn]);
            }

            SetPieceAtPosition(m.EndPosition, pawn);
            if (player == 1)
            {
                SetPieceAtPosition(new BoardPosition(m.EndPosition.Row + 1, m.EndPosition.Col), ChessPiece.Empty);
            }
            else
            {
                SetPieceAtPosition(new BoardPosition(m.EndPosition.Row - 1, m.EndPosition.Col), ChessPiece.Empty);
            }

            SetPieceAtPosition(m.StartPosition, ChessPiece.Empty);
            CurrentAdvantage = newAdvantage;
            mAdvantageHistory.Add(newAdvantage);
        }

        private void CastleApplyMove(ChessMove m) {
            if (CurrentPlayer == 1) {
                whiteKingSideCastle = false;
                whiteQueenSideCastle = false;
            }
            else {
                blackKingSideCastle = false;
                blackQueenSideCastle = false;
            }

            int player = CurrentPlayer;
            ChessPiece rook = new ChessPiece(ChessPieceType.Rook, player);
            if (m.MoveType == ChessMoveType.CastleQueenSide) {
                SetPieceAtPosition(m.EndPosition, GetPieceAtPosition(m.StartPosition));
                SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col + 1), rook);
                SetPieceAtPosition(m.StartPosition, ChessPiece.Empty);
                SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col - 2), ChessPiece.Empty);
            }
            else {
                SetPieceAtPosition(m.EndPosition, GetPieceAtPosition(m.StartPosition));
                SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col - 1), rook);
                SetPieceAtPosition(m.StartPosition, ChessPiece.Empty);
                SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col + 1), ChessPiece.Empty);
            }
            mAdvantageHistory.Add(CurrentAdvantage);
        }

        private void NormalUndoMove(GameAdvantage previousAdvantage, ChessMove lastMove) {
            //We can just undo a move normally if nothing happened capture wise
            if (previousAdvantage == CurrentAdvantage) {
                SetPieceAtPosition(lastMove.StartPosition, GetPieceAtPosition(lastMove.EndPosition));
                SetPieceAtPosition(lastMove.EndPosition, ChessPiece.Empty);
            }
            //This means only a piece was captured
            else if (previousAdvantage.Player == CurrentAdvantage.Player && previousAdvantage.Advantage != CurrentAdvantage.Advantage) {
                ChessPieceType capturedPiece = mCaptureHistory.Last();

                SetPieceAtPosition(lastMove.StartPosition, GetPieceAtPosition(lastMove.EndPosition));
                SetPieceAtPosition(lastMove.EndPosition, new ChessPiece(capturedPiece, CurrentPlayer)); //Captured chess piece is of the current player
            }
            //This means that a piece was captured, and player with advantage was swapped
            else {
                ChessPieceType capturedPiece = mCaptureHistory.Last();

                SetPieceAtPosition(lastMove.StartPosition, GetPieceAtPosition(lastMove.EndPosition));
                SetPieceAtPosition(lastMove.EndPosition, new ChessPiece(capturedPiece, CurrentPlayer)); //Captured chess piece is of the current player
            }
        }

        private void CastleUndoMove(ChessMove lastMove) {
            int player = 3 - CurrentPlayer;
            ChessPieceType king = ChessPieceType.King;
            ChessPieceType rook = ChessPieceType.Rook;
            SetPieceAtPosition(lastMove.EndPosition, ChessPiece.Empty);
            if (lastMove.MoveType == ChessMoveType.CastleQueenSide) {
                if (player == 1) {
                    SetPieceAtPosition(new BoardPosition(7, 4), new ChessPiece(king, player));
                    SetPieceAtPosition(new BoardPosition(7, 0), new ChessPiece(rook, player));
                    SetPieceAtPosition(new BoardPosition(7, 3), ChessPiece.Empty);
                }
                else {
                    SetPieceAtPosition(new BoardPosition(0, 4), new ChessPiece(king, player));
                    SetPieceAtPosition(new BoardPosition(0, 0), new ChessPiece(rook, player));
                    SetPieceAtPosition(new BoardPosition(0, 3), ChessPiece.Empty);
                }
            }
            else {
                if (player == 1) {
                    SetPieceAtPosition(new BoardPosition(7, 4), new ChessPiece(king, player));
                    SetPieceAtPosition(new BoardPosition(7, 7), new ChessPiece(rook, player));
                    SetPieceAtPosition(new BoardPosition(7, 5), ChessPiece.Empty);
                }
                else {
                    SetPieceAtPosition(new BoardPosition(0, 4), new ChessPiece(king, player));
                    SetPieceAtPosition(new BoardPosition(0, 7), new ChessPiece(rook, player));
                    SetPieceAtPosition(new BoardPosition(0, 5), ChessPiece.Empty);
                }
            }
        }
        
        private void PassantUndoMove (ChessMove lastMove) {
            int player = 3 - CurrentPlayer;
            ChessPiece pawn = new ChessPiece(ChessPieceType.Pawn, player);
            ChessPiece enemyPawn = new ChessPiece(ChessPieceType.Pawn, 3 - player);
            if (player == 1) {
                SetPieceAtPosition(lastMove.StartPosition, pawn);
                SetPieceAtPosition(new BoardPosition(lastMove.EndPosition.Row + 1, lastMove.EndPosition.Col), enemyPawn);
                SetPieceAtPosition(lastMove.EndPosition, ChessPiece.Empty);
            }
            else {
                SetPieceAtPosition(lastMove.StartPosition, pawn);
                SetPieceAtPosition(new BoardPosition(lastMove.EndPosition.Row - 1, lastMove.EndPosition.Col), enemyPawn);
                SetPieceAtPosition(lastMove.EndPosition, ChessPiece.Empty);
            }
        }

        #endregion

        #region Explicit IGameBoard implementations.
        IEnumerable<IGameMove> IGameBoard.GetPossibleMoves() {
			return GetPossibleMoves();
		}
		void IGameBoard.ApplyMove(IGameMove m) {
			if (m is not ChessMove move) {
				throw new ArgumentException("Can only apply a ChessMove to a ChessBoard");
			}
			ApplyMove(move);
		}


		IReadOnlyList<IGameMove> IGameBoard.MoveHistory => mMoveHistory;
		#endregion

	}
}
