﻿// 
// BoardTests.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015 Jon Thysell <http://jonthysell.com>
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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mzinga.Core;

namespace Mzinga.CoreTest
{
    [TestClass]
    public class BoardTests
    {
        [TestMethod]
        public void Board_NewTest()
        {
            Board b = new Board();
            Assert.IsNotNull(b);
        }

        [TestMethod]
        public void Board_NewBoardValidMovesTest()
        {
            Board b = new Board();
            Assert.IsNotNull(b);

            MoveSet validMoves = b.GetValidMoves();
            Assert.IsNotNull(validMoves);
            Assert.AreEqual(10, validMoves.Count);

            b.Play(new Move(PieceName.WhiteSpider1, Position.Origin));

            validMoves = b.GetValidMoves();
            Assert.IsNotNull(validMoves);
            Assert.AreEqual(60, validMoves.Count);
        }

        [TestMethod]
        public void Board_ValidMovesTest()
        {
            Board b = new Board();
            Assert.IsNotNull(b);

            b.Play(new Move(PieceName.WhiteSpider1, Position.Origin));
            b.Play(new Move(PieceName.BlackSpider1, Position.Origin.NeighborAt(Direction.Up)));

            MoveSet validMoves = b.GetValidMoves();
            Assert.IsNotNull(validMoves);
        }

        [TestMethod]
        public void Board_QueenMustPlayByFourthMoveValidMovesTest()
        {
            Board b = new Board();
            Assert.IsNotNull(b);

            // Turn 1
            b.Play(new Move(PieceName.WhiteSpider1, Position.Origin));
            b.Play(new Move(PieceName.BlackSpider1, Position.Origin.NeighborAt(Direction.Up)));

            // Turn 2
            b.Play(new Move(PieceName.WhiteSpider2, Position.Origin.NeighborAt(Direction.Down)));
            b.Play(new Move(PieceName.BlackSpider2, Position.Origin.NeighborAt(StraightLine(Direction.Up, 2))));

            // Turn 3
            b.Play(new Move(PieceName.WhiteSoldierAnt1, Position.Origin.NeighborAt(StraightLine(Direction.Down, 2))));
            b.Play(new Move(PieceName.BlackSoldierAnt1, Position.Origin.NeighborAt(StraightLine(Direction.Up, 3))));

            // Turn 4
            MoveSet validMoves = b.GetValidMoves();
            Assert.IsNotNull(validMoves);
            Assert.AreEqual(7, validMoves.Count);

            foreach (Move move in validMoves)
            {
                Assert.AreEqual(PieceName.WhiteQueenBee, move.PieceName);
            }
        }

        [TestMethod]
        public void Board_CanCommitSuicideTest()
        {
            Board b = new Board();
            Assert.IsNotNull(b);

            // Turn 1
            b.Play(new Move(PieceName.WhiteSpider1, Position.Origin));
            b.Play(new Move(PieceName.BlackSpider1, Position.Origin.NeighborAt(Direction.Up)));

            // Turn 2
            b.Play(new Move(PieceName.WhiteQueenBee, Position.Origin.NeighborAt(Direction.Down)));
            b.Play(new Move(PieceName.BlackQueenBee, Position.Origin.NeighborAt(StraightLine(Direction.Up, 2))));

            // Turn 3
            b.Play(new Move(PieceName.WhiteSpider2, b.GetPiece(PieceName.WhiteQueenBee).Position.NeighborAt(Direction.UpLeft)));
            b.Play(new Move(PieceName.BlackSpider2, Position.Origin.NeighborAt(StraightLine(Direction.Up, 3))));

            // Turn 4
            b.Play(new Move(PieceName.WhiteSoldierAnt1, b.GetPiece(PieceName.WhiteQueenBee).Position.NeighborAt(Direction.UpRight)));
            b.Play(new Move(PieceName.BlackSoldierAnt1, Position.Origin.NeighborAt(StraightLine(Direction.Up, 4))));

            // Turn 5
            b.Play(new Move(PieceName.WhiteSoldierAnt2, b.GetPiece(PieceName.WhiteQueenBee).Position.NeighborAt(Direction.DownLeft)));
            b.Play(new Move(PieceName.BlackSoldierAnt2, Position.Origin.NeighborAt(StraightLine(Direction.Up, 5))));

            // Turn 6
            b.Play(new Move(PieceName.WhiteSoldierAnt3, b.GetPiece(PieceName.WhiteQueenBee).Position.NeighborAt(Direction.DownRight)));
            b.Play(new Move(PieceName.BlackSoldierAnt3, Position.Origin.NeighborAt(StraightLine(Direction.Up, 6))));

            // Turn 7
            b.Play(new Move(PieceName.WhiteBeetle1, b.GetPiece(PieceName.WhiteQueenBee).Position.NeighborAt(Direction.Down)));

            Assert.AreEqual(BoardState.BlackWins, b.BoardState);

            MoveSet validMoves = b.GetValidMoves();
            Assert.IsNotNull(validMoves);
            Assert.AreEqual(0, validMoves.Count);
        }

        private Direction[] StraightLine(Direction direction, int length)
        {
            if (length <= 0)
            {
                throw new ArgumentNullException("length");
            }

            Direction[] line = new Direction[length];

            for (int i = 0; i < line.Length; i++)
            {
                line[i] = direction;
            }

            return line;
        }
    }
}
