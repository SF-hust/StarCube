﻿using System;

namespace StarCube.Bootstrap.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class BootstrapClassAttribute : Attribute
    {
        public BootstrapClassAttribute(Type type)
        {
            this.type = type;
        }

        public readonly Type type;
    }
}
