using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;








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

        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;
        const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const uint MOUSEEVENTF_RIGHTUP = 0x0010;



        void PressKey(int key)
        {
            switch (key)
            {
                case 1:
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
                    break;

                case 2:
                    mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, UIntPtr.Zero);
                    break;

                case 4:
                    mouse_event(0x0080, 0, 0, 0x0001, UIntPtr.Zero);
                    break;

                default:
                    keybd_event((byte)key, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                    break;
            }
        }

        void ReleaseKey(int key)
        {
            switch (key)
            {
                case 1:
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
                    break;

                case 2:
                    mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, UIntPtr.Zero);
                    break;

                case 4:
                    mouse_event(0x0100, 0, 0, 0x0001, UIntPtr.Zero);
                    break;

                default:
                    keybd_event((byte)key, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                    break;
            }
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
        volatile bool IsRunning = false;

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



        private void Form1_Load(object sender, EventArgs e)
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


        private void button1_Click(object sender, EventArgs e)
        {
            if (Languege)
            {
                button1.Text = "Подключиться";
                label2.Text = "Подключиться";
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



        private void button3_Click(object sender, EventArgs e)
        {
            ChosenIP = this.textBox2.Text;
            if (Languege)
            {
                button1.Text = "Поделиться";
                label2.Text = "Поделиться";
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
                IsRunning = true;
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
                IsRunning = true;
                if (Languege)
                    textBox3.AppendText("Подключено!\n");
                else
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
            while (IsRunning && ClientConnected)
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
            IsRunning = false;
            ClientConnected = false;

            try
            {
                STR?.Close();
                STW?.Close();

                Client?.Close();
                listener?.Stop();

                Client = null;
                listener = null;
            }
            catch { }

            LastKeys.Clear();

            if (Languege)
            {
                await AMessage("Остановлено");
                textBox3.AppendText("Соединение закрыто\n");
            }
            else
            {
                await AMessage("Stopped");
                textBox3.AppendText("Connection closed\n");
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
                this.checkBox1.Text = "Синхронизация\n     мыши";
                this.checkBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                this.Share.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                Languege = true; // False: Eng True: Ru
                Share.Text = "Вкладка «Поделиться»";
                Connect.Text = "Вкладка «Подключиться»";
                this.Connect.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                if (Server)
                {
                    label2.Text = "Ожидание подключения";
                    button1.Text = "Поделиться";
                }
                else
                {
                    label2.Text = "Подключение";
                    button1.Text = "Подключиться";
                }
                Host.Text = "Хост";
                label1.Text = "Порт";
                button5.Text = "Очистить логи";
                button2.Text = "Остановка";
                label3.Text = "Логи";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (Languege != false)
            {
                this.checkBox1.Text = "Sync Mouse";
                this.Share.Font = new System.Drawing.Font("Verdana", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                Languege = false; // False: Eng True: Ru
                this.checkBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                Share.Text = "Share tab";
                Connect.Text = "Connect tab";
                this.Connect.Font = new System.Drawing.Font("Verdana", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
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
            while (IsRunning)
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
                                    this.textBox3.AppendText(KeyNameFromVK(key) + " DOWN ");
                                }
                                else
                                    this.textBox3.AppendText(KeyNameFromVK(key) + " НАЖАТИЕ ");


                                PressKey(key);



                            }
                            else if (keyStr.EndsWith("U"))
                            {
                                await Task.Delay(10);
                                int key = int.Parse(keyStr.TrimEnd('U'));
                                if (Languege != true)
                                {

                                    this.textBox3.AppendText(KeyNameFromVK(key) + " UP ");
                                }
                                else
                                    this.textBox3.AppendText(KeyNameFromVK(key) + " ОТПУСКАНИЕ ");

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
                if (Languege != true)
                    textBox3.AppendText("Cannot change Mouse Sync during connection!\n");
                else
                    textBox3.AppendText("Нельзя изменить синхронизацию мыши во время соединения!\n");
                checkBox1.Checked = MouseSync;
                return;
            }
            MouseSync = checkBox1.Checked;

            if (MouseSync)
            {



                if (Languege != true)
                    textBox3.AppendText("Mouse sync enabled.\n");
                else
                    textBox3.AppendText("Синхронизация мыши включена.\n");
            }
            else
            {
                if (Languege != true)
                    textBox3.AppendText("Mouse sync disabled.\n");
                else
                    textBox3.AppendText("Синхронизация мыши выключена.\n");
            }
        }
    }
}
