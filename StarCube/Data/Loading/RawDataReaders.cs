using Newtonsoft.Json.Linq;

using StarCube.Utility;

namespace StarCube.Data.Loading
{
    public static class RawDataReaders
    {
        public static RawDataReader<JObject> JSON = JsonHelper.TryReadFromStreamSync;
    }
}
