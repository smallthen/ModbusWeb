using Microsoft.EntityFrameworkCore;
using ModbusIndustrialAPI.Data;
using ModbusIndustrialAPI.Models.Entities;
using ModbusIndustrialAPI.Repositories.Interfaces;

namespace ModbusIndustrialAPI.Repositories.Implementations
{
  // 仓储实现：具体执行数据库操作
  public class RegisterDataRepository : IRegisterDataRepository
  {
    private readonly ModbusDbContext _context;

    public RegisterDataRepository(ModbusDbContext context)
    {
      _context = context;
    }

    // 获取所有寄存器数据
    public async Task<List<RegisterData>> GetAllAsync()
    {
      return await _context.RegisterData.ToListAsync();
    }

    // 根据ID获取数据
    public async Task<RegisterData?> GetByIdAsync(int id)
    {
      return await _context.RegisterData.FindAsync(id);
    }

    // 根据寄存器地址获取数据
    public async Task<List<RegisterData>> GetByAddressAsync(int address)
    {
      return await _context.RegisterData
          .Where(r => r.Address == address)
          .ToListAsync();
    }

    // 添加新数据
    public async Task AddAsync(RegisterData registerData)
    {
      _context.RegisterData.Add(registerData);
      await _context.SaveChangesAsync();
    }

    // 更新数据
    public async Task UpdateAsync(RegisterData registerData)
    {
      _context.RegisterData.Update(registerData);
      await _context.SaveChangesAsync();
    }

    // 删除数据
    public async Task DeleteAsync(int id)
    {
      var registerData = await _context.RegisterData.FindAsync(id);
      if (registerData != null)
      {
        _context.RegisterData.Remove(registerData);
        await _context.SaveChangesAsync();
      }
    }

    // 按时间范围查询数据
    public async Task<List<RegisterData>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
    {
      return await _context.RegisterData
          .Where(r => r.Time >= startTime && r.Time <= endTime)
          .ToListAsync();
    }
  }
}