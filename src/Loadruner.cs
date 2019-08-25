using BeetleX.Buffers;
using BeetleX.Clients;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using BeetleX;
using System.Linq;

namespace TcpBanchmarks
{
    public class Loadruner
    {

        public Loadruner()
        {
            mLinks.Add(new LinkedList<AsyncTcpClient>());
            mLinks.Add(new LinkedList<AsyncTcpClient>());
            mLinks.Add(new LinkedList<AsyncTcpClient>());
        }

        public List<Message> Messages { get; set; }

        public string[] IPAddress { get; set; }

        public Server Server { get; set; }

        public Setting Setting { get; set; }

        public RunStatus Status { get; set; } = RunStatus.None;

        private int mCount;

        private List<LinkedList<BeetleX.Clients.AsyncTcpClient>> mLinks = new List<LinkedList<AsyncTcpClient>>();

        private static Dictionary<string, PortResource> mPortResources = new Dictionary<string, PortResource>();

        private List<BeetleX.Clients.AsyncTcpClient> mClients;

        private PortResource GetPortResource(string ip)
        {
            if (!mPortResources.TryGetValue(ip, out PortResource value))
            {
                value = new PortResource();
                mPortResources[ip] = value;
            }
            return value;
        }

        public CodeStatistics CodeStatistics { get; set; } = new CodeStatistics("tcp");


        public void Run()
        {
            Status = RunStatus.Init;
            if (Setting.Interval < 50)
                Setting.Interval = 50;
            mCount = 0;
            mClients = new List<BeetleX.Clients.AsyncTcpClient>(Setting.Connection);
            for (int i = 0; i <= 50000 * IPAddress.Length; i++)
            {
                string ip = IPAddress[i % IPAddress.Length];
                var port = GetPortResource(ip);

                AsyncTcpClient client = BeetleX.SocketFactory.CreateClient<AsyncTcpClient>(Server.Host, Server.Port);
                client.LocalEndPoint = new IPEndPoint(System.Net.IPAddress.Parse(ip), port.Next());
                if (client.Connect())
                {
                    client.DataReceive = OnClientReceive;
                    ClientToken token = new ClientToken();
                    client.Token = token;
                    mClients.Add(client);
                    mCount++;
                    if (Setting.Mode == "Interval")
                    {
                        token.Node = new LinkedListNode<AsyncTcpClient>(client);
                        mLinks[mCount % mLinks.Count].AddFirst(token.Node);
                    }
                    if (mClients.Count >= Setting.Connection)
                    {
                        Task.Run(() => BeginSend());
                        return;
                    }
                }
                else
                {
                    SocketException se = client.LastError as SocketException;
                    if (se != null)
                    {
                        if (se.ErrorCode == (int)SocketError.AddressNotAvailable ||
                            se.ErrorCode == (int)SocketError.ConnectionRefused ||
                            se.ErrorCode == (int)SocketError.NetworkUnreachable)
                        {

                            Stop();
                            throw se;
                        }
                    }
                }

            }

            if (mClients.Count > 0)
                Task.Run(() => BeginSend());
            return;
        }

