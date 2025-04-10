using System.Net;
using System.Net.Sockets;

namespace NetworkServer;
public struct UDPPacket
{
    public string ClientKey;
    public byte[] Data;
}
public class UDPClient
{
    
    public Thread sendThread;
    public IPEndPoint ipPort;
    public AutoResetEvent SendDataSignal;
    public Queue<byte[]> PendingSendData;
    public void Send(byte[] data)
    {//abc
        lock (PendingSendData)
        {
            var copiedData = (byte[])data.Clone();
            PendingSendData.Enqueue(copiedData);
            SendDataSignal.Set();
        }
    }
}
public class UDPServer : ServerBase
{
    Thread listenThread;
    Dictionary<string, UDPClient> clients = new Dictionary<string, UDPClient>();
    public UdpClient server;
    private Queue<UDPPacket> m_RecvData=new Queue<UDPPacket>();
    private IPAddress ip;
    private int port;
    private int n = 0;

    public Thread CreateThread(ThreadStart threadStart)
    {
        var t = new Thread(threadStart)
        {
            IsBackground = true,
            Priority = ThreadPriority.Normal
        };
        t.Start();
        return t;
    }
    public void AddReceiveData(UDPPacket packet)
    {
        lock (m_RecvData)
        {
            m_RecvData.Enqueue(packet);
        }
    }

    public bool getReceivedData(Queue<UDPPacket> packets)
    {
        lock (m_RecvData)
        {
            while (m_RecvData.Count > 0)
            {
                packets.Enqueue(m_RecvData.Dequeue());
            }
        }

        return packets.Count > 0;
    }
    public void SendToClient(string clientKey, byte[] data)
    {
        lock (clients)
        {
            if (!clients.TryGetValue(clientKey, out var clientInfo))
            {
                return;
            }
            clientInfo.Send(data);
        }
    }
    private void ClientSendThreadFunc(UDPClient clientInfo)
    {
        var dataToSend = new Queue<byte[]>();
        while (true)
        {
            if (_isRunning)
            {
                return;
            }
            clientInfo.SendDataSignal.WaitOne();
            try
            {
                lock (clientInfo.PendingSendData)
                {
                    while (clientInfo.PendingSendData.Count != 0)
                    {
                        var packet = clientInfo.PendingSendData.Dequeue();
                        dataToSend.Enqueue(packet);
                    }
                }
                while (dataToSend.Count != 0)
                {
                    var data = dataToSend.Dequeue();
                    if (data != null && data.Length > 0)
                    {
                        server.Send(data, data.Length, clientInfo.ipPort);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }
    }
    public void listenThreadFunc()
    {
        var remoteIPEndPoint =
            new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            if (!_isRunning)
            {
                return;
            }

            try
            {
                while (server != null &&
                       server.Available > 0)
                {
                    if (!_isRunning)
                    {
                        return;
                    }
                    var data = server.Receive(ref remoteIPEndPoint);
                    lock (clients)
                    {
                        if (!clients.TryGetValue(
                                remoteIPEndPoint.ToString(),
                                out UDPClient client))
                        {
                            client = new UDPClient()
                            {
                                ipPort = new IPEndPoint(remoteIPEndPoint.Address, remoteIPEndPoint.Port),
                                SendDataSignal = new AutoResetEvent(false),
                                PendingSendData = new Queue<byte[]>()
                            };
                            client.sendThread = CreateThread(() => ClientSendThreadFunc(client));
                            clients.Add(remoteIPEndPoint.ToString(), client);
                            AddReceiveData(new UDPPacket()
                            {
                                ClientKey = remoteIPEndPoint.ToString(), Data = data
                            });
                        }
                    }
                }

                do
                {
                    if (!_isRunning)
                    {
                        return;
                    }
                    Thread.Sleep(1);
                }while(server.Available<=0||server==null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    } 
    public void BroadcastToClients(byte[] data)
    {
        lock (clients)
        {
            foreach (var pair in clients)
            {
                var clientInfo = pair.Value;
                clientInfo.Send(data);
            }
        }
    }

    protected override void Init()
    {
        base.Init();
        string hostName = Dns.GetHostName();
        IPAddress[] localIPs = Dns.GetHostAddresses(hostName);
        ip = localIPs.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        if(ip == null)
        {
            throw new Exception("未找到有效的IPv4地址");
        }
        int port = 12345; 
        server = new UdpClient(new IPEndPoint(ip, port));
        Console.WriteLine(ip + ":" + port);
    }

    protected override void Start()
    {
        base.Start();
        listenThread = new Thread(listenThreadFunc);
    }

    protected override void Update(double deltaTime)
    {
        base.Update(deltaTime);
        
    }

    protected override void Close()
    {
        base.Close();
        
        server.Close();
        Console.WriteLine("close");
        
    }
}