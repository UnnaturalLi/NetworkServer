using System;
using System.Diagnostics;

public abstract class ServerBase
{
    protected bool _isRunning;
    private int _targetTps = 60; // 默认60TPS
    private double _frameTimeMs;
    private Stopwatch _stopwatch;

    /// <summary>
    /// 获取或设置目标更新率(TPS)
    /// </summary>
    public int TargetTps
    {
        get => _targetTps;
        set
        {
            if (value <= 0)
                throw new ArgumentException("TPS must be greater than 0");
            
            _targetTps = value;
            _frameTimeMs = 1000.0 / _targetTps;
        }
    }

    /// <summary>
    /// 获取当前是否正在运行
    /// </summary>
    public bool IsRunning => _isRunning;

    protected ServerBase()
    {
        TargetTps = 60; // 默认60TPS
        _stopwatch = new Stopwatch();

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // 阻止进程直接退出
            Stop();
        };
    }

    /// <summary>
    /// 启动服务器
    /// </summary>
    public void Run()
    {
        if (_isRunning) return;

        _isRunning = true;
        
        // 初始化
        Init();
        
        // 开始
        Start();
        
        // 主循环
        MainLoop();
    }

    /// <summary>
    /// 停止服务器
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        
        // 关闭
        Close();
    }

    private void MainLoop()
    {
        _stopwatch.Start();
        double deltaTime = 0;
        double accumulator = 0;
        double previousTime = _stopwatch.Elapsed.TotalMilliseconds;

        while (_isRunning)
        {
            double currentTime = _stopwatch.Elapsed.TotalMilliseconds;
            deltaTime = currentTime - previousTime;
            previousTime = currentTime;
            accumulator += deltaTime;

            // 处理固定时间步长更新
            while (accumulator >= _frameTimeMs)
            {
                Update(_frameTimeMs / 1000.0); // 转换为秒
                accumulator -= _frameTimeMs;
            }

            // 处理剩余时间
            if (accumulator > 0)
            {
                Update(accumulator / 1000.0);
                accumulator = 0;
            }

            // 计算剩余帧时间
            double processTime = _stopwatch.Elapsed.TotalMilliseconds - currentTime;
            double sleepTime = _frameTimeMs - processTime;

            if (sleepTime > 1) // 只有需要睡眠超过1ms时才睡眠
            {
                System.Threading.Thread.Sleep((int)sleepTime);
            }
            else if (sleepTime > 0)
            {
                // 短时间等待，使用自旋等待提高精度
                System.Threading.Thread.SpinWait(100);
            }
        }
    }

    // 可重写的方法

    /// <summary>
    /// 初始化方法，在服务器启动时调用一次
    /// </summary>
    protected virtual void Init() { }

    /// <summary>
    /// 开始方法，在初始化后调用一次
    /// </summary>
    protected virtual void Start() { }

    /// <summary>
    /// 更新方法，按照设定的TPS频率调用
    /// </summary>
    /// <param name="deltaTime">距离上次更新的时间(秒)</param>
    protected virtual void Update(double deltaTime) { }

    /// <summary>
    /// 关闭方法，在服务器停止时调用一次
    /// </summary>
    protected virtual void Close() { }
}