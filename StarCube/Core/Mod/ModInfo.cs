﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace StarCube.Core.Mod
{
    public class ModInfo
    {
        public readonly string modid;

        public readonly Assembly assembly;

        public readonly IMod modClass;

        internal ModInfo(string modid, Assembly assembly, IMod modClass)
        {
            this.modid = modid;
            this.assembly = assembly;
            this.modClass = modClass;
        }
    }
}