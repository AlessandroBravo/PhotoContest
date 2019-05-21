namespace TSAC.Bravo.PhotoContest.Queue
{
    public interface IQueueAccess
    {
        void SendToQueue(string message);
    }
}