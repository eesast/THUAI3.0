using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace WindowsFormsApp4
{
    public partial class Form2 : Form
    {

        public string token = " ";
        public Form2()
        {
            InitializeComponent();
        }

        private void createPort_TextChanged(object sender, EventArgs e)
        {

        }

        private void Cancel2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void create_Click(object sender, EventArgs e)
        {

            
            if (!IsNumberic(createPort.Text))
            {

                Console.WriteLine(token);
                MessageBox.Show("请输入正确的端口号!");
            }
            else
            {
                string data = "{\"contestId\": 2,\"teams\": [],\"ip\": \"127.0.0.1\",\"port\": " + createPort.Text + "}";
                CreatRoom("https://api.eesast.com/v1/rooms", data);
            }
        }
        public bool CreatRoom(string url, string data)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            //字符串转换为字节码
            byte[] bs = Encoding.UTF8.GetBytes(data);
            //参数类型，这里是json类型
            httpWebRequest.ContentType = "application/json";
            //参数数据长度
            //httpWebRequest.ContentLength = bs.Length;
            //设置请求类型
            httpWebRequest.Method = "POST";
            //设置超时时间
            httpWebRequest.Timeout = 20000;
            //将参数写入请求地址中
            httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);

            httpWebRequest.Accept = "*/*";
            
            string token2 = "Bearer " + token;
            httpWebRequest.Headers.Add("Authorization", token2);
            //httpWebRequest.Headers["Authorization"] = "Bearer " + token;
            //发送请求
            try
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                System.IO.Stream getStream = httpWebResponse.GetResponseStream();
                System.IO.StreamReader streamreader = new System.IO.StreamReader(getStream);
                String result = streamreader.ReadToEnd();
                //这后面的代码是登陆成功后执行的
                MessageBox.Show("创建成功");
                this.Close();
                Console.WriteLine(result);
                httpWebRequest.Abort();
                return true;
            }
            catch (WebException ex)
            {
                //这后面的代码是登录失败后执行的
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Unauthorized)
                {
                    MessageBox.Show("token错误，请重新登录");
                }
                else
                {
                    MessageBox.Show("服务器错误，可能是端口已被占用");
                }
                
                //HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
                //System.IO.Stream getStream = httpWebResponse.GetResponseStream();
                //System.IO.StreamReader streamreader = new System.IO.StreamReader(getStream);
                //String result = streamreader.ReadToEnd();
                //Console.WriteLine(result);
                httpWebRequest.Abort();
                return false;
            }
        }
        public static bool IsNumberic(string str)
        {
            int vsNum;
            bool isNum;
            isNum = Int32.TryParse(str, out vsNum);
            return isNum;
        }
    }
}
