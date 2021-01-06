using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketClient
{
    public partial class Form1 : Form
    {
        public Socket clientSocket;

        private bool isConnected = false;

        public IPEndPoint endPoint;

        public byte[] data = new byte[1024 * 1024];

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            Connect(textBoxIp.Text);
        }
        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (!isConnected) return;

            byte[] dataToSend = (new byte[1] { 0 }).Concat(Encoding.UTF8.GetBytes(textBox1.Text)).ToArray();
            clientSocket.Send(dataToSend);
        }
        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendFile_Click(object sender, EventArgs e)
        {
            if (!isConnected) return;

            openFileDialog1.ShowDialog();
            string fileName = openFileDialog1.FileName;
            byte[] info = Encoding.UTF8.GetBytes(fileName);
            byte[] preBuffer = (new byte[1] { 1 }).Concat(info).Append((byte)'\0').ToArray();
            clientSocket.SendFile(fileName, 
                preBuffer, 
                null, 
                TransmitFileOptions.UseDefaultWorkerThread
                );
            MessageBox.Show("文件发送完成!");
        }

        /// <summary>
        /// 关闭Close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClose_Click(object sender, EventArgs e)
        {
            clientSocket.Close();
            isConnected = false;
        }
        /// <summary>
        /// 关闭Shutdown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonShutdown_Click(object sender, EventArgs e)
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            isConnected = false;
        }

        /// <summary>
        /// 连接远程主机
        /// </summary>
        private void Connect(string ipStr)
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                endPoint = new IPEndPoint(IPAddress.Parse(ipStr), 2333);
                clientSocket.Connect(endPoint);

                isConnected = true;

                Thread newThread = new Thread(ReceiveMessage);
                newThread.IsBackground = true;
                newThread.Start();
            }
            catch
            {
                MessageBox.Show("连接失败！");
            }
           
        }

        /// <summary>
        /// 循环接收信息
        /// </summary>
        private void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    if (!clientSocket.Connected)
                    {
                        MessageBox.Show("客户端接受失败,因为未连接。");
                        break;
                    }
                    int length = clientSocket.Receive(data);
                    string receiveStr = Encoding.UTF8.GetString(data, 0, length);
                    richTextBoxMain.Text += (receiveStr + "\n");
                }
                catch(Exception ex)
                {
                    MessageBox.Show("客户端接受失败。错误原因：" + ex.ToString());
                    break;
                }
                
            }
        }

        
    }
}
