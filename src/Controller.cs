using BeetleX.Buffers;
using BeetleX.FastHttpApi;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpBanchmarks
{
    [BeetleX.FastHttpApi.Controller]
    public class Banchmark
    {
        public void AddServer(string host)
        {
            Uri uri = new Uri(host);
            Service.Default.AddServer(uri.Host, uri.Port);
            Service.Default.Save();
        }

        public void DelServer(string host)
        {
            Uri uri = new Uri(host);
            Service.Default.RemoveServer(uri.Host, uri.Port);
            Service.Default.Save();
        }

        public object ListServer()
        {
            return Service.Default.ListServer();
        }

        public object NewMessage()
        {
            return new Message();
        }

        [Post]
        public void SaveMessage(Message msg)
        {
            Service.Default.SaveMessage(msg);
            Service.Default.Save();
        }

        public void DeleteMessage(string id)
        {
            Service.Default.DeleteMessage(id);
            Service.Default.Save();
        }

        public void DeleteCategoryMessages(string category)
        {
            Service.Default.DeleteCategoryMessages(category);
            Service.Default.Save();
        }

        public object ListMessages(string category)
        {
            return Service.Default.ListMessages(category);
        }

        public object Test(Message message, string host)
        {
            Uri uri = new Uri(host);
            var client = BeetleX.SocketFactory.CreateClient<BeetleX.Clients.TcpClient>(uri.Host, uri.Port);
            using (client)
            {
                byte[] data = message.ToBytes();
                client.TimeOut = 5000;
                client.Send(s => s.Write(data, 0, data.Length));
                var stream = client.Receive();
                data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                var hex = Service.Default.BytesToHex(data);
                var utf8 = Encoding.UTF8.GetString(data);
                return new { hex, utf8 };
            }
        }

        public object Download()
        {
            var items = Service.Default.GetStore();
            string text = Newtonsoft.Json.JsonConvert.SerializeObject(items);
            return new DownLoad(text);
        }

        public object Upload(string name, bool completed, string data)
        {
            byte[] array = Convert.FromBase64String(data);
            using (System.IO.Stream writer = System.IO.File.Open(name, System.IO.FileMode.Append))
            {
                writer.Write(array, 0, array.Length);
                writer.Flush();
            }
            if (completed)
            {
                try
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(name))
                    {
                        string value = reader.ReadToEnd();
                        Store setting = Newtonsoft.Json.JsonConvert.DeserializeObject<Store>(value);
                        Service.Default.SetStore(setting);
                    }
                }
                finally
                {
                    System.IO.File.Delete(name);
                }
            }
            return true;
        }

        public object ListMessageCategories()
        {
            return Service.Default.ListMessageCategores();
        }

        private Loadruner mLoadruner = new Loadruner();

        public object GetRunerDetail()
        {
            return mLoadruner?.Detail;
        }

        public object GetFileID()
        {
            return Guid.NewGuid().ToString("N");
        }

        public async Task Stop()
        {

            await Task.Run(() => mLoadruner.Stop());

        }
        [Post]
        public async Task Run(Server server, string[] ipaddress, List<Message> messages, Setting setting)
        {
            if (mLoadruner.Status != RunStatus.Runing && mLoadruner.Status != RunStatus.Init)
            {
                mLoadruner = new Loadruner();
                mLoadruner.Server = server;
                mLoadruner.IPAddress = ipaddress;
                mLoadruner.Setting = setting;
                mLoadruner.Messages = messages;
                await Task.Run(() => mLoadruner.Run());
            }
        }

        public object GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ipaddress = new List<object>();
            ipaddress.Add(new { IP = "127.0.0.1" });
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipaddress.Add(new { IP = ip.ToString() });
                }
            }
            return ipaddress;
        }
        public class DownLoad : BeetleX.FastHttpApi.IResult
        {
            public DownLoad(string text)
            {
                Text = text;
            }

            public string Text { get; set; }

            public IHeaderItem ContentType => ContentTypes.OCTET_STREAM;

            public int Length { get; set; }

            public bool HasBody => true;

            public void Setting(HttpResponse response)
            {
                response.Header.Add("Content-Disposition", "attachment;filename=TcpBanchmarks.json");
            }

            public void Write(PipeStream stream, HttpResponse response)
            {
                stream.Write(Text);
            }
        }
    }
}
