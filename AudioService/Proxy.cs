using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AudioService
{
    public class Proxy : IDisposable
    {
        private readonly TcpClient _clientSocket = new TcpClient();
        private Host _host = new Host();

        public Proxy(Host host)
        {
            _host = host;
            Connect();
        }

        private bool Connect()
        {
            try
            {
                _clientSocket.Connect(_host.IP, _host.Port);
            }
            catch (SocketException se)
            {
                throw se;
            }
            return _clientSocket.Connected;
           // return true;
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
            var y = Convert.ToInt32(Encoding.UTF8.GetString(x));
            return y;
        }

        private byte[] SendAndReceive(string msg)
        {
            Send(msg, _clientSocket.GetStream());
            //int numBytes = ReceiveLength(_clientSocket.GetStream());
            byte[] ret = Receive(_clientSocket.GetStream());
            return ret; 
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

        //private int ReceiveLength(NetworkStream stream)
        //{

        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        int numBytesRead;
        //        do 
        //        {
        //            ms.Write(data, 0, numBytesRead);
        //        }
        //        while ((numBytesRead = stream.Read(data, 0, data.Length)) > 0 && stream.DataAvailable);
        //        bytes = ms.ToArray();
        //    }
        //}

        private byte[] Receive(NetworkStream stream)
        {
            byte[] bytes;
            byte[] data = new byte[1024];
            using (MemoryStream ms = new MemoryStream())
            {

                int numBytesRead;
                do
                {
                    numBytesRead = stream.Read(data, 0, data.Length);
                    ms.Write(data, 0, numBytesRead);
                } while (numBytesRead > 0 && stream.DataAvailable);
                bytes = ms.ToArray();
            }

            //int counter = 0;

            //int n = 0;
            //if (stream.CanRead)
            //{    
            //    using (var writer = new MemoryStream())
            //    {

            //        var buf = new byte[4096];
            //        for (; ; )
            //        {
            //            if (!stream.CanRead) break;
            //            var cnt = stream.Read(buf, 0, buf.Length);
            //            if (cnt == 0) break;
            //            writer.Write(buf, 0, cnt);
            //        }
                   
            //        return writer.ToArray();
            //    }


            //}
            return bytes;
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
            _clientSocket.Close();
            //Send("cl",_clientSocket.GetStream());
        }
    }
}
