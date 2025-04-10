using NetworkServer;

class Program
{
    static void Main(string[] args)
    {
        var server = new UDPServer();
        server.TargetTps = 60; // 设置30TPS
        server.Run();
    }
}