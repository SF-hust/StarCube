﻿using System;

using StarCube.Game.Levels;
using StarCube.Game.Levels.Generation;

namespace StarCube.Game
{
    public class ServerGame
    {
        public ServerGame(ILevelBound bound, ILevelGenerator generator)
        {
            level = new ServerLevel(Guid.NewGuid(), bound, generator);
        }

        public readonly ServerLevel level;
    }
}