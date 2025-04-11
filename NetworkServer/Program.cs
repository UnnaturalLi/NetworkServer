using NetworkServer;

class Program
{
    static void Main(string[] args)
    {
        var server = new UDPTestServer();
        server.Run();
    }
    
}

class UDPTestServer : ServerBase
{
    private UDPServer server;
    private Queue<UDPPacket> packets = new Queue<UDPPacket>();
    protected override void Init()
    {
        base.Init();
        server = new UDPServer();
        
    }

    protected override void Start()
    {
        base.Start();
        server.Run();
    }

    protected override void Update(double deltaTime)
    {
        if(!_isRunning)return;
        base.Update(deltaTime);
        if (server.getReceivedData(packets))
        {
            lock (packets)
            {
                while (packets.Count>0)
                {
                    var pack = packets.Dequeue();
                    Console.WriteLine($"Got a message from {pack.ClientKey}");
                }
            }
        }
    }
    protected override void Close()
    {
        base.Close();
        server.Close();
        server = null;
    }

    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return base.ToString();
    }
}