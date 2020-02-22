using System;
using System.Net;
using System.IO;
using System.Text;

namespace Login
{
    public class Program
    {
        
        bool HttpPost(string url, string data)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            //字符串转换为字节码
            byte[] bs = Encoding.UTF8.GetBytes(data);
            //参数类型，这里是json类型
            httpWebRequest.ContentType = "application/json";
            //参数数据长度
            httpWebRequest.ContentLength = bs.Length;
            //设置请求类型
            httpWebRequest.Method = "POST";
            //设置超时时间
            httpWebRequest.Timeout = 20000;
            //将参数写入请求地址中
            httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);
            //发送请求
            try
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                //这后面的代码是登陆成功后执行的
                Console.WriteLine("登陆成功");
                httpWebRequest.Abort();
                return true;
            }
            catch (WebException ex)
            {
                //这后面的代码是登录失败后执行的
                if(((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine("用户名或密码错误");
                }
                else
                {
                    Console.WriteLine("服务器错误");
                }
                httpWebRequest.Abort();
                return false;

            }
        }


        static void Main(string[] args)
        {
            Program example1 = new Program();
            string data = "{\"username\": \"username\",\"password\": \"password\"}";
            //HttpPost本身也会返回布尔值以供判断成功与否
            if(example1.HttpPost("https://api.eesast.com/v1/users/login", data))
            {
                
            }
        }
    }
}
