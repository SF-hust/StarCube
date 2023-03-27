namespace StarCube.Utility.Logging
{
    public interface ILogger
    {
        public void Trace(object message);
        public void Debug(object message);
        public void Info(object message);
        public void Warning(object message);
        public void Error(object message);
        public void Fatal(object message);
    }
}
