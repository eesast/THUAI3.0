using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;
using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.CommandLineUtils;
using System.Runtime.InteropServices;


namespace Downloader_cli
{

    class Program
    {
        static List<string> newFileName = new List<string>();
        static List<string> updateFileName = new List<string>();
        public class Data
        {
            static string path;
            public static string FilePath = "";

            public Data(string path)
            {
                Data.path = Path.Combine(path, "/THUAI3.0.dat");
                if (File.Exists(Data.path))
                    using (StreamReader r = new StreamReader(Data.path))
                        Data.FilePath = r.ReadLine();
                else Data.FilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            public static void ChangeData(string newLine)
            {
                if (!Directory.Exists(@newLine))
                {
                    Console.Write($"是否创建新路径 {newLine}？y/n:");
                    if (Console.Read() != 'y')
                    {
                        Console.WriteLine("创建取消!"); return;
                    }
                }
                using (StreamWriter w = new StreamWriter(Data.path))
                    w.WriteLine(@newLine.Trim('\r').Trim('\n'));
                Console.WriteLine($"当前下载路径为{newLine}");
            }
        }
        class Tencent_cos_download
        {
            public void download(string download_dir, string key)
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

                    Dictionary<string, string> test = request.GetRequestHeaders();
                    request.SetCosProgressCallback(delegate (long completed, long total)
                    {
                        //Console.WriteLine(String.Format("progress = {0:##.##}%", completed * 100.0 / total));
                    });
                    //执行请求
                    GetObjectResult result = cosXml.GetObject(request);
                    //请求成功
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

