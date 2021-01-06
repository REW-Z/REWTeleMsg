using System;
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

namespace SocketServer
{
    public class SocketConnection
    {
        public Form1 currentForm;

        public Socket currentSocket;

        public byte[] data = new byte[1024 * 1024];

        public bool Connected { get { return currentSocket.Connected; } }

        public SocketConnection(Socket socket, Form1 form)
        {
            currentSocket = socket;
            currentForm = form;

            Thread newThread = new Thread(ReceiveMessage);
            newThread.IsBackground = true;
            newThread.Start();
        }

        /// <summary>
        /// 持续接收数据
        /// </summary>
        public void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    if (currentSocket.Poll(10, SelectMode.SelectRead))
                    {
                        MessageBox.Show("CloseSocket! \n Because: Poll_SelectRead==false!");
                        currentSocket.Close();
                        break;
                    }
                    int length = 0;
                    length = currentSocket.Receive(data);
                    if (length == 0)
                    {
                        MessageBox.Show("CloseSocket! \n Because: length== 0");
                        currentSocket.Close();
                        break;
                    }

                    string strSenderInfo = currentSocket.RemoteEndPoint.ToString() + "\t" + DateTime.Now.ToString() + "\n";
                    switch (data[0])
                    {
                        case 0://接收字符串：
                            string receiveStr = Encoding.UTF8.GetString(data, 1, length - 1);
                            currentForm.GetSetTextBox(strSenderInfo + receiveStr + "\n");
                            currentForm.Boardcast(strSenderInfo + receiveStr + "\n");
                            break;
                        case 1://接收文件：
                            int index = data.ToList().IndexOf((byte)'\0');
                            byte[] fileNameData = data.Skip(1).Take(index - 1).ToArray();
                            byte[] fileData = data.Skip(index + 1).Take(length - index - 1).ToArray();
                            string fileName = System.IO.Path.GetFileName(Encoding.UTF8.GetString(fileNameData));

                            currentForm.SaveFile(fileData, fileName);

                            currentForm.GetSetTextBox(strSenderInfo + "服务器接收文件" + fileName + "\n");
                            currentForm.Boardcast(strSenderInfo + "服务器接收文件" + fileName + "\n");
                            currentForm.Log("接收文件。。");
                            break;
                        default:
                            break;
                    }
                        
                    
                }
                catch(Exception ex)
                {
                    MessageBox.Show("从客户端" + currentSocket.RemoteEndPoint.ToString() + "接收失败。。错误原因：" + ex.ToString());
                    break;
                }



            }

        }

        /// <summary>
        /// 向客户端发送消息
        /// </summary>
        /// <param name="msg"></param>
        public void SendMessage(string msg)
        {
            currentSocket.Send(Encoding.UTF8.GetBytes(msg));
        }
    }
}
