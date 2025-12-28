using System.Net.Sockets;
using System.Net;
using System.Text;
using ModbusIndustrialAPI.Models.DTOs;
using System.Text.Json;

namespace ModbusIndustrialAPI.Services
{
  public class ModbusTcpService : IModbusService
  {
    private readonly List<RegisterDto> _registerData;
    private readonly object _lockObject = new object();
    private bool _isCollecting = false;
    private Timer? _timer;
    private Action? _onDataUpdated;

    public ModbusTcpService()
    {
      _registerData = new List<RegisterDto>();

      // 初始化一些默认数据
      for (int i = 0; i < 10; i++)
      {
        _registerData.Add(new RegisterDto
        {
          Address = (ushort)(40000 + i),
          Value = new Random().Next(0, 1000),
          Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });
      }
    }

    public void SetOnDataUpdated(Action onDataUpdated)
    {
      _onDataUpdated = onDataUpdated;
    }

    public List<RegisterDto> GetRegisterData()
    {
      lock (_lockObject)
      {
        return _registerData.ToList();
      }
    }

    public void StartDataCollection()
    {
      if (_isCollecting) return;
      _isCollecting = true;

      // 使用定时器模拟数据采集，实际应用中替换为真正的Modbus通信
      _timer = new Timer(async (state) =>
      {
        await CollectDataAsync();
      }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5)); // 每5秒采集一次数据
    }

    public void StopDataCollection()
    {
      _isCollecting = false;
      _timer?.Dispose();
      _timer = null;
    }

    private async Task CollectDataAsync()
    {
      try
      {
        // 模拟Modbus数据采集
        var newData = new List<RegisterDto>();

        // 模拟从不同寄存器地址获取数据
        for (int i = 0; i < 10; i++)
        {
          var registerValue = new Random().Next(0, 1000); // 模拟随机值
          var registerDto = new RegisterDto
          {
            Address = (ushort)(40000 + i), // 模拟从40000开始的寄存器地址
            Value = registerValue,
            Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
          };

          newData.Add(registerDto);
        }

        // 更新本地数据
        lock (_lockObject)
        {
          _registerData.Clear();
          _registerData.AddRange(newData);
        }

        // 通知数据已更新
        _onDataUpdated?.Invoke();
      }
      catch (Exception ex)
      {
        // 记录错误，实际应用中可能需要写入日志
        Console.WriteLine($"Error collecting data: {ex.Message}");
      }
    }
  }
}