        private void IntervalSend(LinkedList<BeetleX.Clients.AsyncTcpClient> link)
        {
            try
            {
                Double timestep = (double)Setting.Interval / (double)link.Count;
                long time = TimeWatch.GetElapsedMilliseconds();
                var c = link.Last;
                int i = 1;
                while (c != null)
                {
                    var token = (ClientToken)c.Value.Token;
                    token.LastRequestTime = (long)(time + timestep * i);
                    i++;
                    c = c.Previous;
                }

                while (Status == RunStatus.Runing)
                {
                    var item = link.Last;
                    if (item != null)
                    {
                        var client = item.Value;
                        var token = (ClientToken)client.Token;
                        if (TimeWatch.GetElapsedMilliseconds() - token.LastRequestTime > Setting.Interval)
                        {
                            SendMessage(client);
                            link.RemoveLast();
                            link.AddFirst(item);
                            token.LastRequestTime = TimeWatch.GetElapsedMilliseconds();
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(10);
                        }
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                }
            }
            catch (Exception e_)
            {
                Console.WriteLine(e_.Message + e_.StackTrace);
            }

        }

        private void BeginSend()
        {
            Status = RunStatus.Runing;
            CodeStatistics.Start();
            if (Setting.Mode == "Response")
            {
                foreach (var item in mClients)
                {
                    SendMessage(item);
                }
            }
            else
            {
                foreach (var item in mLinks)
                    //Task.Run(() => IntervalSend(item));
                    System.Threading.ThreadPool.QueueUserWorkItem((o) => IntervalSend(item));

            }
        }

        private void SendMessage(AsyncTcpClient client)
        {
            ClientToken token = (ClientToken)client.Token;
            token.RequestTime = TimeWatch.GetElapsedMilliseconds();
            token.MessageIndex++;
            int index = (int)(token.MessageIndex % Messages.Count);
            var msg = Messages[index];
            var data = msg.ToBytes();
            client.Send(s => s.Write(data, 0, data.Length));
            this.CodeStatistics.Send(data.Length);
        }

        private void OnClientReceive(IClient c, ClientReceiveArgs reader)
        {
            if (Status == RunStatus.Runing)
            {
                AsyncTcpClient client = (AsyncTcpClient)c;
                ClientToken token = (ClientToken)client.Token;
                long time = TimeWatch.GetElapsedMilliseconds() - token.RequestTime;
                this.CodeStatistics.Add(time);
                PipeStream stream = reader.Stream.ToPipeStream();
                int length = (int)stream.Length;
                this.CodeStatistics.Receive(length);
                stream.ReadFree(length);
                if (Setting.Mode == "Response")
                    SendMessage(client);
            }
        }

        public void Stop()
        {
            if (Status != RunStatus.Runing && Status != RunStatus.Init)
                return;
            Status = RunStatus.Closing;
            foreach (var item in mLinks)
                item.Clear();
            if (mClients != null)
            {
                foreach (var item in mClients)
                {
                    mCount--;
                    item.Token = null;
                    item.DisConnect();
                }
                mClients.Clear();
            }
            Status = RunStatus.Closed;
        }

        public RunerDetails Detail
        {
            get
            {
                RunerDetails result = new RunerDetails();
                result.Status = Status.ToString();
                result.Count = mCount;
                result.Setting = Setting;
                result.Statistics = CodeStatistics.GetData();
                result.Delay =
                    from a in result.Statistics.GetTimeStats()
                    where a.Count > 0
                    select new StatsBaseItem(a);
                return result;
            }
        }


    }


    public class ClientToken
    {
        public LinkedListNode<BeetleX.Clients.AsyncTcpClient> Node { get; set; }

        public long RequestTime { get; set; }

        public long MessageIndex { get; set; }

        public long LastRequestTime { get; set; }

    }

    public class PortResource
    {
        private int mValue = 10000;

        public int Next()
        {
            mValue++;
            if (mValue > 60000)
                mValue = 10000;
            return mValue;
        }
    }


    public class RunerDetails
    {
        public string Status { get; set; }

        public int? Count { get; set; }

        public StatisticsData Statistics { get; set; }

        public Setting Setting { get; set; }

        public object Delay { get; set; }
    }


    public enum RunStatus
    {
        None,
        Init,
        Runing,
        Closing,
        Closed
    }


    public class Setting
    {
        public int Connection { get; set; }

        public string Mode { get; set; }

        public int Interval { get; set; }
    }

    public class CodeStatistics
    {
        public CodeStatistics(string name)
        {
            mLastTime = BeetleX.TimeWatch.GetTotalSeconds();
            Name = name;
        }

        public string Name { get; set; }

        private long mCount;

        public long Count => mCount;

        private double mLastTime;

        private long mLastCount;

        private double mFirstTime;

        public int AvgRps
        {
            get; set;
        }

        public int MaxRps
        {
            get; set;
        }

        public int Rps
        {
            get; set;
        }

        public void Start()
        {
            mLastTime = BeetleX.TimeWatch.GetTotalSeconds();
        }

