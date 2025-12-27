namespace ModbusIndustrialAPI.Models.DTOs
{
  // 数据传输对象：用于API传输
  public class RegisterDto
  {
    public int Address { get; set; } // 寄存器地址
    public int Value { get; set; }   // 寄存器值
    public string Time { get; set; } = string.Empty; // 采集时间（字符串格式）
  }
}