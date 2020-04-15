using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;
using Newtonsoft.Json;
using System.Threading;

namespace Downloader
{
    public partial class Form1 : Form
    {
        static List<string> newFileName = new List<string>();
        static List<string> updateFileName = new List<string>();
        static List<string> SelectedNewFile = new List<string>();
        static List<string> SelectedUpdateFile = new List<string>();
        static CancellationTokenSource tokenSource = new CancellationTokenSource();
        static CancellationToken token = tokenSource.Token;
        static bool Checked = false;

        public class Data
        {
            static string Path = "./data.dat";
            public static string FilePath = "";
            public static string tencentID = "";
            public static string tencentKey = "";

            public Data(string Path)
            {
                using (StreamReader r = new StreamReader(Path))
                {
                    Data.Path = Path;
                    Data.tencentID = r.ReadLine();
                    Data.tencentKey = r.ReadLine();
                    Data.FilePath = r.ReadLine();
                }
            }
            public static void ChangeLine(int lineNum, string newLine)
            {
                List<string> lines = new List<string>();
                using (StreamReader r = new StreamReader(Path))
                {
                    string t = r.ReadToEnd();
                    lines = t.Split('\n').ToList();
                }
                if (lineNum > lines.Count - 1)
                    lines.Add(newLine);
                else lines[lineNum] = newLine;
                using (StreamWriter w = new StreamWriter(Path))
                    foreach (string line in lines)
                        w.WriteLine(line.Trim('\r').Trim('\n'));
            }
        }
        public Form1()
        {
            InitializeComponent();
            Data Filedata = new Data("./data.dat");
            this.textBox1.Text = Data.FilePath;
            //this.skinEngine1 = new Sunisoft.IrisSkin.SkinEngine(((System.ComponentModel.Component)(this)));
            //this.skinEngine1.SkinFile = Application.StartupPath + "//Eighteen.ssk";
            Control.CheckForIllegalCrossThreadCalls = false;

        }
        private void txt_GotFocus(object sender, EventArgs e)
        {
            var source = new AutoCompleteStringCollection();
            if (File.Exists("./data.dat"))
            {
                string nextline;
                StreamReader r = new StreamReader("./data.dat");
                while ((nextline = r.ReadLine()) != null)
                    source.Add(nextline);
                r.Close();
            }
        }
        private void btnDown_Click(object sender, EventArgs e)
        {

        }
        class Tencent_cos_download
        {
            public void download(string download_dir, string key = "md5list.json")
            {
                //初始化 CosXmlConfig 
                string appid = "1255334966";//设置腾讯云账户的账户标识 APPID
                string region = "ap-beijing"; //设置一个默认的存储桶地域
                CosXmlConfig config = new CosXmlConfig.Builder()
                  .IsHttps(true)  //设置默认 HTTPS 请求
                  .SetAppid(appid)  //设置腾讯云账户的账户标识 APPID
                  .SetRegion(region)  //设置一个默认的存储桶地域
                  .SetDebugLog(true)  //显示日志
                  .Build();  //创建 CosXmlConfig 对象


                //方式1， 永久密钥
                string secretId = Data.tencentID; //"云 API 密钥 SecretId";
                string secretKey = Data.tencentKey; //"云 API 密钥 SecretKey";
                long durationSecond = 1000;  //每次请求签名有效时长，单位为秒
                QCloudCredentialProvider cosCredentialProvider = new DefaultQCloudCredentialProvider(secretId, secretKey, durationSecond);

                //初始化 CosXmlServer
                CosXmlServer cosXml = new CosXmlServer(config, cosCredentialProvider);
                try
                {
                    string bucket = "thuai-1255334966"; //存储桶，格式：BucketName-APPID
                    string localDir = System.IO.Path.GetDirectoryName(download_dir);//本地文件夹                               <--------------
                    string localFileName = System.IO.Path.GetFileName(download_dir); //指定本地保存的文件名      <--------------
                    GetObjectRequest request = new GetObjectRequest(bucket, key, localDir, localFileName);
                    //设置签名有效时长
                    request.SetSign(DateTimeOffset.UtcNow.ToUnixTimeSeconds(), 1000);
                    //设置进度回调
                    Dictionary<string, string> test = request.GetRequestHeaders();
                    request.SetCosProgressCallback(delegate (long completed, long total)
                    {
                        //Console.WriteLine(String.Format("progress = {0:##.##}%", completed * 100.0 / total));
                    });
                    //执行请求
                    GetObjectResult result = cosXml.GetObject(request);
                    //请求成功
                    Console.WriteLine(result.GetResultInfo());
                }
                catch (COSXML.CosException.CosClientException clientEx)
                {
                    //请求失败
                    Console.WriteLine("CosClientException: " + clientEx);
                }
                catch (COSXML.CosException.CosServerException serverEx)
                {
                    //请求失败
                    Console.WriteLine("CosServerException: " + serverEx.GetInfo());
                }
            }
        }

        public static string GetFileMd5Hash(string strFileFullPath)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.


            System.IO.FileStream fst = null;
            try
            {
                fst = new System.IO.FileStream(strFileFullPath, System.IO.FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5.ComputeHash(fst);

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                fst.Close();
                // Return the hexadecimal string.
                return sBuilder.ToString().ToLower();
            }
            catch (System.Exception)
            {
                if (fst != null)
                    fst.Close();
                return "";
            }
            finally
            {
            }
        }