        public void Execute()
        {
            double time = TimeWatch.GetTotalSeconds() - mLastTime;
            mLastTime = TimeWatch.GetTotalSeconds();
            int value = (int)((double)(mCount - mLastCount) / time);
            mLastCount = mCount;
            Rps = value;

            SendIOPer = (long)((double)(mSendIO - mLastSendIO) / time);
            mLastSendIO = mSendIO;

            SendBytesPer = (long)((double)(mSendBytes - mLastSendBytes) / time);
            mLastSendBytes = mSendBytes;

            ReceiveIOPer = (long)((double)(mReceiveIO - mLastReceiveIO) / time);
            mLastReceiveIO = mReceiveIO;

            ReceiveBytesPer = (long)((double)(mReceiveBytes - mLastReceiveBytes) / time);
            mLastReceiveBytes = mReceiveBytes;

            if (value > MaxRps)
                MaxRps = value;
            AvgRps = (int)(mCount / (TimeWatch.GetTotalSeconds() - mFirstTime));

        }

        private long mLastSendIO;

        private long mSendIO;

        public long SendIO => mSendIO;

        public long SendIOPer { get; set; }

        private long mLastSendBytes;

        private long mSendBytes;

        public long SendBytes => mSendBytes;

        public long SendBytesPer { get; set; }

        private long mLastReceiveIO;

        private long mReceiveIO;

        public long ReceiveIO => mReceiveIO;

        public long ReceiveIOPer { get; set; }

        private long mLastReceiveBytes;

        private long mReceiveBytes;

        public long ReceiveBytes => mReceiveBytes;

        public long ReceiveBytesPer { get; set; }

        public void Send(int bytes)
        {
            System.Threading.Interlocked.Add(ref mSendBytes, bytes);
            System.Threading.Interlocked.Increment(ref mSendIO);
        }

        public void Receive(int bytes)
        {
            System.Threading.Interlocked.Add(ref mReceiveBytes, bytes);
            System.Threading.Interlocked.Increment(ref mReceiveIO);
        }

        public void Add(long time)
        {
            long value = System.Threading.Interlocked.Increment(ref mCount);
            if (value == 1)
            {
                mFirstTime = TimeWatch.GetTotalSeconds();
                mLastTime = mFirstTime;
            }
            if (time <= 5)
                System.Threading.Interlocked.Increment(ref ms5);
            else if (time <= 10)
                System.Threading.Interlocked.Increment(ref ms10);
            else if (time <= 20)
                System.Threading.Interlocked.Increment(ref ms20);
            else if (time <= 50)
                System.Threading.Interlocked.Increment(ref ms50);
            else if (time <= 100)
                System.Threading.Interlocked.Increment(ref ms100);
            else if (time <= 200)
                System.Threading.Interlocked.Increment(ref ms200);
            else if (time <= 500)
                System.Threading.Interlocked.Increment(ref ms500);
            else if (time <= 1000)
                System.Threading.Interlocked.Increment(ref ms1000);
            else if (time <= 2000)
                System.Threading.Interlocked.Increment(ref ms2000);
            else if (time <= 5000)
                System.Threading.Interlocked.Increment(ref ms5000);
            else if (time <= 10000)
                System.Threading.Interlocked.Increment(ref ms10000);
            else
                System.Threading.Interlocked.Increment(ref msOther);
        }

        public override string ToString()
        {
            return mCount.ToString();
        }

        private long ms5;

        private long ms5LastCount;

        public long Time5ms => ms5;

        private long ms10;

        private long ms10LastCount;

        public long Time10ms => ms10;

        private long ms20;

        private long ms20LastCount;

        public long Time20ms => ms20;

        private long ms50;

        private long ms50LastCount;

        public long Time50ms => ms50;

        private long ms100;

        private long ms100LastCount;

        public long Time100ms => ms100;

        private long ms200;

        private long ms200LastCount;

        public long Time200ms => ms200;

        private long ms500;

        private long ms500LastCount;

        public long Time500ms => ms500;

        private long ms1000;

        private long ms1000LastCount;

        public long Time1000ms => ms1000;

        private long ms2000;

        private long ms2000LastCount;

        public long Time2000ms => ms2000;

        private long ms5000;

        private long ms5000LastCount;

        public long Time5000ms => ms5000;

        private long ms10000;

        private long ms10000LastCount;

        public long Time10000ms => ms10000;

        private long msOther;

        private long msOtherLastCount;

        public long TimeOtherms => msOther;

        private double mLastRpsTime = 0;

