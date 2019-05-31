using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //当点击开始监听的时候 在服务器端创建一个负责监听IP地址和端口号的Socket
                Socket socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //获取ip地址
                IPAddress ip = IPAddress.Any; //IPAddress.Parse(this.textBox1.Text.Trim());
                                              //创建端口号
                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(this.textBox2.Text.Trim()));
                //绑定IP地址和端口号
                socketWatch.Bind(point);
                ShowBox("监听成功" + " \r \n");
                //开始监听:设置最大可以同时连接多少个请求
                socketWatch.Listen(10);
                Thread th = new Thread(Listen);
                th.IsBackground = true;
                th.Start(socketWatch);
            }
            catch 
            {
                
            }
            

        }
        Socket socketSend;
        Dictionary<string, Socket> diclist = new Dictionary<string, Socket>();
        /// <summary>
        /// 等待客户端的连接，并且创建与之通信用的Socket
        /// </summary>
        /// <param name="obj"></param>
        private void Listen(object obj)
        {
            Socket socketWatch = obj as Socket;
            while (true)
            {
                try
                {
                    //等待客户端的连接，并且创建一个用于通信的Socket
                    socketSend = socketWatch.Accept();
                    //将客户端socket存到集合中
                    diclist.Add(socketSend.RemoteEndPoint.ToString(), socketSend);
                    //将客户端ip 存到下拉列表中
                    comboBox1.Items.Add(socketSend.RemoteEndPoint.ToString());
                    ShowBox(socketSend.RemoteEndPoint.ToString() + "：连接成功");
                    //
                    Thread th = new Thread(Receive);
                    th.IsBackground = true;
                    th.Start(socketSend);
                }
                catch 
                {
                    
                }
            }
        }
        /// <summary>
        /// 服务器端不停接受客户端发来的消息
        /// </summary>
        /// <param name="o"></param>
        void Receive(object o) {
            Socket socketSend = o as Socket;
            while (true) {
                try
                {
                    //客户端连接成功后，服务器端该接受客户端发来的额消息
                    byte[] buffer = new byte[1024 * 1024 * 2];
                    //时间接收到的有效字节数
                    int r = socketSend.Receive(buffer);
                    if (r == 0)
                    {
                        break;
                    }
                    string str = Encoding.UTF8.GetString(buffer, 0, r);

                    ShowBox(socketSend.RemoteEndPoint + "：" + str);
                }
                catch 
                {
                    
                }
                
            }
           
        }
        void ShowBox(string str) {
            this.textBox3.AppendText(str+"\r\n");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        /// <summary>
        /// 服务器给客户端发送消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            string str = textBox4.Text.Trim();
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(str);
            //发送 文件、文本、震动等
            List<byte> list = new List<byte>();
            list.Add(0);
            list.AddRange(buffer);
            //将集合转成数组
            byte[] newBuffer = list.ToArray();
            //socketSend.Send(buffer);
            string socketid = comboBox1.SelectedItem.ToString();
            diclist[socketid].Send(newBuffer);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = @"C:\Users\ZP\Desktop";
            ofd.Title = "请选择要发送的文件";
            ofd.Filter = "所有文件|*.*";
            ofd.ShowDialog();

            textBox5.Text = ofd.FileName;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //获得要发送文件的路径
            string path = textBox5.Text;
            using (FileStream fsRead = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[1024 * 1024 * 10];
                int r = fsRead.Read(buffer, 0, buffer.Length);
                List<byte> list = new List<byte>();
                list.Add(1);//表示发送文件
                list.AddRange(buffer);
                byte[] newBuffer = list.ToArray();
                diclist[comboBox1.SelectedItem.ToString()].Send(newBuffer, 0,r+1,SocketFlags.None);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            byte[] buffer = new byte[1];
            buffer[0] = 2;
            diclist[comboBox1.SelectedItem.ToString()].Send(buffer);
        }
    }
}
