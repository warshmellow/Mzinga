﻿// 
// BoardMetrics.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017 Jon Thysell <http://jonthysell.com>
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

using System.Collections.Generic;
using System.Linq;

namespace Mzinga.Core
{
    public class BoardMetrics
    {
        public BoardState BoardState { get; private set; }

        public int ValidMoveCount
        {
            get
            {
                return _pieceMetrics.Sum((pieceMetrics) => { return (null != pieceMetrics) ? pieceMetrics.ValidMoveCount : 0; });
            }
        }

        public int PiecesInPlayCount
        {
            get
            {
                return _pieceMetrics.Sum((pieceMetrics) => { return (null != pieceMetrics) ? pieceMetrics.InPlay : 0; });
            }
        }

        public int PiecesPinnedCount
        {
            get
            {
                return _pieceMetrics.Sum((pieceMetrics) => { return (null != pieceMetrics) ? pieceMetrics.IsPinned : 0; });
            }
        }

        public PieceMetrics this[PieceName pieceName]
        {
            get
            {
                return _pieceMetrics[(int)pieceName];
            }
            set
            {
                _pieceMetrics[(int)pieceName] = value;
            }
        }

        private PieceMetrics[] _pieceMetrics;

        public BoardMetrics(BoardState boardState)
        {
            BoardState = boardState;

            _pieceMetrics = new PieceMetrics[EnumUtils.NumPieceNames];
        }
    }
}
