using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //Dns(Domain in Name System)
            string host = Dns.GetHostName();//로컬 컴퓨터의 호스트 이름을 가져옵니다.
            IPHostEntry ipHost = Dns.GetHostEntry(host);//IPHostEntry 클래스 별칭의 배열 및 IP 주소와 일치 하는 배열을 사용 하 여 도메인 이름 시스템 (DNS) 호스트 이름을 연결 합니다.
            IPAddress ipAddr = ipHost.AddressList[0];//호스트와 연결된 IP 주소 목록을 가져오거나 설정합니다.
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);//네트워크 엔드포인트를 IP 주소와 포트 번호로 나타냅니다.

            while(true)
            {
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    socket.Connect(endPoint);
                    Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()}");//RemoteEndPoint 연결된 원격 IP 주소 및 포트 번호를 Socket 포함하는 값을 가져옵니다

                    //보낸다
                    byte[] sendBuff = Encoding.UTF8.GetBytes("Hello world!");
                    int sendBytes = socket.Send(sendBuff);

                    //받는다
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = socket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From Server]{recvData}");

                    //나간다
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(100);
            }
           

        }
    }
}