        private async void btn_Click(object sender, EventArgs e)
        {
            if (!Checked)
                await Task.Run(() => { this.button3_Click(sender, e); });
            else
            {
                Form2 f2 = new Form2(newFileName, updateFileName);
                f2.StartPosition = FormStartPosition.CenterParent;
                f2.ShowDialog();
                if (f2.DialogResult == DialogResult.Yes)
                {
                    List<string>[] FileNames = (List<string>[])f2.Tag;
                    SelectedNewFile = FileNames[0];
                    SelectedUpdateFile = FileNames[1];
                    Tencent_cos_download Downloader = new Tencent_cos_download();

                    bool cancelled = false;
                    int newFile = 0, updateFile = 0;
                    int totalnew = SelectedNewFile.Count, totalupdate = SelectedUpdateFile.Count;
                    MessageBoxButtons mes = MessageBoxButtons.OKCancel;
                    if (newFileName.Count > 0 || updateFileName.Count > 0)
                    {
                        DialogResult dialogResult = MessageBox.Show("有新的更新可用，是否下载?", "提示", mes);
                        if (dialogResult != DialogResult.OK) return;
                        try
                        {
                            await Task.Run(async () =>
                                {
                                    foreach (string Filename in SelectedNewFile)
                                    {
                                        if (token.IsCancellationRequested)
                                        { cancelled = true; tokenSource = new CancellationTokenSource(); token = tokenSource.Token; return; }
                                        this.textBox2.AppendText(newFile + 1 + "/" + totalnew + ": 开始下载" + Filename + Environment.NewLine);
                                        await Task.Run(() => { Downloader.download(System.IO.Path.Combine(this.textBox1.Text, Filename), Filename); });
                                        this.textBox2.AppendText(Filename + "下载完毕!" + Environment.NewLine); newFile++;
                                    }
                                    foreach (string Filename in SelectedUpdateFile)
                                    {
                                        if (token.IsCancellationRequested)
                                        { cancelled = true; tokenSource = new CancellationTokenSource(); token = tokenSource.Token; return; }
                                        this.textBox2.AppendText(updateFile + 1 + totalupdate + ": 开始更新" + Filename + Environment.NewLine);
                                        await Task.Run(() => { Downloader.download(System.IO.Path.Combine(this.textBox1.Text, Filename), Filename); });
                                        this.textBox2.AppendText(Filename + "更新完毕!" + Environment.NewLine); updateFile++;
                                    }
                                }, token);
                        }
                        catch (System.Exception)
                        {
                            throw;
                        }
                        if (!cancelled)
                            this.textBox2.AppendText($"下载成功！共下载{newFile}个新文件，更新{updateFile}个文件" + Environment.NewLine);
                        else
                            this.textBox2.AppendText($"下载取消，共下载{newFile}个新文件，更新{updateFile}个文件" + Environment.NewLine);
                        cancelled = false;
                    }
                    else this.textBox2.AppendText("当前平台已是最新版本！" + Environment.NewLine);
                }
            }
        }


        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.ShowDialog();
            this.textBox1.Text = folder.SelectedPath;
            Data.ChangeLine(2, this.textBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tokenSource.Cancel();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_LostFocus(object sender, EventArgs e)
        {
            Data.ChangeLine(2, this.textBox1.Text);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private async void button3_Click(object sender, EventArgs e)
        {
            string json, MD5;
            int newFile = 0, updateFile = 0;
            Tencent_cos_download Downloader = new Tencent_cos_download();
            await Task.Run(() => { Downloader.download(System.IO.Path.Combine(this.textBox1.Text, "md5list.json")); });
            using (StreamReader r = new StreamReader(System.IO.Path.Combine(this.textBox1.Text, "md5list.json")))
            {
                json = r.ReadToEnd();
            }
            json = json.Replace("\r", string.Empty).Replace("\n", string.Empty);
            Dictionary<string, string> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            foreach (KeyValuePair<string, string> pair in jsonDict)
            {
                MD5 = GetFileMd5Hash(System.IO.Path.Combine(this.textBox1.Text, pair.Key));
                if (MD5.Length == 0 && !newFileName.Exists(t => t == pair.Key))
                {
                    newFile++;
                    newFileName.Add(pair.Key);
                }
                else if (MD5 != pair.Value && !newFileName.Exists(t => t == pair.Key))
                {
                    updateFile++;
                    updateFileName.Add(pair.Key);
                }
            }
            if (newFile + updateFile == 0)
            {
                this.textBox2.AppendText("当前平台已是最新版本！" + Environment.NewLine);
                newFileName.Clear();updateFileName.Clear();
            }
            else
            {
                this.textBox2.AppendText($"发现{newFile}个新文件：" + Environment.NewLine);
                foreach (string filename in newFileName)
                    this.textBox2.AppendText(filename + Environment.NewLine);
                this.textBox2.AppendText(Environment.NewLine + $"发现{updateFile}个文件更新：" + Environment.NewLine);
                foreach (string filename in updateFileName)
                    this.textBox2.AppendText(filename + Environment.NewLine);
                this.textBox2.AppendText(Environment.NewLine + "请点击下载按钮选择文件" + Environment.NewLine);
            }
            Checked = true;
        }
    }
}
