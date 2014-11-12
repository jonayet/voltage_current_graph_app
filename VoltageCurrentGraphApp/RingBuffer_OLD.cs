using System;
using System.Collections.Generic;
using System.Text;

namespace VoltageCurrentGraphApp
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;

    namespace VoltageCurrentGraphApp
    {
        public class RingBuffer<T> where T : new()
        {
            private int _readIndex;
            private int _writeIndex;
            private int _lengthToRead;
            private readonly T[] _buffer;
            private readonly object _lockObject = new object();

            public RingBuffer(int size)
            {
                _buffer = new T[size];
                for (int i = 0; i < size; i++)
                {
                    _buffer[i] = new T();
                }
            }

            public int LengthToRead
            {
                get
                {
                    lock (_lockObject)
                    {
                        return _lengthToRead;
                    }
                }
            }

            public int RemainingLengthToWrite
            {
                get
                {
                    lock (_lockObject)
                    {
                        return _buffer.Length - _lengthToRead;
                    }
                }
            }

            public void PutNormal(T data)
            {
                lock (_lockObject)
                {
                    if (_lengthToRead == _buffer.Length) { return; }
                    _buffer[_writeIndex] = data;
                    _lengthToRead++;
                    _writeIndex = (_writeIndex + 1) % _buffer.Length;
                    Monitor.Pulse(_lockObject);
                }
            }

            public void PutNormal(T[] data, int startIndex, int length)
            {
                lock (_lockObject)
                {
                    for (int i = 0; i < length; i++)
                    {
                        if (_lengthToRead == _buffer.Length) { return; }
                        _buffer[_writeIndex] = data[startIndex + i];
                        _lengthToRead++;
                        _writeIndex = (_writeIndex + 1) % _buffer.Length;
                        Monitor.Pulse(_lockObject);
                    }
                }
            }

            public void PutBlocking(T data)
            {
                lock (_lockObject)
                {
                    while (_lengthToRead == _buffer.Length) { Monitor.Wait(_lockObject); }
                    _buffer[_writeIndex] = data;
                    _writeIndex = (_writeIndex + 1) % _buffer.Length;
                    _lengthToRead++;
                    Monitor.Pulse(_lockObject);
                }
            }

            public void PutBlocking(T[] data, int startIndex, int length)
            {
                lock (_lockObject)
                {
                    for (int i = 0; i < length; i++)
                    {
                        while (_lengthToRead == _buffer.Length) { Monitor.Wait(_lockObject, 100); }
                        _buffer[_writeIndex] = data[startIndex + i];
                        _writeIndex = (_writeIndex + 1) % _buffer.Length;
                        _lengthToRead++;
                        Monitor.Pulse(_lockObject);
                    }
                }
            }

            public void GetNormal(ref T data)
            {
                lock (_lockObject)
                {
                    if (_lengthToRead == 0) { return; }
                    data = _buffer[_readIndex];
                    _readIndex = (_readIndex + 1) % _buffer.Length;
                    _lengthToRead--;
                    Monitor.Pulse(_lockObject);
                }
            }

            public void GetNormal(T[] data, int length)
            {
                lock (_lockObject)
                {
                    for (int i = 0; i < length; i++)
                    {
                        if (_lengthToRead == 0) { Monitor.Wait(_lockObject); }
                        data[i] = _buffer[_readIndex];
                        _readIndex = (_readIndex + 1) % _buffer.Length;
                        _lengthToRead--;
                        Monitor.Pulse(_lockObject);
                    }
                }
            }

            public void GetBlocking(ref T data)
            {
                lock (_lockObject)
                {
                    while (_lengthToRead == 0) { Monitor.Wait(_lockObject); }
                    data = _buffer[_readIndex];
                    _readIndex = (_readIndex + 1) % _buffer.Length;
                    _lengthToRead--;
                    Monitor.Pulse(_lockObject);
                }
            }

            public void GetBlocking(T[] data, int length)
            {
                lock (_lockObject)
                {
                    for (int i = 0; i < length; i++)
                    {
                        while (_lengthToRead == 0) { Monitor.Wait(_lockObject, 100); }
                        data[i] = _buffer[_readIndex];
                        _readIndex = (_readIndex + 1) % _buffer.Length;
                        _lengthToRead--;
                        Monitor.Pulse(_lockObject);
                    }
                }
            }
        }
    }
}
