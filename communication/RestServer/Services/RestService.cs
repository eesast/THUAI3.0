using Communication.RestServer.Models;
using Communication.RestServer.Services.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using static Communication.RestServer.Services.ServiceProvider;

namespace Communication.RestServer.Services
{
    //TODO: procedure exceptions as rest exceptions

    public class RestService : IRestService
    {
        private readonly HttpListener listener = new HttpListener();
        private static Dictionary<string, RestHandler> commands = new Dictionary<string, RestHandler>
        {
            ["/create"] = new RestHandler(CreateRoom),
            ["/query"] = new RestHandler(QueryRoom),
            ["/upload"] = new RestHandler(UploadFile),
            ["/join"] = new RestHandler(JoinRoom),
            ["/register"] = new RestHandler(RegisterUser),
            ["/login"] = new RestHandler(GetToken, false),
            ["/status"] = new RestHandler(GetStatus)
        };

        private static JObject GetStatus(JObject args, string username)
        {
            return new JObject
            {
                ["status"] = "success",
                ["result"] = Get<ICommService>().GetStatus((string)args["room"]).ToString()
            };
        }

        private static JObject RegisterUser(JObject args, string username)
        {
            if (username != "admin")
                throw new RestException("Permission denied.");
            Get<IAuthService>().CreateUser((string)args["username"], (string)args["password"]);
            return new JObject
            {
                ["status"] = "success"
            };
        }

        private static JObject GetToken(JObject args, string username)
        {
            return new JObject
            {
                ["status"] = "success",
                ["result"] = Get<IAuthService>().GetToken((string)args["username"], (string)args["password"])
            };
        }

        private static JObject CreateRoom(JObject args, string username)
        {
            string room = Get<IDockerService>().CreateDocker();
            Get<IDockerService>().RunDocker(room,
                new IPEndPoint(IPAddress.Loopback, Program.TcpServerPort));
            return new JObject
            {
                ["status"] = "success",
                ["result"] = room
            };
        }

        private static JObject QueryRoom(JObject args, string username)
        {
            JArray result = new JArray();
            foreach (var room in Get<ICommService>().QueryRoom())
            {
                result.Add(new JObject
                {
                    ["id"] = room.Item1,
                    ["count"] = room.Item2
                });
            }
            return new JObject
            {
                ["status"] = "success",
                ["result"] = result
            };
        }

        private static JObject UploadFile(JObject args, string username)
        {
            Get<IFileService>().UploadFile((string)args["name"],
                Convert.FromBase64String((string)args["data"]),
                username, (bool) args["sharing"]);
            return new JObject
            {
                ["status"] = "success"
            };
        }

        private static JObject JoinRoom(JObject args, string username)
        {
            string fileID = (string)args["name"];
            Get<ICommService>().JoinRoom((string)args["room"],
                fileID,
                Get<IFileService>().GetBinary(fileID, username));
            return new JObject
            {
                ["status"] = "success"
            };
        }

        private void GetContext(IAsyncResult result)
        {
            HttpListenerContext context = listener.EndGetContext(result);
            listener.BeginGetContext(GetContext, null);
            JObject output;

            try
            {
                context.Response.ContentEncoding = Encoding.UTF8;
                string url = context.Request.Url.LocalPath.ToString();
                JObject args;

                if (context.Request.HttpMethod == "GET")
                {
                    args = new JObject();
                    foreach (var key in context.Request.QueryString.AllKeys)
                        args[key] = context.Request.QueryString[key] ?? "";
                }
                else if (context.Request.HttpMethod == "POST")
                {
                    byte[] raw = new byte[context.Request.ContentLength64];
                    context.Request.InputStream.Read(raw, 0, raw.Length);
                    args = JObject.Parse(Encoding.UTF8.GetString(raw));
                }
                else
                    throw new RestException("Method not allowed");

                if (commands.ContainsKey(url))
                {
                    string username = args.ContainsKey("token") ? 
                        Get<IAuthService>().GetUser((string) args["token"]) : 
                        null;
                    if (username == null && commands[url].RequireToken)
                        throw new RestException("Invalid token");
                    else
                        output = commands[url].Callback(args, username);
                }
                else
                    throw new RestException("Invalid REST Command.");

            }
            catch (RestException e)
            {
                output = new JObject
                {
                    ["status"] = "failed",
                    ["result"] = e.ToString()
                };
            }
            catch (Exception e)
            {
                output = new JObject
                {
                    ["status"] = "error",
                    ["result"] = e.ToString()
                };
            }

            byte[] res = Encoding.UTF8.GetBytes(output.ToString());
            context.Response.OutputStream.Write(res, 0, res.Length);
            context.Response.StatusCode = 200;
            context.Response.Close();
        }

        public void StartServer(ushort port)
        {
            listener.Prefixes.Add($"http://*:{port}/");
            listener.Start();
            listener.BeginGetContext(GetContext, null);
        }

    }
}
