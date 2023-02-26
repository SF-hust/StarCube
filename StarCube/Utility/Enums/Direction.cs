using System;
using System.Collections.Generic;
using System.Text;

namespace StarCube.Utility.Enums
{
    public enum Direction
    {
        None = 0x0,
        North = 0x1,
        East = 0x2,
        South = 0x4,
        West = 0x8,
        Up = 0x10,
        Down = 0x20,
    }

    public static class DirectionExtension
    {
        private static readonly Direction[] directions = new Direction[7]
        {
            Direction.None,
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West,
            Direction.Up,
            Direction.Down
        };

        public static Direction GetByIndex(int i)
        {
            return i >=0 && i < 7 ? directions[i] : Direction.None;
        }

        public static Direction ByIndex (this Direction direction, int i)
        {
            return GetByIndex(i);
        }
        
        public static int GetIndex(this Direction direction)
        {
            if (direction == Direction.North)
            {
                return 1;
            }
            if (direction == Direction.East)
            {
                return 2;
            }
            if (direction == Direction.South)
            {
                return 3;
            }
            if (direction == Direction.West)
            {
                return 4;
            }
            if (direction == Direction.Up)
            {
                return 5;
            }
            if (direction == Direction.Down)
            {
                return 6;
            }
            return 0;
        }

        public static Direction GetOpposite(this Direction direction)
        {
            if (direction == Direction.North)
            {
                return Direction.South;
            }
            if (direction == Direction.South)
            {
                return Direction.North;
            }
            if (direction == Direction.East)
            {
                return Direction.West;
            }
            if (direction == Direction.West)
            {
                return Direction.East;
            }
            if (direction == Direction.Up)
            {
                return Direction.Down;
            }
            if (direction == Direction.Down)
            {
                return Direction.Up;
            }
            return Direction.None;
        }

        public static Axis GetAxis(this Direction direction)
        {
            if (direction == Direction.East || direction == Direction.West)
            {
                return Axis.X;
            }
            if (direction == Direction.Up || direction == Direction.Down)
            {
                return Axis.Y;
            }
            if (direction == Direction.North || direction == Direction.South)
            {
                return Axis.Z;
            }
            return Axis.None;
        }
    }
}
