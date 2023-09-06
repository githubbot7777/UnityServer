﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;
        //[size(2)][packetId(2)][...][size(2)][packetId(2)]
        //다른 클래스가 PacketSession 상속후 OnRecv를 override 못함
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int processLen = 0;

            while(true)
            {
                //최소한 헤더는 파싱할 수 있는지 확인
                if (buffer.Count < HeaderSize)
                    break;

                //패킷이 완전체로 도착했는지 확인
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                    break;

                //여기까지 왔으면 패킷 조립 가능
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset+dataSize, buffer.Count-dataSize);

            }
            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }


    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;//socket.Disconnect() 두번 연속으로 하는 것을 방지하기 위해
        RecvBuffer _recvBuffer = new RecvBuffer(1024);
        object _lock = new object();
        
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();//매번 RegisterSend 하는게 아닌 큐에다가 보낼 것을 저장 해놓음

        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);//얼마만큼의 데이터 처리했는지 int 반환해줌
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        void Clear()
        {
            lock(_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();

            }
        }
        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
        
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }
        
        public void Send(ArraySegment<byte> sendBuff)
        {
            lock(_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
     
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();
        }

        #region 네트워크 통신

        void RegisterSend()
        {
            if (_disconnected == 1)
                return;

            while (_sendQueue.Count>0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff);
            }
               //Dequeue 한번에 sendAsync 한번에서 개선
               //리스트에 큐에 저장된 데이터 모은후 한번에 보낸다.
               _sendArgs.BufferList = _pendingList;

            try
            {
                bool pending = _socket.SendAsync(_sendArgs);
                if (pending == false)
                    OnSendCompleted(null, _sendArgs);

            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterSend Failed {e}");
            }
          
        }
        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock(_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);
                        

                        //보내는 동안에 큐에 누가 데이터를 넣은경우 체크
                        if(_sendQueue.Count>0)
                            RegisterSend();
                       
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}"); ;
                    }
                }
                else
                {
                    Disconnect();
                }
            }
         
        }
        void RegisterRecv()
        {
            if (_disconnected == 1)
                return;

            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = _socket.ReceiveAsync(_recvArgs);
                if (pending == false)
                    OnRecvCompleted(null, _recvArgs);
            }
            catch(Exception e)
            {
                Console.WriteLine($"RegisterRecv {e}");
            }
           
        }

        void OnRecvCompleted(object sender,SocketAsyncEventArgs args)
        {
            if(args.BytesTransferred>0 && args.SocketError==SocketError.Success)
            {
                    try
                    {
                        // write 커서 이동
                        if(_recvBuffer.OnWrite(args.BytesTransferred)==false)
                        {
                            Disconnect();
                            return;
                        }
                        // 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다
                        int processLen=OnRecv(_recvBuffer.ReadSegment);
                        if(processLen<0||_recvBuffer.DataSize<processLen)
                        {
                             Disconnect();
                             return;
                        }
                        //Read 커서 이동
                        if (_recvBuffer.OnRead(processLen) == false)
                        {
                            Disconnect();
                            return;
                        }
                        RegisterRecv();
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine($"OnRecvCompleted Failed {e}"); ;
                    }
            }
            else
            {
                Disconnect();
            }
         }
     }
        #endregion
}
