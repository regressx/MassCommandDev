namespace OrionMassCommandSenderOld
{
    using System;

    public static class Logger
    {
        public static event EventHandler<LogMessageReceivedEventArgs> LogMessageReceived;

        public static void AddLog(string Message)
        {
            if (Logger.LogMessageReceived == null)
                return;
            Logger.LogMessageReceived((object) null, new LogMessageReceivedEventArgs(Message));
        }
    }
}