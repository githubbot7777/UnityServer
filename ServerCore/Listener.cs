using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    public class Listener
    {
        Socket _listenSocket;
        Func<Session> _sessionFactory;

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            _listenSocket.Bind(endPoint);//Socket을 로컬 엔드포인트와 연결합니다.
            _listenSocket.Listen(10); //backlog: 최대 대기수

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            bool pending= _listenSocket.AcceptAsync(args);//비동기 방식의 Accept
            if(pending==false)//바로 처리 되는 경우(동기)
                OnAcceptCompleted(null,args);
           
        }

        void OnAcceptCompleted(object sender,SocketAsyncEventArgs args)
        {
            if(args.SocketError==SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
               
            }
            else
                Console.WriteLine(args.SocketError.ToString());

          
            RegisterAccept(args);//다음번 클라이언트를 위해서 다시 등록
        }
       
    }
}
