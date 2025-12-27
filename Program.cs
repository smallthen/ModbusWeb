using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModbusIndustrialAPI.Services;
using ModbusIndustrialAPI.Data;
using ModbusIndustrialAPI.Repositories.Interfaces;
using ModbusIndustrialAPI.Repositories.Implementations;

// 创建Web应用程序生成器
var builder = WebApplication.CreateBuilder(args);

// 添加控制器支持
builder.Services.AddControllers();

// 配置SQLite数据库
builder.Services.AddDbContext<ModbusDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ??
                     "Data Source=modbus.db"));

// 注册仓储服务（接口到实现的映射）
builder.Services.AddScoped<IRegisterDataRepository, RegisterDataRepository>();

// 注册Modbus服务为单例
builder.Services.AddSingleton<IModbusService, ModbusTcpService>();

var app = builder.Build();

// 配置中间件
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// 启动时初始化数据库并读取Modbus数据
using (var scope = app.Services.CreateScope())
{
  var context = scope.ServiceProvider.GetRequiredService<ModbusDbContext>();
  context.Database.EnsureCreated(); // 确保数据库已创建

  var modbusService = scope.ServiceProvider.GetRequiredService<IModbusService>();
  modbusService.ReadModbusData(); // 启动时读取一次数据
}

app.Run();