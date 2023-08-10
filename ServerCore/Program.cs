using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Program
    {
        static Listener _listener = new Listener();
        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                //받는다
                byte[] recvBuff = new byte[1024];
                int recvBytes = clientSocket.Receive(recvBuff);
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                Console.WriteLine($"[From Client] {recvData}");
                //보낸다
                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to server!");
                clientSocket.Send(sendBuff);//blocking 함수
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
              
          
        }
        static void Main(string[] args)
        {
            //Dns(Domain in Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

          
           _listener.Init(endPoint,OnAcceptHandler);
            Console.WriteLine("Listening....");

                while (true)
                {
                    ;
                }


         

        }
    }
}