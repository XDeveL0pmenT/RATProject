using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;


using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;


using System.Net;
using System.Net.Sockets;
using System.IO;






namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
        //  public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);


        const int KEYEVENTF_KEYDOWN = 0x0000;
        const int KEYEVENTF_KEYUP = 0x0002;


        //private void PressKey(int key)
        //{
        //    keybd_event((byte)key, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        //    keybd_event((byte)key, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        //}

        bool Server = true;
        bool Languege = false; // False: Eng True: Ru
        string localIP = "?";
        string ChosenIP = "?";
        string ServerPort = "?";
        string ChosenPort = "?";
        bool ClientConnected = false;
        public string KeysToSent;
        private TcpClient Client;
        private TcpListener listener;
        public StreamReader STR;
        public StreamWriter STW;

        public string DataToSend;

        public Form1()
        {
            InitializeComponent();


            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }

            this.textBox2.Text = localIP;

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (Server)
            {
                ServerPort = textBox1.Text;
            }
            else
            {
                ChosenPort = textBox1.Text;
            }
        }

        private void label1_Click_2(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Languege)
            {
                button1.Text = "Подключится";
                label2.Text = "Подключится";
            }
            else
            {
                button1.Text = "Connect";
                label2.Text = "Connecting";
            }
            this.Text = "Client";
            Server = false;
            this.textBox2.Text = ChosenIP;
            this.textBox1.Text = ChosenPort;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            ChosenIP = this.textBox2.Text;
            if (Languege)
            {
                button1.Text = "Поделится";
                label2.Text = "Поделится";
            }
            else
            {
                button1.Text = "Share";
                label2.Text = "Sharing";
            }
            this.Text = "Server";
            this.textBox2.Text = localIP;
            Server = true;
            this.textBox1.Text = ServerPort;
        }



        private async Task AMessage(string message)
        {
            MessageBox.Show(message);
        }

        private async Task StartServerAsync()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, int.Parse(textBox1.Text));
                listener.Start();
                if (Languege)
                {
                    textBox3.AppendText("Ожидание клиента...\n");
                }
                else
                {
                    textBox3.AppendText("Waiting for client...\n");
                }

                Client = await listener.AcceptTcpClientAsync();

                if (Languege)
                {
                    textBox3.AppendText("Клиент подключен!\n");
                }
                else
                {
                    textBox3.AppendText("Client connected!\n");
                }

                STR = new StreamReader(Client.GetStream());
                STW = new StreamWriter(Client.GetStream()) { AutoFlush = true };
            }
            catch (Exception ex)
            {
                await AMessage(ex.Message);
            }
        }


        private async Task StartClientAsync()
        {
            try
            {
                Client = new TcpClient();
                if (Languege)
                {
                    textBox3.AppendText("Подключение к серверу...\n");
                }
                else
                {
                    textBox3.AppendText("Connecting to server...\n");
                }

                await Client.ConnectAsync(
                    IPAddress.Parse(textBox2.Text),
                    int.Parse(textBox1.Text)
                );

                STR = new StreamReader(Client.GetStream());
                STW = new StreamWriter(Client.GetStream()) { AutoFlush = true };
                ClientConnected = true;

                if (Languege)
                {
                    textBox3.AppendText("Подключено!\n");
                }
                else
                {
                    textBox3.AppendText("Connected!\n");
                }

            }
            catch (Exception ex)
            {
                await AMessage(ex.Message);
            }
        }


        List<int> HoldedKeys = new List<int>();

        private async Task UpdateKey()
        {
            while (ClientConnected)
            {
                HoldedKeys.Clear();
                for (int i = 0; i < 256; i++)
                {
                    if ((GetAsyncKeyState(i) & 0x8000) != 0)
                    {
                        HoldedKeys.Add(i);
                    }
                }


                if (HoldedKeys.Count > 0 && Client.Connected)
                {
                    string SendToServer = string.Join(",", HoldedKeys);
                    STW.WriteLine(SendToServer);
                }

                await Task.Delay(10);
            }
        }




        private async void ConnectOrShare(object sender, EventArgs e)
        {
            if (Server)
            {
                await StartServerAsync();
            }
            else
            {

                await StartClientAsync();
                await UpdateKey();
            }

        }


        private void backgroundworker1_DoWork()
        {
            while (Client.Connected)
            {
                try
                {
                    DataToSend = STR.ReadLine();

                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

        private async void Stop(object sender, EventArgs e)
        {
            if (Server)
            {
                if (listener != null)
                {

                    listener.Stop();
                    listener = null;

                    if (Languege)
                    {
                        await AMessage("Остановка сервера");
                        this.textBox3.AppendText("Клиент отключен.\n");
                    }
                    else
                    {
                        await AMessage("Stoping server");
                        this.textBox3.AppendText("Client disconnected.\n");
                    }
                }

            }
            else
            {
                if (Client.Connected)
                {
                    Client.Close();
                    ClientConnected = false;
                    Client = null;
                    if (Languege)
                    {
                        await AMessage("Остоновка подключения");
                        this.textBox3.AppendText("Клиент отключен.\n");
                    }
                    else
                    {
                        await AMessage("Stopping connection");
                        this.textBox3.AppendText("Client disconnected.\n");
                    }
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void backgroundwork1(object sender, EventArgs e)
        {
            while (Client.Connected)
            {
                try
                {
                    KeysToSent = STR.ReadLine();
                    this.textBox3.AppendText(KeysToSent);
                    KeysToSent = "";
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message.ToString());
                }
            }

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (Languege != true)
            {
                Languege = true; // False: Eng True: Ru
                Share.Text = "Вкладка «Поделиться»";
                Connect.Text = "Вкладка «Подключится»";
                if (Server)
                {
                    label2.Text = "Поделиться";
                    button1.Text = "Поделиться";
                }
                else
                {
                    label2.Text = "Подключится";
                    button1.Text = "Подключится";
                }
                Host.Text = "Хост";
                label1.Text = "Порт";
                button2.Text = "Остановится";
                label3.Text = "Логи";
            }
            }

        private void button4_Click(object sender, EventArgs e)
        {
            if (Languege != false)
            {
                Languege = false; // False: Eng True: Ru
                Share.Text = "Share tab»";
                Connect.Text = "Connect tab";
                if (Server == true)
                {
                    label2.Text = "Sharing";
                    button1.Text = "Share";
                }
                else
                {
                    label2.Text = "Connecting";
                    button1.Text = "Connect";
                }
                Host.Text = "Host";
                label1.Text = "Port";
                button2.Text = "Stop";
                label3.Text = "Output";
            }
        }
    }
}
