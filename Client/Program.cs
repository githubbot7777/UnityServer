using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            //보낸다
            for (int i = 0; i < 5; i++)
            {
                byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello world! {i}");
                Send(sendBuff);
            }

        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server]{recvData}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            //Dns(Domain in Name System)
            string host = Dns.GetHostName();//로컬 컴퓨터의 호스트 이름을 가져옵니다.
            IPHostEntry ipHost = Dns.GetHostEntry(host);//IPHostEntry 클래스 별칭의 배열 및 IP 주소와 일치 하는 배열을 사용 하 여 도메인 이름 시스템 (DNS) 호스트 이름을 연결 합니다.
            IPAddress ipAddr = ipHost.AddressList[0];//호스트와 연결된 IP 주소 목록을 가져오거나 설정합니다.
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);//네트워크 엔드포인트를 IP 주소와 포트 번호로 나타냅니다.

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return new GameSession(); });
            while(true)
            {
                

                try
                {
                   
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