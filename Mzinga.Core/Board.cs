﻿// 
// Board.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017 Jon Thysell <http://jonthysell.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mzinga.Core
{
    public class Board
    {
        #region State Properties

        public BoardState BoardState { get; protected set; }

        public int CurrentTurn
        {
            get
            {
                return _currentTurn;
            }
            protected set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _currentTurn = value;
                ResetCaches();
            }
        }
        private int _currentTurn;

        public Color CurrentTurnColor
        {
            get
            {
                return (Color)(CurrentTurn % 2);
            }
        }

        public int CurrentPlayerTurn
        {
            get
            {
                return 1 + (CurrentTurn / 2);
            }
        }

        public bool GameInProgress
        {
            get
            {
                return (BoardState == BoardState.NotStarted || BoardState == BoardState.InProgress);
            }
        }

        public bool GameIsOver
        {
            get
            {
                return (BoardState == BoardState.WhiteWins || BoardState == BoardState.BlackWins || BoardState == BoardState.Draw);
            }
        }

        public string BoardString
        {
            get
            {
                if (null == _boardString)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendFormat("{0}{1}", BoardState.ToString(), BoardStringSeparator);

                    sb.AppendFormat("{0}[{1}]{2}", CurrentTurnColor.ToString(), CurrentPlayerTurn, BoardStringSeparator);

                    foreach (Piece piece in PiecesInPlay)
                    {
                        sb.AppendFormat("{0}{1}", piece.ToString(), BoardStringSeparator);
                    }

                    _boardString = sb.ToString().TrimEnd(BoardStringSeparator);
                }

                return _boardString;
            }
        }

        private string _boardString = null;

        public string TranspositionKey
        {
            get
            {
                if (null == _transpositionKey)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (Piece piece in PiecesInPlay)
                    {
                        sb.Append(EnumUtils.GetShortName(piece.PieceName, true));

                        Position pos = piece.Position;

                        sb.AppendFormat("{0},{1}", pos.Q, pos.R);
                        if (pos.Stack > 0)
                        {
                            sb.AppendFormat(",{0}", pos.Stack);
                        }
                    }

                    _transpositionKey = sb.ToString();
                }

                return _transpositionKey;
            }
        }

        private string _transpositionKey = null;

        #endregion

        #region Piece Enumerable Properties

        public IEnumerable<Piece> AllPieces
        {
            get
            {
                return _pieces.AsEnumerable();
            }
        }

        public IEnumerable<Piece> CurrentTurnPieces
        {
            get
            {
                return _pieces.Where((piece) => { return piece.Color == CurrentTurnColor; });
            }
        }

        public IEnumerable<Piece> WhitePieces
        {
            get
            {
                return _pieces.Where((piece) => { return piece.Color == Color.White; });
            }
        }

        public IEnumerable<Piece> WhiteHand
        {
            get
            {
                return _pieces.Where((piece) => { return (piece.Color == Color.White && piece.InHand); });
            }
        }

        public IEnumerable<Piece> BlackPieces
        {
            get
            {
                return _pieces.Where((piece) => { return piece.Color == Color.Black; });
            }
        }

        public IEnumerable<Piece> BlackHand
        {
            get
            {
                return _pieces.Where((piece) => { return (piece.Color == Color.Black && piece.InHand); });
            }
        }

        public IEnumerable<Piece> PiecesInPlay
        {
            get
            {
                return _pieces.Where((piece) => { return piece.InPlay; });
            }
        }

        private Piece[] _pieces;

        private Dictionary<Position, Piece> _piecesByPosition;

        #endregion

        #region Piece State Properties

        public bool WhiteQueenInPlay
        {
            get
            {
                return GetPiece(PieceName.WhiteQueenBee).InPlay;
            }
        }

        public bool BlackQueenInPlay
        {
            get
            {
                return GetPiece(PieceName.BlackQueenBee).InPlay;
            }
        }

        public bool CurrentTurnQueenInPlay
        {
            get
            {
                return ((CurrentTurnColor == Color.White && WhiteQueenInPlay) || (CurrentTurnColor == Color.Black && BlackQueenInPlay));
            }
        }

        public bool OpponentQueenInPlay
        {
            get
            {
                return ((CurrentTurnColor == Color.White && BlackQueenInPlay) || (CurrentTurnColor == Color.Black && WhiteQueenInPlay));
            }
        }

        #endregion

        #region Caches

        private MoveSet[] _cachedValidMovesByPiece;
        private HashSet<Position> _cachedValidPlacementPositions;

        public CacheMetricsSet ValidMoveCacheMetricsSet { get; private set; } = new CacheMetricsSet();

        public int ValidMoveCacheResets { get; private set; } = 0;

        #endregion

        #region Dimensions

        public int Height
        {
            get
            {
                if (PiecesInPlay.Count() == 0)
                {
                    return 0;
                }

                int minY = PiecesInPlay.Min(p => p.Position.Y);
                int maxY = PiecesInPlay.Max(p => p.Position.Y);

                return (maxY - minY);
            }
        }

        public int Width
        {
            get
            {
                if (PiecesInPlay.Count() == 0)
                {
                    return 0;
                }

                int minX = PiecesInPlay.Min(p => p.Position.X);
                int maxX = PiecesInPlay.Max(p => p.Position.X);

                return (maxX - minX);
            }
        }

        #endregion

        public Board()
        {
            _pieces = new Piece[EnumUtils.NumPieceNames];
            _piecesByPosition = new Dictionary<Position, Piece>();

            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                _pieces[(int)pieceName] = new Piece(pieceName);
            }

            CurrentTurn = 0;
            BoardState = BoardState.NotStarted;
        }

        public Board(string boardString) : this()
        {
            if (string.IsNullOrWhiteSpace(boardString))
            {
                throw new ArgumentNullException("boardString");
            }

            string[] split = boardString.Split(BoardStringSeparator);

            string boardStateString = split[0];

            BoardState boardState;
            if (!Enum.TryParse(boardStateString, out boardState))
            {
                throw new ArgumentException("Couldn't parse board state.", "boardString");
            }
            BoardState = boardState;

            string[] currentTurnSplit = split[1].Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);

            string currentTurnColorString = currentTurnSplit[0];

            Color currentTurnColor;
            if (!Enum.TryParse(currentTurnColorString, out currentTurnColor))
            {
                throw new ArgumentException("Couldn't parse current turn color.", "boardString");
            }

            string currentPlayerTurnString = currentTurnSplit[1];

            int currentPlayerTurn;
            if (!int.TryParse(currentPlayerTurnString, out currentPlayerTurn))
            {
                throw new ArgumentException("Couldn't parse current player turn.", "boardString");
            }

            CurrentTurn = 2 * (currentPlayerTurn - 1) + (int)currentTurnColor;

            Queue<Piece> parsedPieces = new Queue<Piece>(EnumUtils.NumPieceNames);

            for (int i = 2; i < split.Length; i++)
            {
                parsedPieces.Enqueue(new Piece(split[i]));
            }

            while (parsedPieces.Count > 0)
            {
                Piece parsedPiece = parsedPieces.Dequeue();
                if (parsedPiece.InPlay)
                {
                    if (parsedPiece.Position.Stack > 0 && !HasPieceAt(parsedPiece.Position.GetShifted(0, 0, 0, -1)))
                    {
                        parsedPieces.Enqueue(parsedPiece);
                    }
                    else
                    {
                        Piece piece = GetPiece(parsedPiece.PieceName);
                        MovePiece(piece, parsedPiece.Position);
                    }
                }
            }

            if (!IsOneHive())
            {
                throw new ArgumentException("The boardString violates the one-hive rule.", "boardString");
            }
        }

        public bool HasPieceAt(Position position)
        {
            return (null != GetPiece(position));
        }

        public Piece GetPiece(PieceName pieceName)
        {
            if (pieceName == PieceName.INVALID)
            {
                throw new ArgumentOutOfRangeException("pieceName");
            }

            return _pieces[(int)pieceName];
        }

        public Piece GetPiece(Position position)
        {
            if (null == position)
            {
                throw new ArgumentNullException("position");
            }

            Piece piece;
            if (_piecesByPosition.TryGetValue(position, out piece))
            {
                return piece;
            }

            return null;
        }

        public Piece GetPieceOnTop(Position position)
        {
            if (null == position)
            {
                throw new ArgumentNullException("position");
            }

            if (position.Stack > 0)
            {
                position = position.GetShifted(0, 0, 0, -position.Stack);
            }

            Piece topPiece = GetPiece(position);

            if (null != topPiece)
            {
                while (null != topPiece.PieceAbove)
                {
                    topPiece = topPiece.PieceAbove;
                }
            }

            return topPiece;
        }

        protected void MovePiece(Piece piece, Position newPosition)
        {
            if (null == piece)
            {
                throw new ArgumentNullException("piece");
            }

            if (piece.InPlay)
            {
                RemoveFromPieceByPosition(piece);
                if (null != piece.PieceBelow)
                {
                    piece.PieceBelow.PieceAbove = null;
                    piece.PieceBelow = null;
                }
            }

            piece.Move(newPosition);

            if (piece.InPlay)
            {
                AddToPieceByPosition(piece);
                if (newPosition.Stack > 0)
                {
                    Position posBelow = newPosition.GetShifted(0, 0, 0, -1);
                    Piece pieceBelow = GetPiece(posBelow);
                    pieceBelow.PieceAbove = piece;
                    piece.PieceBelow = pieceBelow;
                }
            }
        }

        protected void AddToPieceByPosition(Piece piece)
        {
            if (null == piece)
            {
                throw new ArgumentNullException("piece");
            }

            if (null == piece.Position)
            {
                throw new ArgumentOutOfRangeException("piece");
            }

            _piecesByPosition[piece.Position] = piece;
        }

        protected void RemoveFromPieceByPosition(Piece piece)
        {
            if (null == piece)
            {
                throw new ArgumentNullException("piece");
            }

            if (null == piece.Position)
            {
                throw new ArgumentOutOfRangeException("piece");
            }

            _piecesByPosition[piece.Position] = null;
        }

        public bool PieceIsOnTop(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            return (null == targetPiece.PieceAbove);
        }

        public bool IsOneHive()
        {
            // Whether or not a piece has been found to be part of the hive
            bool[] partOfHive = new bool[EnumUtils.NumPieceNames];

            // Find a piece on the board to start checking
            Piece startingPiece = null;
            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                Piece piece = GetPiece(pieceName);
                if (piece.InHand)
                {
                    partOfHive[(int)pieceName] = true;
                }
                else
                {
                    partOfHive[(int)pieceName] = false;
                    if (null == startingPiece && piece.Position.Stack == 0)
                    {
                        // Save off a starting piece on the bottom
                        startingPiece = piece;
                        partOfHive[(int)pieceName] = true;
                    }
                }
            }

            // There is at least one piece on the board
            if (null != startingPiece)
            {
                Queue<Piece> piecesToLookAt = new Queue<Piece>();
                piecesToLookAt.Enqueue(startingPiece);

                while (piecesToLookAt.Count > 0)
                {
                    Piece currentPiece = piecesToLookAt.Dequeue();

                    // Check all pieces at this stack level
                    foreach (Position neighbor in currentPiece.Position.Neighbors)
                    {
                        Piece neighborPiece = GetPiece(neighbor);
                        if (null != neighborPiece && !partOfHive[(int)neighborPiece.PieceName])
                        {
                            piecesToLookAt.Enqueue(neighborPiece);
                            partOfHive[(int)neighborPiece.PieceName] = true;
                        }
                    }

                    // Check for all pieces above this one
                    Piece pieceAbove = currentPiece.PieceAbove;
                    while (null != pieceAbove)
                    {
                        partOfHive[(int)pieceAbove.PieceName] = true;
                        pieceAbove = pieceAbove.PieceAbove;
                    }
                }

                // Return true iff every piece was part of the one-hive
                return partOfHive.All((value) => { return value; });
            }

            // If there's no startingPiece, there's nothing on the board
            return true;
        }

        #region Metrics

        public BoardMetrics GetBoardMetrics()
        {
            BoardMetrics boardMetrics = new BoardMetrics(BoardState);

            // Get the metrics for the current turn
            SetCurrentPlayerMetrics(boardMetrics);

            // Save off current valid moves/placements since we'll be returning to it
            MoveSet[] validMoves = _cachedValidMovesByPiece;
            _cachedValidMovesByPiece = null;

            HashSet<Position> validPlacements = _cachedValidPlacementPositions;
            _cachedValidPlacementPositions = null;

            // Spoof going to the next turn to get the opponent's metrics
            _currentTurn++;
            SetCurrentPlayerMetrics(boardMetrics);
            _currentTurn--;

            // Returned, so reload saved valid moves/placements into cache
            _cachedValidPlacementPositions = validPlacements;
            _cachedValidMovesByPiece = validMoves;

            return boardMetrics;
        }

        private void SetCurrentPlayerMetrics(BoardMetrics boardMetrics)
        {
            if (null == boardMetrics)
            {
                throw new ArgumentNullException("boardMetrics");
            }

            IEnumerable<PieceName> currentTurnPieceNames = CurrentTurnColor == Color.White ? EnumUtils.WhitePieceNames : EnumUtils.BlackPieceNames;

            foreach (PieceName pieceName in currentTurnPieceNames)
            {
                boardMetrics[pieceName] = GetPieceMetrics(pieceName);
            }
        }

        public PieceMetrics GetPieceMetrics(PieceName pieceName)
        {
            if (pieceName == PieceName.INVALID)
            {
                throw new ArgumentOutOfRangeException("pieceName");
            }

            Piece targetPiece = GetPiece(pieceName);

            PieceMetrics pieceMetrics = new PieceMetrics();

            pieceMetrics.IsInPlay = targetPiece.InPlay;

            // Move metrics
            MoveSet validMoves = GetValidMoves(targetPiece.PieceName);
            pieceMetrics.ValidMoveCount = validMoves.Count;

            pieceMetrics.NeighborCount = CountNeighbors(targetPiece.PieceName);

            return pieceMetrics;
        }

        public int CountNeighbors(PieceName pieceName)
        {
            Piece piece = GetPiece(pieceName);

            if (piece.InHand)
            {
                return 0;
            }

            int count = 0;

            foreach (Position neighbor in piece.Position.Neighbors)
            {
                count += HasPieceAt(neighbor) ? 1 : 0;
            }

            return count;
        }

        #endregion

        #region GetValidMoves

        public MoveSet GetValidMoves()
        {
            MoveSet moves = new MoveSet();

            if (BoardState == BoardState.NotStarted || BoardState == BoardState.InProgress)
            {
                foreach (Piece piece in CurrentTurnPieces)
                {
                    moves.Add(GetValidMoves(piece.PieceName));
                }

                if (moves.Count == 0)
                {
                    moves.Add(Move.Pass);
                }
            }

            moves.Lock();

            return moves;
        }

        public MoveSet GetValidMoves(PieceName pieceName)
        {
            if (null == _cachedValidMovesByPiece)
            {
                _cachedValidMovesByPiece = new MoveSet[EnumUtils.NumPieceNames];
            }

            if (null != _cachedValidMovesByPiece[(int)pieceName])
            {
                // MoveSet is cached in L1 cache
                ValidMoveCacheMetricsSet["ValidMoves." + EnumUtils.GetShortName(pieceName)].Hits++;
            }
            else
            {
                // MoveSet is not cached in L1 cache
                ValidMoveCacheMetricsSet["ValidMoves." + EnumUtils.GetShortName(pieceName)].Misses++;

                // Calculate MoveSet
                Piece targetPiece = GetPiece(pieceName);
                MoveSet moves = GetValidMovesInternal(targetPiece);
                moves.Lock();

                // Populate cache
                _cachedValidMovesByPiece[(int)pieceName] = moves;
            }

            return _cachedValidMovesByPiece[(int)pieceName];
        }

        private MoveSet GetValidMovesInternal(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            MoveSet validMoves = new MoveSet();

            if (BoardState == BoardState.NotStarted || BoardState == BoardState.InProgress)
            {
                if (targetPiece.Color == CurrentTurnColor && PlacingPieceInOrder(targetPiece))
                {
                    if (CurrentTurn == 0 && targetPiece.Color == Color.White && targetPiece.InHand && targetPiece.PieceName != PieceName.WhiteQueenBee)
                    {
                        // First move must be at the origin and not the White Queen Bee
                        validMoves.Add(new Move(targetPiece.PieceName, Position.Origin));
                    }
                    else if (CurrentTurn == 1 && targetPiece.Color == Color.Black && targetPiece.InHand && targetPiece.PieceName != PieceName.BlackQueenBee)
                    {
                        // Second move must be around the origin and not the Black Queen Bee
                        foreach (Position neighbor in Position.Origin.Neighbors)
                        {
                            validMoves.Add(new Move(targetPiece.PieceName, neighbor));
                        }
                    }
                    else if (targetPiece.InHand && (CurrentPlayerTurn != 4 || // Normal turn OR
                                                    (CurrentPlayerTurn == 4 && // Turn 4 and AND
                                                     (CurrentTurnQueenInPlay || (!CurrentTurnQueenInPlay && targetPiece.BugType == BugType.QueenBee))))) // Queen is in play or you're trying to play it
                    {
                        // Look for valid new placements
                        validMoves.Add(GetValidPlacements(targetPiece));
                    }
                    else if (targetPiece.InPlay && CurrentTurnQueenInPlay && PieceIsOnTop(targetPiece) && CanMoveWithoutBreakingHive(targetPiece))
                    {
                        // Look for valid moves of already played pieces
                        switch (targetPiece.BugType)
                        {
                            case BugType.QueenBee:
                                validMoves.Add(GetValidQueenBeeMovements(targetPiece));
                                break;
                            case BugType.Spider:
                                validMoves.Add(GetValidSpiderMovements(targetPiece));
                                break;
                            case BugType.Beetle:
                                validMoves.Add(GetValidBeetleMovements(targetPiece));
                                break;
                            case BugType.Grasshopper:
                                validMoves.Add(GetValidGrasshopperMovements(targetPiece));
                                break;
                            case BugType.SoldierAnt:
                                validMoves.Add(GetValidSoldierAntMovements(targetPiece));
                                break;
                        }
                    }
                }
            }

            return validMoves;
        }

        private MoveSet GetValidPlacements(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            MoveSet validMoves = new MoveSet();

            Color targetColor = CurrentTurnColor;

            if (targetPiece.Color != targetColor)
            {
                return validMoves;
            }

            if (null == _cachedValidPlacementPositions)
            {
                _cachedValidPlacementPositions = new HashSet<Position>();

                foreach (Piece piece in PiecesInPlay)
                {
                    if (piece.Position.Stack == 0 && GetPieceOnTop(piece.Position).Color == targetColor)
                    {
                        // Piece is in play, on the bottom, and the top is the right color, look through neighbors
                        foreach (Position neighbor in piece.Position.Neighbors)
                        {
                            if (!HasPieceAt(neighbor))
                            {
                                // Neighboring position is a potential, verify its neighbors are empty or same color
                                bool validPlacement = true;
                                foreach (Position surroundingPosition in neighbor.Neighbors)
                                {
                                    Piece surroundingPiece = GetPieceOnTop(surroundingPosition);
                                    if (null != surroundingPiece && surroundingPiece.Color != targetColor)
                                    {
                                        validPlacement = false;
                                        break;
                                    }
                                }

                                if (validPlacement)
                                {
                                    _cachedValidPlacementPositions.Add(neighbor);
                                }
                            }
                        }
                    }
                }

                ValidMoveCacheMetricsSet["ValidPlacements"].Misses++;
            }
            else
            {
                ValidMoveCacheMetricsSet["ValidPlacements"].Hits++;
            }

            foreach (Position validPlacement in _cachedValidPlacementPositions)
            {
                validMoves.Add(new Move(targetPiece.PieceName, validPlacement));
            }

            return validMoves;
        }

        private MoveSet GetValidQueenBeeMovements(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            // Get all slides one away
            return GetValidSlides(targetPiece, 1);
        }

        private MoveSet GetValidSpiderMovements(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            MoveSet validMoves = new MoveSet();

            // Get all slides up to 2 spots away
            MoveSet upToTwo = GetValidSlides(targetPiece, 2);

            if (upToTwo.Count > 0)
            {
                // Get all slides up to 3 spots away
                MoveSet upToThree = GetValidSlides(targetPiece, 3);

                // Get all slides ONLY 3 spots away
                upToThree.Remove(upToTwo);

                validMoves.Add(upToThree);
            }

            return validMoves;
        }

        private MoveSet GetValidBeetleMovements(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            MoveSet validMoves = new MoveSet();

            // Look in all directions
            foreach (Direction direction in EnumUtils.Directions)
            {
                Position newPosition = targetPiece.Position.NeighborAt(direction);
                newPosition = newPosition.GetShifted(0, 0, 0, -newPosition.Stack);

                Piece topNeighbor = GetPieceOnTop(newPosition);

                // Get positions to left and right or direction we're heading
                Direction leftOfTarget = EnumUtils.LeftOf(direction);
                Direction rightOfTarget = EnumUtils.RightOf(direction);
                Position leftNeighborPosition = targetPiece.Position.NeighborAt(leftOfTarget);
                Position rightNeighborPosition = targetPiece.Position.NeighborAt(rightOfTarget);

                Piece topLeftNeighbor = GetPieceOnTop(leftNeighborPosition);
                Piece topRightNeighbor = GetPieceOnTop(rightNeighborPosition);

                // At least one neighbor is present
                int currentHeight = targetPiece.Position.Stack + 1;
                int destinationHeight = null != topNeighbor ? topNeighbor.Position.Stack + 1 : 0;

                int topLeftNeighborHeight = null != topLeftNeighbor ? topLeftNeighbor.Position.Stack + 1 : 0;
                int topRightNeighborHeight = null != topRightNeighbor ? topRightNeighbor.Position.Stack + 1 : 0;

                Position targetPosition = newPosition.GetShifted(0, 0, 0, destinationHeight);
                Move targetMove = new Move(targetPiece.PieceName, targetPosition);

                // "Take-off" beetle
                currentHeight--;

                if (!(currentHeight == 0 && destinationHeight == 0 && topLeftNeighborHeight == 0 && topRightNeighborHeight == 0))
                {
                    // Logic from http://boardgamegeek.com/wiki/page/Hive_FAQ#toc9
                    if (!(destinationHeight < topLeftNeighborHeight && destinationHeight < topRightNeighborHeight && currentHeight < topLeftNeighborHeight && currentHeight < topRightNeighborHeight))
                    {
                        validMoves.Add(targetMove);
                    }
                }
            }

            return validMoves;
        }

        private MoveSet GetValidGrasshopperMovements(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            MoveSet validMoves = new MoveSet();

            Position startingPosition = targetPiece.Position;

            foreach (Direction direction in EnumUtils.Directions)
            {
                Position landingPosition = startingPosition.NeighborAt(direction);

                int distance = 0;
                while (HasPieceAt(landingPosition))
                {
                    // Jump one more in the same direction
                    landingPosition = landingPosition.NeighborAt(direction);
                    distance++;
                }

                if (distance > 0)
                {
                    // Can only move if there's at least one piece in the way
                    Move move = new Move(targetPiece.PieceName, landingPosition);
                    validMoves.Add(move);
                }
            }

            return validMoves;
        }

        private MoveSet GetValidSoldierAntMovements(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            // Get all slides all the way around
            return GetValidSlides(targetPiece, null);
        }

        private MoveSet GetValidSlides(Piece targetPiece, int? maxRange)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            if (maxRange.HasValue && maxRange.Value < 1)
            {
                throw new ArgumentOutOfRangeException("maxRange");
            }

            MoveSet validMoves = new MoveSet();

            GetValidSlides(targetPiece, targetPiece.Position, 0, maxRange, validMoves);

            return validMoves;
        }

        private void GetValidSlides(Piece targetPiece, Position startingPosition, int currentRange, int? maxRange, MoveSet validMoves)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            if (null == startingPosition)
            {
                throw new ArgumentNullException("startingPosition");
            }

            if (maxRange.HasValue && maxRange.Value < 1)
            {
                throw new ArgumentOutOfRangeException("maxRange");
            }

            if (null == validMoves)
            {
                throw new ArgumentNullException("validMoves");
            }

            if (!maxRange.HasValue || currentRange < maxRange.Value)
            {
                foreach (Direction slideDirection in EnumUtils.Directions)
                {
                    Position slidePosition = targetPiece.Position.NeighborAt(slideDirection);

                    if (slidePosition != startingPosition && !HasPieceAt(slidePosition))
                    {
                        // Slide position is open

                        Direction right = EnumUtils.RightOf(slideDirection);
                        Direction left = EnumUtils.LeftOf(slideDirection);

                        if (HasPieceAt(targetPiece.Position.NeighborAt(right)) != HasPieceAt(targetPiece.Position.NeighborAt(left)))
                        {
                            // Can slide into slide position
                            Move move = new Move(targetPiece.PieceName, slidePosition);

                            if (validMoves.Add(move))
                            {
                                // Sliding from this position has not been tested yet
                                Position preSlidePosition = targetPiece.Position;
                                MovePiece(targetPiece, move.Position);
                                GetValidSlides(targetPiece, startingPosition, currentRange + 1, maxRange, validMoves);
                                MovePiece(targetPiece, preSlidePosition);
                            }
                        }
                    }
                }
            }
        }

        protected bool CanMoveWithoutBreakingHive(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            if (targetPiece.InPlay && targetPiece.Position.Stack == 0)
            {
                // Try edge heurestic
                int edges = 0;
                bool? lastHasPiece = null;
                foreach (Position neighbor in targetPiece.Position.Neighbors)
                {
                    bool hasPiece = HasPieceAt(neighbor);
                    if (lastHasPiece.HasValue)
                    {
                        if (lastHasPiece.Value != hasPiece)
                        {
                            edges++;
                            if (edges > 2)
                            {
                                break;
                            }
                        }
                    }
                    lastHasPiece = hasPiece;
                }

                if (edges <= 2)
                {
                    return true;
                }

                // Temporarily remove piece from board
                Position originalPosition = targetPiece.Position;
                MovePiece(targetPiece, null);

                // Determine if the hive is broken
                bool isOneHive = IsOneHive();

                // Return piece to the board
                MovePiece(targetPiece, originalPosition);

                return isOneHive;
            }

            return true;
        }

        protected bool PlacingPieceInOrder(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            if (targetPiece.InHand)
            {
                switch (targetPiece.PieceName)
                {
                    case PieceName.WhiteSpider2:
                        return GetPiece(PieceName.WhiteSpider1).InPlay;
                    case PieceName.WhiteBeetle2:
                        return GetPiece(PieceName.WhiteBeetle1).InPlay;
                    case PieceName.WhiteGrasshopper2:
                        return GetPiece(PieceName.WhiteGrasshopper1).InPlay;
                    case PieceName.WhiteGrassHopper3:
                        return GetPiece(PieceName.WhiteGrasshopper2).InPlay;
                    case PieceName.WhiteSoldierAnt2:
                        return GetPiece(PieceName.WhiteSoldierAnt1).InPlay;
                    case PieceName.WhiteSoldierAnt3:
                        return GetPiece(PieceName.WhiteSoldierAnt2).InPlay;
                    case PieceName.BlackSpider2:
                        return GetPiece(PieceName.BlackSpider1).InPlay;
                    case PieceName.BlackBeetle2:
                        return GetPiece(PieceName.BlackBeetle1).InPlay;
                    case PieceName.BlackGrasshopper2:
                        return GetPiece(PieceName.BlackGrasshopper1).InPlay;
                    case PieceName.BlackGrassHopper3:
                        return GetPiece(PieceName.BlackGrasshopper2).InPlay;
                    case PieceName.BlackSoldierAnt2:
                        return GetPiece(PieceName.BlackSoldierAnt1).InPlay;
                    case PieceName.BlackSoldierAnt3:
                        return GetPiece(PieceName.BlackSoldierAnt2).InPlay;
                }
            }

            return true;
        }

        protected void ResetCaches()
        {
            _cachedValidMovesByPiece = null;
            _cachedValidPlacementPositions = null;
            ValidMoveCacheResets++;

            _boardString = null;
            _transpositionKey = null;
        }

        #endregion

        public override string ToString()
        {
            return BoardString;
        }

        public const char BoardStringSeparator = ';';
    }

    public enum BoardState
    {
        NotStarted,
        InProgress,
        Draw,
        WhiteWins,
        BlackWins
    }

    [Serializable]
    public class InvalidMoveException : Exception
    {
        public Move Move { get; private set; }

        public InvalidMoveException(Move move) : this(move, "You can't move that piece there.") { }

        public InvalidMoveException(Move move, string message) : base(message)
        {
            Move = move;
        }
    }
}
