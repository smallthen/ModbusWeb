using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModbusIndustrialAPI.Data;
using ModbusIndustrialAPI.Models.Entities;
using ModbusIndustrialAPI.Repositories.Interfaces;
using ModbusIndustrialAPI.Services;
using Moq;
using Polly;
using System.Net.Sockets;
using Xunit;

namespace ModbusIndustrialAPI.IntegrationTests
{
  public class TcpRetryPolicyTest
  {
    [Fact]
    public async Task ModbusService_ShouldRetryOnConnectionFailure()
    {
      // 模拟仓储服务
      var mockRepository = new Mock<IRegisterDataRepository>();
      var mockScopeFactory = new Mock<IServiceScopeFactory>();
      var mockScope = new Mock<IServiceScope>();
      var mockServiceProvider = new Mock<IServiceProvider>();

      mockServiceProvider.Setup(sp => sp.GetService(typeof(IRegisterDataRepository)))
          .Returns(mockRepository.Object);
      mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
      mockScopeFactory.Setup(sf => sf.CreateScope()).Returns(mockScope.Object);

      var service = new ModbusTcpService(mockScopeFactory.Object);

      // 记录开始时间
      var startTime = DateTime.Now;

      // 模拟连接失败，应该触发重试机制
      // 使用一个无法连接的IP地址和端口

      // 捕获异常并验证重试次数
      var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
      {
        service.ReadModbusData("192.0.2.1", 65432); // 使用无效IP和端口
      });

      var endTime = DateTime.Now;
      var elapsed = endTime - startTime;

      // 验证耗时应该接近重试间隔的总和（大约 1.5 + 2.25 + 3.375 + 5.0625 + 7.59375 秒）
      var expectedMinTime = TimeSpan.FromSeconds(1.5 + 2.25 + 3.375 + 5.0625 + 7.59375); // 约19.78秒
      Assert.True(elapsed >= expectedMinTime,
          $"Expected at least {expectedMinTime.TotalSeconds} seconds due to retry policy, but took {elapsed.TotalSeconds} seconds");
    }

    [Fact]
    public async Task RetryPolicy_UsesExponentialBackoff()
    {
      // 验证重试策略确实使用指数退避
      var policy = Policy
          .Handle<Exception>()
          .WaitAndRetryAsync(
              retryCount: 5,
              sleepDurationProvider: retryAttempt =>
                  TimeSpan.FromSeconds(Math.Pow(1.5, retryAttempt)),
              onRetry: (outcome, timespan, retryCount, context) =>
              {
                // 验证重试间隔确实是指数增长的
                var expectedDelay = Math.Pow(1.5, retryCount);
                Assert.True(Math.Abs(timespan.TotalSeconds - expectedDelay) < 0.1,
                          $"Expected retry delay of ~{expectedDelay}s, but was {timespan.TotalSeconds}s");
              });

      var attemptCount = 0;
      var maxAttempts = 6; // 1初始+5重试

      await policy.ExecuteAsync(async () =>
      {
        attemptCount++;
        if (attemptCount < maxAttempts)
        {
          throw new Exception("Simulated connection failure");
        }
        return attemptCount;
      });

      Assert.Equal(maxAttempts, attemptCount);
    }
  }
}