using Microsoft.EntityFrameworkCore;
using ModbusIndustrialAPI.Models.Entities;

namespace ModbusIndustrialAPI.Data
{
  // 数据库上下文：负责与SQLite数据库交互
  public class ModbusDbContext : DbContext
  {
    public ModbusDbContext(DbContextOptions<ModbusDbContext> options) : base(options)
    {
    }

    // DbSet表示数据库中的RegisterData表
    public DbSet<RegisterData> RegisterData { get; set; }

    // 配置实体模型和表结构
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<RegisterData>(entity =>
      {
        entity.HasKey(e => e.Id); // 设置主键
        entity.Property(e => e.Address).IsRequired(); // 设置必需字段
        entity.Property(e => e.Value).IsRequired();
        entity.Property(e => e.Time).IsRequired();
      });
    }
  }
}