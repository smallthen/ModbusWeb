namespace ModbusIndustrialAPI.Models.Entities
{
  // 数据库实体：存储Modbus寄存器数据
  public class RegisterData
  {
    public int Id { get; set; }      // 主键
    public int Address { get; set; } // 寄存器地址
    public int Value { get; set; }   // 寄存器值
    public DateTime Time { get; set; } // 采集时间
  }
}