using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Program
    {
        static void Main(string[] args)
        {
            //Dns(Domain in Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listenSocket.Bind(endPoint);//Socket을 로컬 엔드포인트와 연결합니다.
                listenSocket.Listen(10); //backlog: 최대 대기수

                while (true)
                {
                    Console.WriteLine("Listening....");

                    //입장시킴
                    Socket clientSocket = listenSocket.Accept();//blocking 함수, 클라이언트가 오지 않으면 여기서 멈춤

                    //받는다
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = clientSocket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From Client] {recvData}");
                    //보낸다
                    byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to server!");
                    clientSocket.Send(sendBuff);//blocking 함수


                    //쫓아낸다
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();

                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
    }
}