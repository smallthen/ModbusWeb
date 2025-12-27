using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using ModbusIndustrialAPI.Models.DTOs;
using ModbusIndustrialAPI.Models.Entities;
using ModbusIndustrialAPI.Repositories.Interfaces;
using Polly;
using Polly.Extensions.Http;

namespace ModbusIndustrialAPI.Services
{
  // Modbus TCP服务实现：负责与Modbus设备通信
  public class ModbusTcpService : IModbusService
  {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAsyncPolicy _retryPolicy;

    public ModbusTcpService(IServiceScopeFactory scopeFactory)
    {
      _scopeFactory = scopeFactory;

      // 创建重试策略：断线3秒重连，最多5次，每次间隔指数1.5×
      _retryPolicy = Policy
          .Handle<Exception>()
          .WaitAndRetryAsync(
              retryCount: 5, // 最多重试5次
              sleepDurationProvider: retryAttempt =>
                  TimeSpan.FromSeconds(Math.Pow(1.5, retryAttempt)), // 指数退避：1.5^1, 1.5^2, 1.5^3...
              onRetry: (outcome, timespan, retryCount, context) =>
              {
                Console.WriteLine($"第{retryCount}次重试，等待{timespan.TotalSeconds}秒...");
              });
    }

    // 异步获取寄存器数据
    public async Task<List<RegisterDto>> GetRegisterDataAsync()
    {
      using var scope = _scopeFactory.CreateScope();
      var repository = scope.ServiceProvider.GetRequiredService<IRegisterDataRepository>();

      var registerDataList = await repository.GetAllAsync();
      return registerDataList.Select(rd => new RegisterDto
      {
        Address = rd.Address,
        Value = rd.Value,
        Time = rd.Time.ToString("yyyy-MM-dd HH:mm:ss") // 时间格式化为字符串
      }).ToList();
    }

    // 从Modbus设备读取数据
    public void ReadModbusData(string ip = "127.0.0.1", int port = 502)
    {
      bool connectionSuccessful = false;

      try
      {
        // 使用重试策略执行Modbus连接
        _retryPolicy.ExecuteAsync(async () =>
        {
          try
          {
            // 创建TCP连接到Modbus设备
            using var tcpClient = new TcpClient(ip, port);
            using var stream = tcpClient.GetStream();

            // 构建Modbus TCP请求帧（功能码03：读取保持寄存器）
            byte[] request = new byte[12] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x03, 0x00, 0x00, 0x00, 0x05 };
            stream.Write(request, 0, request.Length); // 发送请求

            // 读取设备响应
            byte[] response = new byte[1024];
            int len = stream.Read(response, 0, response.Length);

            // 解析响应数据并保存到数据库
            var currentTime = DateTime.Now;
            for (int i = 0; i < 5; i++)
            {
              var address = i + 1; // 寄存器地址（从1开始）
                                   // 从响应数据中解析寄存器值（高位字节<<8 | 低位字节）
              var value = (ushort)((response[9 + i * 2] << 8) | response[10 + i * 2]);

              var registerData = new RegisterData
              {
                Address = address,
                Value = value,
                Time = currentTime
              };

              // 使用作用域来获取仓储服务
              using var scope = _scopeFactory.CreateScope();
              var repository = scope.ServiceProvider.GetRequiredService<IRegisterDataRepository>();

              // 检查数据库中是否已存在该地址的数据
              var existingRecord = (await repository.GetByAddressAsync(address))
                          .OrderByDescending(r => r.Time) // 按时间降序排列，获取最新的
                          .FirstOrDefault();

              if (existingRecord != null)
              {
                // 如果存在，更新现有记录
                existingRecord.Value = value;
                existingRecord.Time = currentTime;
                await repository.UpdateAsync(existingRecord);
              }
              else
              {
                // 如果不存在，添加新记录
                await repository.AddAsync(registerData);
              }
            }
            Console.WriteLine("✅ Modbus数据采集成功！");
            connectionSuccessful = true;
          }
          catch (Exception ex)
          {
            // 捕获异常并抛出，让重试策略处理
            Console.WriteLine($"❌ Modbus连接失败：{ex.Message}");
            throw; // 重新抛出异常以触发重试
          }
        }).Wait(); // 等待异步操作完成
      }
      catch (Exception)
      {
        // 所有重试都失败，自动降级到假数据
        Console.WriteLine("⚠️ 所有重连尝试失败，自动降级到假数据模式");
        InitializeTestData().Wait();
        return; // 提前返回
      }

      // 如果重连成功，输出复活信息
      if (connectionSuccessful)
      {
        Console.WriteLine("TCP复活");
      }
    }

    // 异步初始化测试数据
    private async Task InitializeTestData()
    {
      Console.WriteLine("🔄 初始化假数据...");
      var currentTime = DateTime.Now;
      var testAddresses = new[] { 1, 2, 3, 4, 5 };

      using var scope = _scopeFactory.CreateScope();
      var repository = scope.ServiceProvider.GetRequiredService<IRegisterDataRepository>();

      foreach (var address in testAddresses)
      {
        // 检查是否已有该地址的数据
        var existingRecords = await repository.GetByAddressAsync(address);
        if (!existingRecords.Any())
        {
          // 如果没有该地址的数据，添加测试数据
          var testData = new RegisterData
          {
            Address = address,
            Value = new Random().Next(100, 200), // 随机值作为测试数据
            Time = currentTime
          };
          await repository.AddAsync(testData);
        }
      }

      Console.WriteLine("✅ 假数据初始化完成，系统可正常演示");
    }

    // 同步获取寄存器数据
    public List<RegisterDto> GetRegisterData()
    {
      using var scope = _scopeFactory.CreateScope();
      var repository = scope.ServiceProvider.GetRequiredService<IRegisterDataRepository>();

      var registerDataList = repository.GetAllAsync().Result;
      return registerDataList.Select(rd => new RegisterDto
      {
        Address = rd.Address,
        Value = rd.Value,
        Time = rd.Time.ToString("yyyy-MM-dd HH:mm:ss")
      }).ToList();
    }

    // 保存寄存器数据到数据库
    public async Task SaveRegisterDataAsync(List<RegisterDto> registerDtos)
    {
      using var scope = _scopeFactory.CreateScope();
      var repository = scope.ServiceProvider.GetRequiredService<IRegisterDataRepository>();

      foreach (var dto in registerDtos)
      {
        DateTime parsedTime;
        // 尝试解析时间字符串，如果解析失败则使用当前时间
        if (!DateTime.TryParse(dto.Time, out parsedTime))
        {
          parsedTime = DateTime.Now;
        }

        var registerData = new RegisterData
        {
          Address = dto.Address,
          Value = dto.Value,
          Time = parsedTime
        };

        await repository.AddAsync(registerData);
      }
    }
  }
}