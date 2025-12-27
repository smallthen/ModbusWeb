using Microsoft.EntityFrameworkCore;
using ModbusIndustrialAPI.Data;
using ModbusIndustrialAPI.Models.Entities;
using System.Diagnostics;
using Xunit;

namespace ModbusIndustrialAPI.IntegrationTests
{
  public class WalPerformanceTest
  {
    [Fact]
    public async Task ParallelWrites_ShouldCompleteUnderOneSecond()
    {
      // 使用共享的内存数据库，通过在连接字符串中添加"Mode=Memory;Cache=Shared"
      var connectionString = "DataSource=file:WalPerformanceTestDb?mode=memory&cache=shared";

      var options = new DbContextOptionsBuilder<ModbusDbContext>()
          .UseSqlite(connectionString)
          .Options;

      // 创建数据库并启用WAL模式
      using (var setupContext = new ModbusDbContext(options))
      {
        await setupContext.Database.OpenConnectionAsync();
        await setupContext.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
        await setupContext.Database.EnsureCreatedAsync();
      }

      var tasks = new List<Task>();
      var sw = Stopwatch.StartNew();

      // 并行执行1000次写操作 - 每个任务使用自己的DbContext实例，但共享数据库
      for (int i = 0; i < 1000; i++)
      {
        int id = i; // 闭包变量
        tasks.Add(Task.Run(async () =>
        {
          // 为每个任务创建独立的DbContext实例，使用相同的共享连接
          using var context = new ModbusDbContext(options);
          var testData = new RegisterData
          {
            Address = id % 10, // 使用0-9的地址
            Value = new Random(id).Next(100, 200), // 使用id作为随机种子以避免相同值
            Time = DateTime.Now.AddMilliseconds(id) // 确保时间戳不同
          };
          context.RegisterData.Add(testData);
          await context.SaveChangesAsync();
        }));
      }

      await Task.WhenAll(tasks);
      sw.Stop();

      // 验证总耗时小于1秒
      Assert.True(sw.Elapsed.TotalSeconds < 1.0,
          $"Expected completion under 1 second, but took {sw.Elapsed.TotalSeconds:F2} seconds");

      // 验证数据已写入 - 使用新的上下文来查询
      using var queryContext = new ModbusDbContext(options);
      var count = await queryContext.RegisterData.CountAsync();
      Assert.Equal(1000, count);
    }
  }
}