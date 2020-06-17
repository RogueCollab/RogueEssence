using System;
using System.Threading;
using LiteNetLib;

namespace WaypointServer
{
    class Program
    {
        static void Main(string[] args)
        {

            DiagManager.InitInstance();

            ConnectionManager connectionManager = new ConnectionManager();

            EventBasedNetListener listener = new EventBasedNetListener();
            NetManager server = new NetManager(listener);

            listener.ConnectionRequestEvent += connectionManager.ClientRequested;
            listener.PeerConnectedEvent += connectionManager.ClientConnected;
            listener.PeerDisconnectedEvent += connectionManager.ClientDisconnected;
            listener.NetworkReceiveEvent += connectionManager.NetworkReceived;

            server.Start(DiagManager.Instance.Port);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Name:{0}", DiagManager.Instance.ServerName);
                Console.WriteLine("Port:{0}", DiagManager.Instance.Port);
                Console.WriteLine("Searching:{0}/{1}", connectionManager.Searching, connectionManager.Peers);
                Console.WriteLine("Active:{0}/{1}", connectionManager.Active, connectionManager.Peers);
                Console.WriteLine("Errors:{0}", DiagManager.Instance.Errors);
                for (int ii = 0; ii < 100; ii++)
                {
                    server.PollEvents();
                    Thread.Sleep(15);
                }
            }
            //server.Stop(true);
        }

    }
}
