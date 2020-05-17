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
            static string Path;
            public static string FilePath = "";

            public Data(string Path)
            {
                Data.Path = Path + "/THUAI3.0.dat";
                if (File.Exists(Data.Path))
                    using (StreamReader r = new StreamReader(Data.Path))
                        Data.FilePath = r.ReadLine();
                else Data.FilePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            public static void ChangeData(string newLine)
            {
                using (StreamWriter w = new StreamWriter(Data.Path))
                    w.WriteLine(newLine.Trim('\r').Trim('\n'));
            }
        }
        public Form1()
        {
            InitializeComponent();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Data Filedata = new Data(path);
            this.textBox1.Text = Data.FilePath;
            //this.skinEngine1 = new Sunisoft.IrisSkin.SkinEngine(((System.ComponentModel.Component)(this)));
            //this.skinEngine1.SkinFile = Application.StartupPath + "//Eighteen.ssk";
            Control.CheckForIllegalCrossThreadCalls = false;

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
                string secretId = Properties.Resources.TencentID; //"云 API 密钥 SecretId";
                string secretKey = Properties.Resources.TencentKey; //"云 API 密钥 SecretKey";
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
                    //request.SetSign(DateTimeOffset.UtcNow.ToString(), 1000);
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
                    throw clientEx;
                }
                catch (COSXML.CosException.CosServerException serverEx)
                {
                    throw serverEx;
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
            finally { }
        }

        private async void btn_Click(object sender, EventArgs e)
        {
            if (!Checked)
                await Task.Run(() => { this.button3_Click(sender, e); });
            else
            {
                文件列表 f2 = new 文件列表(newFileName, updateFileName);
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
                    if (SelectedNewFile.Count > 0 || SelectedUpdateFile.Count > 0)
                    {
                        DialogResult dialogResult = MessageBox.Show($"共有{SelectedNewFile.Count}个新文件，{SelectedUpdateFile.Count}个更新", "提示", mes);
                        if (dialogResult != DialogResult.OK)
                        {
                            SelectedNewFile.Clear();
                            SelectedUpdateFile.Clear();
                            return;
                        }
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
                                        this.textBox2.AppendText(updateFile + 1 + "/" + totalupdate + ": 开始更新" + Filename + Environment.NewLine);
                                        File.Delete(System.IO.Path.Combine(this.textBox1.Text, Filename));
                                        await Task.Run(() => { Downloader.download(System.IO.Path.Combine(this.textBox1.Text, Filename), Filename); });
                                        this.textBox2.AppendText(Filename + "更新完毕!" + Environment.NewLine); updateFile++;
                                    }
                                }, token);
                        }
                        catch (COSXML.CosException.CosClientException clientEx)
                        {
                            //请求失败
                            this.textBox2.AppendText("CosClientException: " + clientEx.ToString() + Environment.NewLine);
                            return;
                        }
                        catch (COSXML.CosException.CosServerException serverEx)
                        {
                            //请求失败
                            this.textBox2.AppendText("CosClientException: " + serverEx.ToString() + Environment.NewLine);
                            return;
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
                    SelectedNewFile.Clear(); SelectedUpdateFile.Clear();
                    newFileName.Clear(); updateFileName.Clear();
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
            if (folder.SelectedPath != "")
            {
                this.textBox1.Text = folder.SelectedPath;
                Data.ChangeData(this.textBox1.Text);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tokenSource.Cancel();
            SelectedNewFile = new List<string>();
            SelectedUpdateFile = new List<string>();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_LostFocus(object sender, EventArgs e)
        {
            Data.ChangeData(this.textBox1.Text);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private async void button3_Click(object sender, EventArgs e)
        {
            string json, MD5;
            int newFile = 0, updateFile = 0;
            newFileName.Clear();
            updateFileName.Clear();
            Tencent_cos_download Downloader = new Tencent_cos_download();
            try
            {
                if (File.Exists(System.IO.Path.Combine(this.textBox1.Text, "md5list.json")))
                {
                    File.Delete(System.IO.Path.Combine(this.textBox1.Text, "md5list.json"));
                    await Task.Run(() => { Downloader.download(System.IO.Path.Combine(this.textBox1.Text, "md5list.json")); });
                }
                else await Task.Run(() => { Downloader.download(System.IO.Path.Combine(this.textBox1.Text, "md5list.json")); });

            }
            catch (COSXML.CosException.CosClientException clientEx)
            {
                //请求失败
                this.textBox2.AppendText("CosClientException: " + clientEx.ToString() + Environment.NewLine);
                return;
            }
            catch (COSXML.CosException.CosServerException serverEx)
            {
                //请求失败
                this.textBox2.AppendText("CosClientException: " + serverEx.ToString() + Environment.NewLine);
                return;
            }
            using (StreamReader r = new StreamReader(System.IO.Path.Combine(this.textBox1.Text, "md5list.json")))
                json = r.ReadToEnd();
            json = json.Replace("\r", string.Empty).Replace("\n", string.Empty);
            Dictionary<string, string> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            await Task.Run(() =>
            {
                foreach (KeyValuePair<string, string> pair in jsonDict)
                {
                    MD5 = GetFileMd5Hash(System.IO.Path.Combine(this.textBox1.Text, pair.Key));
                    if (MD5.Length == 0)
                        newFileName.Add(pair.Key);
                    else if (MD5 != pair.Value)
                        updateFileName.Add(pair.Key);
                }
            });
            newFile = newFileName.Count();
            updateFile = updateFileName.Count();
            if(this.textBox2.Text.Length!=0)
                this.textBox2.AppendText("----------------------" + Environment.NewLine);

            if (newFile + updateFile == 0)
            {
                this.textBox2.AppendText("当前平台已是最新版本！" + Environment.NewLine);
                newFileName.Clear();updateFileName.Clear();
            }
            else
            {
                this.textBox2.AppendText($"发现{newFile}个新核心文件：" + Environment.NewLine);
                foreach (string filename in newFileName)
                    this.textBox2.AppendText(filename + Environment.NewLine);
                this.textBox2.AppendText(Environment.NewLine + $"发现{updateFile}个核心文件更新：" + Environment.NewLine);
                foreach (string filename in updateFileName)
                    this.textBox2.AppendText(filename + Environment.NewLine);
                this.textBox2.AppendText(Environment.NewLine + "请点击下载按钮选择文件" + Environment.NewLine + Environment.NewLine);
            }
            Checked = true;
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            string json, MD5;
            int newFile = 0, updateFile = 0;
            newFileName.Clear();
            updateFileName.Clear();
            Tencent_cos_download Downloader = new Tencent_cos_download();
            try
            {
                if (File.Exists(System.IO.Path.Combine(this.textBox1.Text, "clientForWinmd5list.json")))
                {
                    File.Delete(System.IO.Path.Combine(this.textBox1.Text, "clientForWinmd5list.json"));
                    await Task.Run(() => { Downloader.download(System.IO.Path.Combine(this.textBox1.Text, "clientForWinmd5list.json"), "clientForWinmd5list.json"); });
                }
                else await Task.Run(() => { Downloader.download(System.IO.Path.Combine(this.textBox1.Text, "clientForWinmd5list.json"), "clientForWinmd5list.json"); });
            }
            catch (COSXML.CosException.CosClientException clientEx)
            {
                //请求失败
                this.textBox2.AppendText("CosClientException: " + clientEx.ToString() + Environment.NewLine);
                return;
            }
            catch (COSXML.CosException.CosServerException serverEx)
            {
                //请求失败
                this.textBox2.AppendText("CosClientException: " + serverEx.ToString() + Environment.NewLine);
                return;
            }
            using(StreamReader r = new StreamReader(System.IO.Path.Combine(this.textBox1.Text, "clientForWinmd5list.json")))
                json = r.ReadToEnd();
            json = json.Replace("\r", string.Empty).Replace("\n", string.Empty);
            Dictionary<string, string> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            await Task.Run(() =>
            {
                foreach (KeyValuePair<string, string> pair in jsonDict)
                {
                    MD5 = GetFileMd5Hash(System.IO.Path.Combine(this.textBox1.Text, pair.Key));
                    if (MD5.Length == 0)
                        newFileName.Add(pair.Key);
                    else if (MD5 != pair.Value)
                        updateFileName.Add(pair.Key);
                }
            });
            newFile = newFileName.Count();
            updateFile = updateFileName.Count();
            if (this.textBox2.Text.Length != 0)
                this.textBox2.AppendText("----------------------" + Environment.NewLine);

            if (newFile + updateFile == 0)
            {
                this.textBox2.AppendText("暂无新unity文件！" + Environment.NewLine);
                newFileName.Clear(); updateFileName.Clear();
            }
            else
            {
                this.textBox2.AppendText($"发现{newFile}个新unity文件：" + Environment.NewLine);
                foreach (string filename in newFileName)
                    this.textBox2.AppendText(filename + Environment.NewLine);
                this.textBox2.AppendText(Environment.NewLine + $"发现{updateFile}个unity文件更新：" + Environment.NewLine);
                foreach (string filename in updateFileName)
                    this.textBox2.AppendText(filename + Environment.NewLine);
                this.textBox2.AppendText(Environment.NewLine + "请点击下载按钮选择文件" + Environment.NewLine + Environment.NewLine);
            }
            Checked = true;
        }
    }
}
