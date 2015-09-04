using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AudioService;

namespace AudioClient
{
    public partial class Form1 : Form
    {
        Proxy _proxy;
        BackgroundWorker bw = new BackgroundWorker();
        private string currentFile = "";
        int bytesRead = 0;
        private int totalLength = 0;
        private List<Host> hosts = new List<Host>();

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
            //listView1.View = View.SmallIcon;
            dirList.View = View.List;
            dirList.Items.Clear();
            msg("Client Started");

            var x = ConfigurationManager.AppSettings;


            // get server address and port info from config file
            hosts.Add(new Host("default",x.Get(0),Convert.ToInt32(x.Get(1))));

            try
            {
                _proxy = new Proxy(hosts[0]);
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Couldn't Connect! Please add a valid host and port.");
                this.Close();
            }

            if (_proxy.Connected())
            {
                var bytes = _proxy.ListDir(@"C:\");
                var s = Encoding.ASCII.GetString(bytes);
                var list = s.Split(';').ToList(); 
                StringBuilder b = new StringBuilder();
                list.ForEach(d =>
                {
                    dirList.Items.Add(d);
                });
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //if (!clientSocket.Connected)
            //    return;
            //NetworkStream serverStream = clientSocket.GetStream();
            //byte[] outStream = Encoding.ASCII.GetBytes(textBox2.Text + "$");
            //serverStream.Write(outStream, 0, outStream.Length);
            //serverStream.Flush();

            //// ok
            //byte[] inStream = new byte[65536];

            //int counter = 0;
            //while (!serverStream.DataAvailable)
            //{
            //    Thread.Sleep(50);
            //    counter++;
            //    if (counter > 50)
            //        return;
            //}
            //serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);
            //string returndata = Encoding.ASCII.GetString(inStream);
            //msg(returndata);
            //textBox2.Text = "";
            //textBox2.Focus();
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

            if (!_proxy.Connected())
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
            totalLength = _proxy.GetLength(fileName);

            progressBar1.Maximum = totalLength;
            progressBar1.Step = 2048*100;
            progressBar1.Value = 0;
        }

        void Read(string fileName)
        {
            //NetworkStream serverStream = clientSocket.GetStream();
            //byte[] outStream = Encoding.ASCII.GetBytes("pl" + fileName + "$");
            //serverStream.Write(outStream, 0, outStream.Length);
            //serverStream.Flush();

            byte[] result = new byte[99999999];

            //int counter = 0;
            //while (!serverStream.DataAvailable)
            //{
            //    Thread.Sleep(500);
            //    counter++;
            //    if (counter > 100)
            //        return;
            //}


            //try
            //{
            //    using (var stream = new MemoryStream())
            //    {
                    
            //        bytesRead = 0;
            //        int totalRead = 0;
            //        int pRead = 0;
            //        int toRead = totalLength;

            //        while (toRead > 0)
            //        {
            //            byte[] buffer = new byte[totalLength]; 
            //            int n = serverStream.Read(buffer, bytesRead, toRead);
            //            // The end of the file is reached.
            //            if (n == 0)
            //            {
            //                break;
            //            }

            //            stream.Write(buffer,bytesRead,n);
            //            bytesRead += n;
            //            bw.ReportProgress(bytesRead);
            //            toRead -= n;
            //        }


            //        result = stream.ToArray();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}

            result = _proxy.Play(currentFile);

            //result = _proxy.Play(fileName);

            // Construct the sound player
            MediaPlayer m = new MediaPlayer(result);
            m.Play();
        }

        private void dirList_DoubleClick(object sender, EventArgs e)
        {
            // do stuff
            _proxy.ListDir(dirList.SelectedItems[0].Text);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _proxy.RequestClose();
            _proxy.Dispose();
        }
    }

    class MediaPlayer
    {
        SoundPlayer soundPlayer;
        public MediaPlayer(byte[] buffer)
        {
            MemoryStream memoryStream = new MemoryStream(buffer, true);
            soundPlayer = new SoundPlayer(memoryStream);
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
