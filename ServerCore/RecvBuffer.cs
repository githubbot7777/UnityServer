using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public class RecvBuffer
    {
        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;

        public RecvBuffer(int bufferSize)
        { 
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);

           
           




        }

        //아직 처리 되지 않은 데이터 크기
        public int DataSize { get { return _writePos-_readPos; } }
        public int FreeSize { get { return _buffer.Count-_writePos; } }

        public ArraySegment<byte> ReadSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }

        }

        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }

        }

        public void Clean()
        {
            int dataSize = DataSize;
            if(dataSize==0)
            {   //남은 데이터 없으면 복사하지 않고 커서 위치만 리셋
                //[][][][][][rw][][]
                _readPos = _writePos = 0;
            }
            else
            {
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
               
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize)
                return false;

            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
                return false;

            _writePos+= numOfBytes;
            return true;
        }


    }
}