            System.IO.FileStream fst = null;
            try
            {
                fst = new System.IO.FileStream(strFileFullPath, System.IO.FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5.ComputeHash(fst);

                StringBuilder sBuilder = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                fst.Close();
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

        private static void Check(bool isMain, bool isAll)
        {
            string json, MD5, jsonName, typeName;
            int newFile = 0, updateFile = 0;
            newFileName.Clear(); updateFileName.Clear();
            if (isMain)
            {
                jsonName = "md5list.json"; typeName = "核心";
            }
            else
            {
                jsonName = "ClientForLinuxmd5list.json"; typeName = "unity";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    jsonName = "ClientForLinuxmd5list.json"; Console.WriteLine("unity for linux:");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    jsonName = "ClientForMacmd5list.json"; Console.WriteLine("unity for mac:");
                }
                else Console.WriteLine("unity for windows:");
            }
            Tencent_cos_download Downloader = new Tencent_cos_download();
            try
            {
                if (File.Exists(System.IO.Path.Combine(Data.FilePath, jsonName)))
                {
                    File.Delete(System.IO.Path.Combine(Data.FilePath, jsonName));
                    Downloader.download(System.IO.Path.Combine(Data.FilePath, jsonName), jsonName);
                    //await Task.Run(() => { Downloader.download(System.IO.Path.Combine(Data.FilePath, jsonName)); });
                }
                else Downloader.download(System.IO.Path.Combine(Data.FilePath, jsonName), jsonName);
                //await Task.Run(() => { Downloader.download(System.IO.Path.Combine(Data.FilePath, jsonName)); });

            }
            catch (COSXML.CosException.CosClientException clientEx)
            {
                //请求失败
                Console.WriteLine("CosClientException: " + clientEx.ToString() + Environment.NewLine);
                return;
            }
            catch (COSXML.CosException.CosServerException serverEx)
            {
                //请求失败
                Console.WriteLine("CosClientException: " + serverEx.ToString() + Environment.NewLine);
                return;
            }
            using (StreamReader r = new StreamReader(System.IO.Path.Combine(Data.FilePath, jsonName)))
                json = r.ReadToEnd();
            json = json.Replace("\r", string.Empty).Replace("\n", string.Empty);
            Dictionary<string, string> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            foreach (KeyValuePair<string, string> pair in jsonDict) //await
            {
                MD5 = GetFileMd5Hash(System.IO.Path.Combine(Data.FilePath, pair.Key));
                if (MD5.Length == 0)
                    newFileName.Add(pair.Key);
                else if (MD5 != pair.Value)
                    updateFileName.Add(pair.Key);
            }

            newFile = newFileName.Count;
            updateFile = updateFileName.Count;
            Console.WriteLine("----------------------" + Environment.NewLine);

            if (newFile + updateFile == 0)
            {
                Console.WriteLine("当前平台已是最新版本！" + Environment.NewLine);
                newFileName.Clear(); updateFileName.Clear();
            }
            else
            {
                Console.WriteLine($"发现{newFile}个新{typeName}文件：" + Environment.NewLine);
                foreach (string filename in newFileName)
                    Console.WriteLine(filename);
                Console.WriteLine(Environment.NewLine + $"发现{updateFile}个{typeName}文件更新：" + Environment.NewLine);
                foreach (string filename in updateFileName)
                    Console.WriteLine(filename);
                Console.Write(Environment.NewLine + "是否下载新文件？ y/n：");
                if (Console.Read() != 'y') Console.WriteLine("下载取消!");
                else Download(isMain, isAll);

            }
        }

        private static void Download(bool isMain, bool isAll)
        {
            Tencent_cos_download Downloader = new Tencent_cos_download();
            bool skip1 = true, skip2 = true, skipped1 = false, skipped2 = false;
            if (isAll)
            {
                if (isMain && updateFileName.Exists(f => Path.GetFileName(f) == "player.cpp"))
                {
                    Console.Write("确认更新player.cpp文件? y/n:"); Console.ReadLine();
                    if (Console.Read() == 'y') skip1 = false;
                }
                if (!isMain && updateFileName.Exists(f => Path.GetFileName(f) == "server.playback"))
                {
                    Console.Write("确认更新server.playback文件? y/n:"); Console.ReadLine();
                    if (Console.Read() == 'y') skip1 = false;
                    
                }
                if (!isMain && updateFileName.Exists(f => Path.GetFileName(f) == "ClientConfig.json"))
                {
                    Console.Write("确认更新ClientConfig.json文件? y/n:"); Console.ReadLine();
                    if (Console.Read() == 'y') skip2 = false;
                }
            }

            int newFile = 0, updateFile = 0;
            int totalnew = newFileName.Count, totalupdate = updateFileName.Count;
            if (newFileName.Count > 0 || updateFileName.Count > 0)
            {
                try
                {
                    foreach (string Filename in newFileName)
                    {
                        Console.WriteLine(newFile + 1 + "/" + totalnew + ": 开始下载" + Filename);
                        Downloader.download(System.IO.Path.Combine(Data.FilePath, Filename), Filename);
                        Console.WriteLine(Filename + "下载完毕!" + Environment.NewLine); newFile++;
                    }
                    foreach (string Filename in updateFileName)
                    {
                        if (skip1 && (Path.GetFileName(Filename) == "player.cpp" || Path.GetFileName(Filename) == "server.playback")) { skipped1 = true; continue; }
                        if (skip2 && Path.GetFileName(Filename) == "ClientConfig.json") { skipped2 = true; continue; }
                        Console.WriteLine(updateFile + 1 + "/" + totalupdate + ": 开始更新" + Filename);
                        File.Delete(System.IO.Path.Combine(Data.FilePath, Filename));
                        Downloader.download(System.IO.Path.Combine(Data.FilePath, Filename), Filename);
                        Console.WriteLine(Filename + "更新完毕!" + Environment.NewLine); updateFile++;
                    }

                }
                catch (COSXML.CosException.CosClientException clientEx)
                {
                    //请求失败
                    Console.WriteLine("CosClientException: " + clientEx.ToString() + Environment.NewLine);
                    return;
                }
                catch (COSXML.CosException.CosServerException serverEx)
                {
                    //请求失败
                    Console.WriteLine("CosClientException: " + serverEx.ToString() + Environment.NewLine);
                    return;
                }
                catch (System.Exception)
                {
                    throw;
                }
                Console.WriteLine($"下载成功！共下载{newFile}个新文件，更新{updateFile}个文件");
                if(skipped1 && isMain) Console.WriteLine(Environment.NewLine + $"自动跳过了player.cpp文件");
                else if(skipped1 && !isMain) Console.WriteLine(Environment.NewLine + $"自动跳过了server.playback文件");
                if(skipped2) Console.WriteLine($"自动跳过了ClientConfig.json文件" + Environment.NewLine);
            }
            else Console.WriteLine("当前平台已是最新版本！" + Environment.NewLine);
            newFileName.Clear(); updateFileName.Clear();

        }
        public static void Main(string[] args)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Data Filedata = new Data(path);
            Program n = new Program();
            var app = new CommandLineApplication();
            app.HelpOption("-h|--help");
            CommandOption GetDir = app.Option("-gd|--get-dir", "显示下载路径", CommandOptionType.NoValue);
            CommandOption SetDir = app.Option("-sd|--set-dir", "设置下载路径", CommandOptionType.SingleValue);
            CommandOption CheckMain = app.Option("-cm|--check-main", "检查主程序更新,不会更新player.cpp文件", CommandOptionType.NoValue);
            CommandOption CheckUnity = app.Option("-cu|--check-unity", "检查unity更新,不会更新server.playback和ClientConfig.json文件", CommandOptionType.NoValue);
            CommandOption CheckMainAll = app.Option("-cma|--check-main-all", "检查主程序更新,将更新player.cpp文件", CommandOptionType.NoValue);
            CommandOption CheckUnityAll = app.Option("-cua|--check-unity-all", "检查unity更新,将更新server.playback和ClientConfig.json文件", CommandOptionType.NoValue);
            app.OnExecute(() =>
            {
                if (SetDir.HasValue()) Data.ChangeData(SetDir.Value());
                else if (GetDir.HasValue()) Console.WriteLine($"当前下载路径为：{Data.FilePath}");
                else if (CheckMain.HasValue()) Check(true, false);
                else if (CheckUnity.HasValue()) Check(false, false);
                else if (CheckMainAll.HasValue()) Check(true, true);
                else if (CheckUnityAll.HasValue()) Check(false, true);
                return 0;
            });
            app.Execute(args);
        }
        //set_dir,show_dir,

    }
}
