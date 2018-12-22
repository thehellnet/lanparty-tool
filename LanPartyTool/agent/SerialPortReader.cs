namespace LanPartyTool.agent
{
    internal class SerialPortReader
    {
        public delegate void NewStatusHandler(Status status);

        public enum Status
        {
            Closed,
            Preparing,
            Waiting,
            Parsing,
            Closing
        }

        public event NewStatusHandler OnNewStatus;
    }
}