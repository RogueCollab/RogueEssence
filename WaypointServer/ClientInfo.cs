namespace WaypointServer
{
    public class ClientInfo
    {
        public byte[] Data;
        public int Activity;
        public int ToActivity;

        public ClientInfo(byte[] data, int activity, int toActivity)
        {
            Data = data;
            Activity = activity;
            ToActivity = toActivity;
        }
    }
}
