﻿// 
// Position.cs
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
using System.Collections.Generic;
using System.Linq;

namespace Mzinga.Core
{
    public class Position : IEquatable<Position>, IComparable<Position>
    {
        public static Position Origin
        {
            get
            {
                if (null == _origin)
                {
                    _origin = new Position(0, 0, 0, 0);
                }
                return _origin;
            }
        }
        private static Position _origin;

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }

        public int Stack { get; private set; }

        public int Q
        {
            get
            {
                return X;
            }
        }

        public int R
        {
            get
            {
                return Z;
            }
        }

        public IEnumerable<Position> Neighbors
        {
            get
            {
                for (int i = 0; i < _neighborDeltas.Length; i++)
                {
                    yield return NeighborAt((Direction)i);
                }
            }
        }

        public Position(int x, int y, int z, int stack)
        {
            if (x + y + z != 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (stack < 0)
            {
                throw new ArgumentOutOfRangeException("stack");
            }

            X = x;
            Y = y;
            Z = z;

            Stack = stack;
        }

        public Position(int q, int r, int stack)
        {
            if (stack < 0)
            {
                throw new ArgumentOutOfRangeException("stack");
            }

            X = q;
            Z = r;
            Y = 0 - q - r;

            Stack = stack;
        }

        public bool IsTouching(Position position)
        {
            if (null == position)
            {
                throw new ArgumentNullException("position");
            }

            return Neighbors.Contains(position);
        }

        public Position NeighborAt(Direction direction, int deltaStack = 0)
        {
            int dirIndex = (int)direction;
            return GetShifted(_neighborDeltas[dirIndex][0], _neighborDeltas[dirIndex][1], _neighborDeltas[dirIndex][2], deltaStack);
        }

        public Position NeighborAt(Direction[] directions, int deltaStack = 0)
        {
            if (null == directions)
            {
                throw new ArgumentNullException("directions");
            }

            Position neighbor = this;
            for (int i = 0; i < directions.Length; i++)
            {
                int dirIndex = (int)directions[i];
                neighbor = neighbor.GetShifted(_neighborDeltas[dirIndex][0], _neighborDeltas[dirIndex][1], _neighborDeltas[dirIndex][2], deltaStack);
            }

            return neighbor;
        }

        public Position GetShifted(int deltaX, int deltaY, int deltaZ, int deltaStack = 0)
        {
            return new Position(X + deltaX, Y + deltaY, Z + deltaZ, Stack + deltaStack);
        }

        public Position GetRotatedRight()
        {
            return new Position(-Z, -X, -Y, Stack);
        }

        public Position GetRotatedLeft()
        {
            return new Position(-Y, -Z, -X, Stack);
        }

        public Position Clone()
        {
            return new Position(X, Y, Z, Stack);
        }

        public static Position Parse(string positionString)
        {
            Position position;
            if (TryParse(positionString, out position))
            {
                return position;
            }

            throw new ArgumentOutOfRangeException("positionString");
        }

        public static bool TryParse(string positionString, out Position position)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(positionString))
                {
                    position = null;
                    return true;
                }

                positionString = positionString.Trim();

                string[] split = positionString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length == 2)
                {
                    int q = Int32.Parse(split[0]);
                    int r = Int32.Parse(split[1]);

                    position = new Position(q, r, 0);
                    return true;
                }
                else if (split.Length >= 3)
                {
                    int x = Int32.Parse(split[0]);
                    int y = Int32.Parse(split[1]);
                    int z = Int32.Parse(split[2]);
                    int stack = split.Length > 3 ? Int32.Parse(split[3]) : 0;

                    position = new Position(x, y, z, stack);
                    return true;
                }
            }
            catch (Exception) { }

            position = null;
            return false;
        }

        public bool Equals(Position pos)
        {
            if (null == pos)
            {
                return false;
            }

            return X == pos.X && Y == pos.Y && Z == pos.Z && Stack == pos.Stack;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Position);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(Position a, Position b)
        {
            if (object.ReferenceEquals(a, null))
            {
                return object.ReferenceEquals(b, null);
            }

            return a.Equals(b);
        }
        public static bool operator !=(Position a, Position b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            if (Stack > 0)
            {
                return String.Format("{0},{1},{2},{3}", X, Y, Z, Stack);
            }

            return String.Format("{0},{1},{2}", X, Y, Z);
        }

        public int CompareTo(Position position)
        {
            if (null == position)
            {
                throw new ArgumentNullException("position");
            }

            int xCompare = X.CompareTo(position.X);

            if (xCompare != 0)
            {
                return xCompare;
            }

            int yCompare = Y.CompareTo(position.Y);

            if (yCompare != 0)
            {
                return yCompare;
            }

            int zCompare = Z.CompareTo(position.Z);

            if (zCompare != 0)
            {
                return zCompare;
            }

            return Stack.CompareTo(position.Stack);
        }

        private static int[][] _neighborDeltas = new int[][]
        {
            new int[] { 0, 1, -1 },
            new int[] { 1, 0, -1 },
            new int[] { 1, -1, 0 },
            new int[] { 0, -1, 1 },
            new int[] { -1, 0, 1 },
            new int[] { -1, 1, 0 },
        };
    }
}
