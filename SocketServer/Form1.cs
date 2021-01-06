using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketServer
{
    public partial class Form1 : Form
    {
        public List<SocketConnection> conns = new List<SocketConnection>();

        public string GetSetTextBox(string str)
        {
            richTextBoxMain.Text += (str + "\n");
            return richTextBoxMain.Text;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Thread newThread = new Thread(Welcoming);
            newThread.IsBackground = true;
            newThread.Start();
        }

        /// <summary>
        /// welcoming socket
        /// </summary>
        public void Welcoming()
        {
            //获取本机IP地址
            IPAddress localIPAddress = null;
            var addressList =Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            Log("主机名: " + Dns.GetHostName());
            foreach(var address in addressList)
            {
                if(address.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (Regex.IsMatch(address.ToString(), "192.168."))
                    {
                        localIPAddress = address;
                        Log("IP地址：" + address.ToString());
                    }
                }
            }
            //创建welcomingSocket并持续监听
            Socket welcomingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(localIPAddress, 2333);

            welcomingSocket.Bind(endPoint);
            welcomingSocket.Listen(100);

            while (true)
            {
                Log("尝试Accept..");
                Socket newSocket = welcomingSocket.Accept();
                Log("完成Accept！");

                SocketConnection conn = new SocketConnection(newSocket, this);
                conns.Add(conn);
            }
        }

        /// <summary>
        /// 广播
        /// </summary>
        /// <param name="str"></param>
        public void Boardcast(string str)
        {
            List<SocketConnection> tempList = new List<SocketConnection>();
            foreach (var conn in conns)
            {
                if (conn.Connected)
                {
                    conn.SendMessage(str);
                }
                else
                {
                    tempList.Add(conn);
                }
            }

            foreach (var item in tempList)
            {
                conns.Remove(item);
            }

        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fileName"></param>
        public void SaveFile(byte[] data, string fileName)
        {
            string fileFullName = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + fileName;
            System.IO.File.WriteAllBytes(fileFullName, data);
        }

        /// <summary>
        /// 日志
        /// </summary>
        /// <param name="str"></param>
        public void Log(string str)
        {
            richTextBoxStatus.Text += (str + "\n");
        }
    }
}
