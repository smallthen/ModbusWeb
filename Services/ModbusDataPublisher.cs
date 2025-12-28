using Microsoft.AspNetCore.SignalR;
using ModbusIndustrialAPI.Hubs;
using ModbusIndustrialAPI.Models.DTOs;
using ModbusIndustrialAPI.Services;

namespace ModbusIndustrialAPI.Services
{
  public class ModbusDataPublisher
  {
    private readonly IModbusService _modbusService;
    private readonly IHubContext<ModbusHub> _hubContext;

    public ModbusDataPublisher(IModbusService modbusService, IHubContext<ModbusHub> hubContext)
    {
      _modbusService = modbusService;
      _hubContext = hubContext;

      // 设置数据更新回调
      _modbusService.SetOnDataUpdated(() =>
      {
        _ = Task.Run(async () =>
        {
          await PublishData();
        });
      });
    }

    public void StartDataCollection()
    {
      _modbusService.StartDataCollection();
    }

    public void StopDataCollection()
    {
      _modbusService.StopDataCollection();
    }

    private async Task PublishData()
    {
      try
      {
        var data = _modbusService.GetRegisterData();
        await _hubContext.Clients.All.SendAsync("ReceiveModbusData", data);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error publishing data: {ex.Message}");
      }
    }
  }
}