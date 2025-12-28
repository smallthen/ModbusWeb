using Microsoft.AspNetCore.SignalR;
using ModbusIndustrialAPI.Services;
using ModbusIndustrialAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IModbusService, ModbusTcpService>();

// 添加SignalR服务
builder.Services.AddSignalR();

// 添加Modbus数据发布服务
builder.Services.AddSingleton<ModbusDataPublisher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllers();

// 映射SignalR Hub
app.MapHub<ModbusHub>("/modbushub");

// 启动Modbus数据采集服务
using (var scope = app.Services.CreateScope())
{
  var modbusDataPublisher = scope.ServiceProvider.GetRequiredService<ModbusDataPublisher>();
  modbusDataPublisher.StartDataCollection();
}

app.Run();