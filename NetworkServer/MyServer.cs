namespace NetworkServer;
public class MyServer: ServerBase
{
    protected override void Init()
    {
        Console.WriteLine("服务器初始化...");
        // 初始化数据库连接、网络监听等
    }

    protected override void Start()
    {
        Console.WriteLine("服务器启动...");
        // 启动网络监听等
    }

    protected override void Update(double deltaTime)
    {
        // 处理网络消息、更新游戏状态等
        Console.WriteLine($"服务器更新，DeltaTime: {deltaTime:F4}s");
    }

    protected override void Close()
    {
        Console.WriteLine("服务器关闭...");
        // 清理资源、保存数据等
    }
}