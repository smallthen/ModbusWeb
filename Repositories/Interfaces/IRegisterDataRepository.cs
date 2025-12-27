using ModbusIndustrialAPI.Models.Entities;

namespace ModbusIndustrialAPI.Repositories.Interfaces
{
  // 仓储接口：定义数据库操作方法
  public interface IRegisterDataRepository
  {
    Task<List<RegisterData>> GetAllAsync(); // 获取所有数据
    Task<RegisterData> GetByIdAsync(int id); // 根据ID获取数据
    Task<List<RegisterData>> GetByAddressAsync(int address); // 根据地址获取数据
    Task AddAsync(RegisterData registerData); // 添加数据
    Task UpdateAsync(RegisterData registerData); // 更新数据
    Task DeleteAsync(int id); // 删除数据
    Task<List<RegisterData>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime); // 时间范围查询
  }
}