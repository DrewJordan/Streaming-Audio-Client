using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;

namespace AudioClient
{
    public partial class Form1 : Form
    {
        System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
        BackgroundWorker bw = new BackgroundWorker();
        private string currentFile = "";
        int bytesRead = 0;
        private int totalLength = 0;
        private Dictionary<string,string> hosts = new Dictionary<string, string>();

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            button1.Click += button1_Click;
            bw.DoWork += bw_DoWork;
            bw.ProgressChanged += bw_ProgressChanged ;
            bw.RunWorkerCompleted += BwOnRunWorkerCompleted;
            bw.WorkerReportsProgress = true;
        }

        private void BwOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            progressBar1.Value = progressBar1.Maximum;
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.PerformStep();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listView1.View = View.SmallIcon;
            msg("Client Started");

            var x = ConfigurationManager.AppSettings;

            foreach (NameValueConfigurationElement v in x)
            {
                // get server address and port info from config file
                hosts.Add(v.Name,v.Value);
            }

            try
            {
                clientSocket.Connect(hosts["defaulthost"], Convert.ToInt32(hosts["defaultport"]));
            }
            catch (SocketException ex)
            {
                DialogResult r = MessageBox.Show("Couldn't Connect! Please add a valid host and port.");
                this.Close();
            }
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!clientSocket.Connected)
                return;
            NetworkStream serverStream = clientSocket.GetStream();
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(textBox2.Text + "$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            byte[] inStream = new byte[65536];

            int counter = 0;
            while (!serverStream.DataAvailable)
            {
                Thread.Sleep(50);
                counter++;
                if (counter > 50)
                    return;
            }
            serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);
            string returndata = System.Text.Encoding.ASCII.GetString(inStream);
            msg(returndata);
            textBox2.Text = "";
            textBox2.Focus();
        }

        public void msg(string mesg)
        {
            listView1.Items.Clear();

            string[] list = mesg.Split(';');

            foreach (var s in list)
            {
                listView1.Items.Add(s);
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            var s = (ListView) sender;
            if (s.SelectedItems.Count <= 0)
                return;

            if (!clientSocket.Connected)
                return;

            GetLength(s.SelectedItems[0].Text);
            currentFile = s.SelectedItems[0].Text;

            bw.RunWorkerAsync();

            
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            Read(currentFile);
        }

        void GetLength(string fileName)
        {
            NetworkStream serverStream = clientSocket.GetStream();
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes("gl" + fileName + "$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();



            int counter = 0;
            while (!serverStream.DataAvailable)
            {
                Thread.Sleep(50);
                counter++;
                if (counter > 50)
                    return;
            }

            byte[] inStream;
            using (var stream = new MemoryStream())
            {
                byte[] buffer = new byte[2048]; // read in chunks of 2KB
                int bytesRead;
                serverStream.Read(buffer, 0, buffer.Length);

                //byte[] result = stream.ToArray();
                inStream = buffer;
            }
            string s = Encoding.ASCII.GetString(inStream);
            totalLength = Convert.ToInt32(s);

            progressBar1.Maximum = totalLength;
            progressBar1.Step = 2048*100;
            progressBar1.Value = 0;
        }

        void Read(string fileName)
        {
            NetworkStream serverStream = clientSocket.GetStream();
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes("pl" + fileName + "$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();



            int counter = 0;
            while (!serverStream.DataAvailable)
            {
                Thread.Sleep(500);
                counter++;
                if (counter > 100)
                    return;
            }

            byte[] inStream = new byte[99999999];

            try
            {
                using (var stream = new MemoryStream())
                {
                    
                    bytesRead = 0;
                    int totalRead = 0;
                    int pRead = 0;
                    int toRead = totalLength;

                    while (toRead > 0)
                    {
                        byte[] buffer = new byte[totalLength]; 
                        int n = serverStream.Read(buffer, bytesRead, toRead);
                        // The end of the file is reached.
                        if (n == 0)
                        {
                            break;
                        }

                        stream.Write(buffer,bytesRead,n);
                        bytesRead += n;
                        bw.ReportProgress(bytesRead);
                        toRead -= n;
                    }


                    byte[] result = stream.ToArray();
                    inStream = result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


            using (MemoryStream ms = new MemoryStream(inStream))
            {
                // Construct the sound player
                SoundPlayer player = new SoundPlayer(ms);
                MediaPlayer m = new MediaPlayer(ms.ToArray());
                m.Play();
            }
        }
    }

    class MediaPlayer
    {
        System.Media.SoundPlayer soundPlayer;
        public MediaPlayer(byte[] buffer)
        {
            MemoryStream memoryStream = new MemoryStream(buffer, true);
            soundPlayer = new System.Media.SoundPlayer(memoryStream);
        }
        public void Play() { soundPlayer.Play(); }
        public void Play(byte[] buffer)
        {
            soundPlayer.Stream.Seek(0, SeekOrigin.Begin);
            soundPlayer.Stream.Write(buffer, 0, buffer.Length);
            soundPlayer.Play();
        }
    }
}
