namespace OrionMassCommandSenderOld
{
    using System;

    public class LogMessageReceivedEventArgs:EventArgs
    {
        private string msg;

        public LogMessageReceivedEventArgs(string logMessage)
        {
            this.msg = logMessage;
        }

        public string Message
        {
            get
            {
                return this.msg;
            }
        }
    }
}