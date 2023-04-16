using System.Diagnostics;

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

        [Conditional("DEBUG")]
        public static void Trace(object message)
        {
            logger.Trace(message);
        }

        [Conditional("DEBUG")]
        public static void Debug(object message)
        {
            logger.Debug(message);
        }

        public static void Info(object message)
        {
            logger.Info(message);
        }

        public static void Warning(object message)
        {
            logger.Warning(message);
        }

        public static void Error(object message)
        {
            logger.Error(message);
        }

        public static void Fatal(object message)
        {
            logger.Fatal(message);
        }
    }
}
