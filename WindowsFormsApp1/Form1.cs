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








namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        const uint KEYEVENTF_KEYDOWN = 0x0000;
        const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        const uint MOUSEEVENTF_MOVE = 0x0001;



        void PressKey(int key)
        {
            keybd_event((byte)key, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        }

        void ReleaseKey(int key)
        {
            keybd_event((byte)key, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        bool Server = true;
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
        bool Languege = false;
        bool MouseSync = true;


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
            this.checkBox1.Hide();
            this.checkBox1.Enabled = false;
            this.checkBox1.Visible = false;
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
            this.checkBox1.Show();
            this.checkBox1.Enabled = true;
            this.checkBox1.Visible = true;
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
                textBox3.AppendText("Connected!\n");

            }
            catch (Exception ex)
            {
                await AMessage(ex.Message);
            }
        }


        List<int> LastKeys = new List<int>();

        private async Task UpdateKey()
        {
            Point lastMousePos = Cursor.Position;
            while (ClientConnected)
            {

                    Point currentPos = Cursor.Position;

                    int dx = currentPos.X - lastMousePos.X;
                    int dy = currentPos.Y - lastMousePos.Y;

                    if (dx != 0 || dy != 0)
                    {
                        STW.WriteLine($"M:{dx},{dy}");
                    }

                    lastMousePos = currentPos;

                var currentKeys = new List<int>();
                for (int i = 0; i < 256; i++)
                {
                    if ((GetAsyncKeyState(i) & 0x8000) != 0)
                        currentKeys.Add(i);
                }

                var pressedKeys = currentKeys.Except(LastKeys).ToList();
                var releasedKeys = LastKeys.Except(currentKeys).ToList();

                if (pressedKeys.Count > 0 || releasedKeys.Count > 0)
                {
                    string msg = string.Join("+", pressedKeys.Select(k => k + "D")) + ";" +
                                 string.Join("+", releasedKeys.Select(k => k + "U"));
                    STW.WriteLine(msg);
                }

                LastKeys = currentKeys;
                await Task.Delay(5);
            }
        }




        private async void ConnectOrShare(object sender, EventArgs e)
        {
            if (Server)
            {

                await StartServerAsync();
                await backgroundwork1();
            }
            else
            {

                await StartClientAsync();
                await UpdateKey();
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
                button5.Text = "Очистить логи";
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
                button5.Text = "Clear Output";
                label1.Text = "Port";
                button2.Text = "Stop";
                label3.Text = "Output";
            }
        }


        string KeyNameFromVK(int vk)
        {
            try
            {
                return ((Keys)vk).ToString();
            }
            catch
            {
                return vk.ToString();
            }
        }




        private async Task backgroundwork1()
        {
            while (Client.Connected)
            {
                try
                {
                    KeysToSent = await STR.ReadLineAsync();
                    if (MouseSync)  
                    {
                        if (KeysToSent.StartsWith("M:"))
                        {
                            var data = KeysToSent.Substring(2).Split(',');
                            int dx = int.Parse(data[0]);
                            int dy = int.Parse(data[1]);

                            if (Languege)
                                textBox3.AppendText($"Мышь: {dx},{dy}\n");
                            else
                                textBox3.AppendText($"Mouse: {dx},{dy}\n");

                            mouse_event(MOUSEEVENTF_MOVE, dx, dy, 0, UIntPtr.Zero);
                            continue;
                        }
                    }  

                    string[] parts = KeysToSent.Split(';');
                    foreach (string part in parts)
                    {
                        if (part.Length == 0) continue;
                        string[] keys = part.Split('+');
                        foreach (var keyStr in keys)
                        {
                            if (keyStr.EndsWith("D"))
                            {
                                int key = int.Parse(keyStr.TrimEnd('D'));
                                if (Languege != true)
                                {
                                    this.textBox3.AppendText(KeyNameFromVK(key) + " START ");
                                }
                                else
                                    this.textBox3.AppendText(KeyNameFromVK(key) + " НАЧАЛО ");


                                PressKey(key);



                            }
                            else if (keyStr.EndsWith("U"))
                            {
                                await Task.Delay(10);
                                int key = int.Parse(keyStr.TrimEnd('U'));
                                if (Languege != true)
                                {
                                    this.textBox3.AppendText(KeyNameFromVK(key) + " END ");
                                }
                                else
                                    this.textBox3.AppendText(KeyNameFromVK(key) + " КОНЕЦ ");

                                ReleaseKey(key);

                            }
                        }
                    }
                    KeysToSent = "";

                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message.ToString());
                }
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox3.Text = string.Empty;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (Client != null && Client.Connected)
            {
                textBox3.AppendText("Cannot change Mouse Sync during connection!\n");
                checkBox1.Checked = MouseSync;
                return;
            }
            MouseSync = checkBox1.Checked;

            if (MouseSync)
            {
                textBox3.AppendText("Mouse sync enabled.\n");
            }
            else
            {
                textBox3.AppendText("Mouse sync disabled.\n");
            }
        }
    }
}
