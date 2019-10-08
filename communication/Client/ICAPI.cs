using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Communication.CAPI
{
    interface ICAPI
    {
        bool isConnected();
        int MyPlayer();
        int PlayerCount();
        void GetPos(int client, out double x, out double y);
        void Move(MOVE_DIRECTION direction);
        long Time();
        void Stop();
        void ConnectServer(IPEndPoint endPoint);
    }
}