        public StatisticsData GetData()
        {
            Execute();
            StatisticsData result = new StatisticsData();
            result.Count = Count;
            result.Rps = Rps;
            result.MaxRps = MaxRps;
            result.AvgRps = AvgRps;
            result.SendIO = SendIO;
            result.SendIOPer = SendIOPer;
            result.SendBytes = SendBytes;
            result.SendBytesPer = SendBytesPer;
            result.ReceiveIO = ReceiveIO;
            result.ReceiveIOPer = ReceiveIOPer;
            result.ReceiveBytes = ReceiveBytes;
            result.ReceiveBytesPer = ReceiveBytesPer;
            result.Name = Name;
            result.Times.Add(Time5ms);
            result.Times.Add(Time10ms);
            result.Times.Add(Time20ms);
            result.Times.Add(Time50ms);
            result.Times.Add(Time100ms);
            result.Times.Add(Time200ms);
            result.Times.Add(Time500ms);
            result.Times.Add(Time1000ms);
            result.Times.Add(Time2000ms);
            result.Times.Add(Time5000ms);
            result.Times.Add(Time10000ms);
            result.Times.Add(TimeOtherms);
            double now = TimeWatch.GetTotalSeconds();
            double time = now - mLastRpsTime;


            int value = (int)((double)(ms5 - ms5LastCount) / time);
            ms5LastCount = ms5;
            result.TimesRps.Add(value);

            value = (int)((double)(ms10 - ms10LastCount) / time);
            ms10LastCount = ms10;
            result.TimesRps.Add(value);


            value = (int)((double)(ms20 - ms20LastCount) / time);
            ms20LastCount = ms20;
            result.TimesRps.Add(value);


            value = (int)((double)(ms50 - ms50LastCount) / time);
            ms50LastCount = ms50;
            result.TimesRps.Add(value);


            value = (int)((double)(ms100 - ms100LastCount) / time);
            ms100LastCount = ms100;
            result.TimesRps.Add(value);


            value = (int)((double)(ms200 - ms200LastCount) / time);
            ms200LastCount = ms200;
            result.TimesRps.Add(value);


            value = (int)((double)(ms500 - ms500LastCount) / time);
            ms500LastCount = ms500;
            result.TimesRps.Add(value);


            value = (int)((double)(ms1000 - ms1000LastCount) / time);
            ms1000LastCount = ms1000;
            result.TimesRps.Add(value);


            value = (int)((double)(ms2000 - ms2000LastCount) / time);
            ms2000LastCount = ms2000;
            result.TimesRps.Add(value);


            value = (int)((double)(ms5000 - ms5000LastCount) / time);
            ms5000LastCount = ms5000;
            result.TimesRps.Add(value);


            value = (int)((double)(ms10000 - ms10000LastCount) / time);
            ms10000LastCount = ms10000;
            result.TimesRps.Add(value);


            value = (int)((double)(msOther - msOtherLastCount) / time);
            msOtherLastCount = msOther;
            result.TimesRps.Add(value);


            mLastRpsTime = now;
            return result;
        }

    }

    public class StatisticsData
    {
        public string Name { get; set; }

        public long Count { get; set; }

        public long Rps { get; set; }

        public long MaxRps { get; set; }

        public long AvgRps { get; set; }

        public long SendIO { get; set; }

        public long SendIOPer { get; set; }

        public long SendBytes { get; set; }

        public long SendBytesPer { get; set; }

        public long ReceiveIO { get; set; }

        public long ReceiveIOPer { get; set; }

        public long ReceiveBytes { get; set; }

        public long ReceiveBytesPer { get; set; }

        public List<long> Times { get; set; } = new List<long>();

        public List<long> TimesRps { get; set; } = new List<long>();

