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
        public const string NORTH = "north";
        public const string EAST = "east";
        public const string SOUTH = "south";
        public const string WEST = "west";
        public const string UP = "up";
        public const string DOWN = "down";

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

        public static Direction Parse(string directionString)
        {
            if (directionString == NORTH)
            {
                return Direction.North;
            }
            if (directionString == EAST)
            {
                return Direction.East;
            }
            if (directionString == SOUTH)
            {
                return Direction.South;
            }
            if (directionString == WEST)
            {
                return Direction.West;
            }
            if (directionString == UP)
            {
                return Direction.Up;
            }
            if (directionString == DOWN)
            {
                return Direction.Down;
            }
            return Direction.None;
        }

        public static string ToDirectionString(this Direction direction)
        {
            if (direction == Direction.North)
            {
                return NORTH;
            }
            if (direction == Direction.East)
            {
                return EAST;
            }
            if (direction == Direction.South)
            {
                return SOUTH;
            }
            if (direction == Direction.West)
            {
                return WEST;
            }
            if (direction == Direction.Up)
            {
                return UP;
            }
            if (direction == Direction.Down)
            {
                return DOWN;
            }
            return string.Empty;
        }
    }
}
