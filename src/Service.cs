using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TcpBanchmarks
{
    public class Service
    {

        public const string CONFIG_NAME = "config.json";

        private static Service mDefault = new Service();

        public static Service Default => mDefault;

        private List<Server> mServers = new List<Server>();

        private List<Message> mMessages = new List<Message>();

        public void AddServer(string host, int port)
        {
            if (mServers.Find(s => s.Host == host && s.Port == port) == null)
            {
                mServers.Add(new Server { Host = host, Port = port });
            }
        }


        public void RemoveServer(string host, int port)
        {
            var item = mServers.Find(s => s.Host == host && s.Port == port);
            if (item != null)
                mServers.Remove(item);
        }

        public object ListServer()
        {
            return mServers.ToArray();
        }

        public void SaveMessage(Message msg)
        {
            var item = mMessages.Find(m => m.ID == msg.ID);
            if (item == null)
            {
                item = msg;
                mMessages.Add(item);
            }
            else
            {
                item.Name = msg.Name;
                item.Category = msg.Category;
                item.Type = msg.Type;
                item.Data = msg.Data;
            }
        }

        public Message GetMessage(string id)
        {
            var item = mMessages.Find(m => m.ID == id);
            if (item == null)
                item = new Message();
            return item;
        }

        public object ListMessageCategores()
        {
            var items = mMessages;
            var result = from a in items
                         group a by a.Category into g
                         select new { g.Key };
            return result;
        }

        public void DeleteCategoryMessages(string category)
        {
            var items = (from a in mMessages where a.Category == category select a).ToArray();
            foreach (var item in items)
            {
                mMessages.Remove(item);
            }
        }

        public void DeleteMessage(string id)
        {
            var item = mMessages.Find(m => m.ID == id);
            if (item != null)
                mMessages.Remove(item);
        }

        public object ListMessages(string category)
        {
            var items = mMessages;
            var result = from a in items
                         group a by a.Category into g
                         select new { g.Key, Show = true, Items = from i in g.ToArray() orderby i.Name ascending select i };
            if (!string.IsNullOrEmpty(category))
                return from a in result where a.Key == category orderby a.Key ascending select a;
            return from a in result orderby a.Key ascending select a;
        }

        public Store GetStore()
        {
            Store store = new Store();
            store.Messages = mMessages;
            store.Servers = mServers;
            return store;
        }

        public void SetStore(Store data)
        {
            mMessages = data.Messages;
            mServers = data.Servers;
            Save();
        }

        public void Save()
        {
            Store store = new Store();
            store.Messages = mMessages;
            store.Servers = mServers;
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(CONFIG_NAME, false))
            {
                string text = Newtonsoft.Json.JsonConvert.SerializeObject(store);
                writer.Write(text);
                writer.Flush();
            }
        }

        public void Load()
        {
            if (System.IO.File.Exists(CONFIG_NAME))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(CONFIG_NAME))
                {
                    string text = reader.ReadToEnd();
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Store>(text);
                    mServers = data.Servers;
                    mMessages = data.Messages;
                }
            }
        }

        public string BytesToHex(byte[] data)
        {
            string hex = BitConverter.ToString(data);
            hex = hex.Replace("-", "");
            return hex;
        }

        public byte[] HexToBytes(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                    .ToArray();
        }



    }


    public class Store
    {
        public List<Server> Servers { get; set; }

        public List<Message> Messages { get; set; }
    }


    public class Server
    {
        public string Host { get; set; }

        public int Port { get; set; }
    }

    public class Message
    {

        public Message()
        {
            ID = Guid.NewGuid().ToString("N");
            Category = "Default";
            Type = "Utf8";
        }

        public string Name { get; set; }

        public string Category { get; set; }

        public string ID { get; set; }

        public string Type { get; set; }

        public string Data { get; set; }

        public Message Copy()
        {
            Message result = new Message();
            result.ID = ID;
            result.Name = Name;
            result.Type = Type;
            result.Data = Data;
            return result;
        }

        private byte[] mData;

        public byte[] ToBytes()
        {
            lock (this)
            {
                if (mData == null)
                {
                    if (Type == "Utf8")
                    {
                        Data = Data.Replace("\n", "\r\n");
                        mData = Encoding.UTF8.GetBytes(Data);
                    }
                    else
                    {
                        mData = Service.Default.HexToBytes(Data);
                    }
                }
            }
            return mData;
        }
    }

    public enum MessageType
    {
        Hex,
        Utf8
    }

}
