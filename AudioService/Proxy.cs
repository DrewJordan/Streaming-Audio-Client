using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AudioService
{
    public class Proxy : IDisposable
    {
        private readonly TcpClient _clientSocket = new TcpClient();

        public Proxy(Host host)
        {
            try
            {
                _clientSocket.Connect(host.IP, host.Port);
            }
            catch (SocketException ex)
            {              
                throw ex;
            }
            
        }

        public bool Connected()
        {
            return _clientSocket.Connected;
        }

        public byte[] ListDir(string dirString)
        {
            return SendAndReceive("ls" + dirString);
        }

        public int GetLength(string filePath)
        {
            var x = SendAndReceive("gl" + filePath);
            return x.Length;
        }

        private byte[] SendAndReceive(string msg)
        {
            using (NetworkStream stream = _clientSocket.GetStream())
            {
                Send(msg, stream);
                byte[] ret = Receive(stream);
                return ret; 
            }
        }

        private void Send(string msg, NetworkStream stream)
        {
            if (!_clientSocket.Connected)
                throw new SocketException((int)SocketError.NotConnected);
            if (string.IsNullOrEmpty(msg))
                throw new ArgumentNullException();

            byte[] outStream = Encoding.ASCII.GetBytes(msg + "$");
            stream.Write(outStream, 0, outStream.Length);
            stream.Flush();
        }

        private byte[] Receive(NetworkStream stream)
        {
            byte[] inStream = new byte[_clientSocket.ReceiveBufferSize];

            int counter = 0;

            int n = 0;
            if (stream.CanRead)
            {    
                using (var writer = new MemoryStream())
                {
                    do
                    {
                        n = stream.Read(inStream, 0, (int) _clientSocket.ReceiveBufferSize);
                        if (n == 0)
                            break;
                        writer.Write(inStream,0,n);
                    } while (stream.DataAvailable);

                    return writer.ToArray();
                }
            }
            return inStream;
        }

        public byte[] Play(string fileName)
        {
            var x = SendAndReceive("pl" + fileName);
            return x;
        }

        public void Dispose()
        {
            //_clientSocket.Close();
        }

        public void RequestClose()
        {
            //Send("cl",_clientSocket.GetStream());
        }
    }
}
