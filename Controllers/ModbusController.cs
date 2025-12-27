using Microsoft.AspNetCore.Mvc;
using ModbusIndustrialAPI.Services;
using ModbusIndustrialAPI.Models.DTOs;

namespace ModbusIndustrialAPI.Controllers
{
  // Modbus控制器：处理Modbus相关的HTTP请求
  [ApiController]
  [Route("api/[controller]")]
  public class ModbusController : ControllerBase
  {
    private readonly IModbusService _modbusService;

    public ModbusController(IModbusService modbusService)
    {
      _modbusService = modbusService;
    }

    // GET: /api/modbus - 获取Modbus数据
    [HttpGet]
    public ActionResult<List<RegisterDto>> Get()
    {
      return Ok(_modbusService.GetRegisterData());
    }

    // GET: /api/modbus/page - 返回HTML页面展示数据
    [HttpGet("page")]
    public ActionResult GetPage()
    {
      var data = _modbusService.GetRegisterData();
      string html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <title>Modbus数据展示</title>
    <style>
        body {{ font-family: Arial; margin: 20px; }}
        table {{ width: 500px; margin: 0 auto; border-collapse: collapse; }}
        th, td {{ border: 1px solid #ccc; padding: 8px; text-align: center; }}
        th {{ background: #f0f0f0; }}
        h3 {{ text-align: center; }}
    </style>
</head>
<body>
    <h3>Modbus工业数据采集展示</h3>
    <table>
        <tr>
            <th>寄存器地址</th>
            <th>数值</th>
            <th>采集时间</th>
        </tr>
        {string.Join("", data.Select(item => $@"
        <tr>
            <td>{item.Address}</td>
            <td>{item.Value}</td>
            <td>{item.Time}</td>
        </tr>"))}
    </table>
</body>
</html>";
      return Content(html, "text/html");
    }
  }
}