        public TimeStats[] GetTimeStats(int count = 20)
        {
            List<TimeStats> result = new List<TimeStats>();
            result.Add(new TimeStats { EndTime = 5, Count = Times[0], Color = 0, Rps = TimesRps[0] });
            result.Add(new TimeStats { EndTime = 10, StartTime = 5, Count = Times[1], Color = 1, Rps = TimesRps[1] });
            result.Add(new TimeStats { EndTime = 20, StartTime = 10, Count = Times[2], Color = 2, Rps = TimesRps[2] });
            result.Add(new TimeStats { EndTime = 50, StartTime = 20, Count = Times[3], Color = 3, Rps = TimesRps[3] });
            result.Add(new TimeStats { EndTime = 100, StartTime = 50, Count = Times[4], Color = 4, Rps = TimesRps[4] });
            result.Add(new TimeStats { EndTime = 200, StartTime = 100, Count = Times[5], Color = 5, Rps = TimesRps[5] });
            result.Add(new TimeStats { EndTime = 500, StartTime = 200, Count = Times[6], Color = 6, Rps = TimesRps[6] });
            result.Add(new TimeStats { EndTime = 1000, StartTime = 500, Count = Times[7], Color = 7, Rps = TimesRps[7] });
            result.Add(new TimeStats { EndTime = 2000, StartTime = 1000, Count = Times[8], Color = 8, Rps = TimesRps[8] });
            result.Add(new TimeStats { EndTime = 5000, StartTime = 2000, Count = Times[9], Color = 9, Rps = TimesRps[9] });
            result.Add(new TimeStats { EndTime = 10000, StartTime = 5000, Count = Times[10], Color = 10, Rps = TimesRps[10] });
            result.Add(new TimeStats { StartTime = 10000, Count = Times[11], Color = 10, Rps = TimesRps[11] });
            var items = (from a in result select a).Take(count).ToArray();
            return items;
        }

    }

    public class TimeStats
    {
        public long Count { get; set; }

        public int StartTime { get; set; }

        public int EndTime { get; set; }

        public int Color { get; set; }

        public long Rps { get; set; }

    }

    public class StatsBaseItem
    {
        public StatsBaseItem()
        {

        }

        public StatsBaseItem(string label, long data, int colorIndex = 0)
        {
            this.name = label;
            this.value = data;
            this.color = colorIndex;
        }

        public StatsBaseItem(TimeStats timeStats)
        {
            this.color = timeStats.Color;
            rps = timeStats.Rps;
            if (timeStats.StartTime > 0 && timeStats.EndTime > 0)
            {
                if (timeStats.StartTime >= 1000)
                    name = $"{timeStats.StartTime / 1000}s";
                else
                    name = $"{timeStats.StartTime}ms";

                if (timeStats.EndTime >= 1000)
                    name += $"-{timeStats.EndTime / 1000}s";
                else
                    name += $"-{timeStats.EndTime}ms";

            }
            else if (timeStats.StartTime > 0)
            {
                if (timeStats.StartTime >= 1000)
                    name = $">{timeStats.StartTime / 1000}s";
                else
                    name = $">{timeStats.StartTime}ms";
            }
            else
            {
                name = $"<{timeStats.EndTime}ms";
            }
            value = timeStats.Count;
        }

        public StatsBaseItem PercentWith(long count)
        {
            double p = (double)value / (double)count;
            if (p > 0)
            {
                percent = $"({(int)(p * 10000) / 100d}%)";
            }
            return this;
        }

        public StatsBaseItem AddItems(IEnumerable<StatisticsData> items)
        {
            foreach (var item in items)
            {
                Items.Add(new StatsBaseItem(item));
            }

            return this;
        }

        public List<StatsBaseItem> Items { get; set; } = new List<StatsBaseItem>();

        public StatsBaseItem(StatisticsData statisticsData, int colorIndex = -1)
        {
            maxRps = statisticsData.MaxRps;
            avgRps = statisticsData.AvgRps;
            value = statisticsData.Count;
            if (colorIndex != -1)
                this.color = colorIndex;
            else
            {
                if (int.TryParse(statisticsData.Name, out int code))
                {
                    if (code < 200)
                    {
                        this.color = 0;
                    }
                    else if (code < 300)
                        this.color = 1;
                    else if (code < 400)
                        this.color = 2;
                    else if (code < 500)
                        this.color = 3;
                    else if (code < 6)
                        this.color = 4;
                    else
                        this.color = 5;
                }
            }
            name = statisticsData.Name;
            rps = statisticsData.Rps;

        }

        public string percent { get; set; }

        public long maxRps { get; set; }

        public long avgRps { get; set; }

        public string name { get; set; }

        public long value { get; set; }

        public long rps { get; set; }

        public int color { get; set; }
    }
}
