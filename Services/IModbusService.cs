using ModbusIndustrialAPI.Models.DTOs;

namespace ModbusIndustrialAPI.Services
{
  // Modbus服务接口：定义Modbus数据处理方法
  public interface IModbusService
  {
    List<RegisterDto> GetRegisterData(); // 获取寄存器数据
    void ReadModbusData(string ip = "127.0.0.1", int port = 502); // 读取Modbus设备数据
    Task<List<RegisterDto>> GetRegisterDataAsync(); // 异步获取寄存器数据
    Task SaveRegisterDataAsync(List<RegisterDto> registerDtos); // 保存数据到数据库
  }
}