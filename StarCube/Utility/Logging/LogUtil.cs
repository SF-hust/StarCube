namespace StarCube.Utility.Logging
{
    public static class LogUtil
    {
        public static ILogger Logger
        {
            get => logger;
            set => logger = value;
        }

        private static ILogger logger = new DummyLogger();
    }
}
