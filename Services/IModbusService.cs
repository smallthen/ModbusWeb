using ModbusIndustrialAPI.Models.DTOs;

namespace ModbusIndustrialAPI.Services
{
  public interface IModbusService
  {
    List<RegisterDto> GetRegisterData();
    void StartDataCollection();
    void StopDataCollection();
    void SetOnDataUpdated(Action onDataUpdated);
  }
}