using System;

namespace StarCube.Utility.Guids
{
    public class SystemGuidGenerator : IGuidGenerator
    {
        public Guid Generate()
        {
            return Guid.NewGuid();
        }
    }
}
