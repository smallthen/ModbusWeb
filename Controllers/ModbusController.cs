using Microsoft.AspNetCore.Mvc;
using ModbusIndustrialAPI.Services;
using ModbusIndustrialAPI.Models.DTOs;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore;

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
        .chart-container {{ width: 800px; height: 400px; margin: 20px auto; }}
        .content-container {{ display: flex; flex-direction: column; align-items: center; }}
    </style>
</head>
<body>
    <h3>Modbus工业数据采集展示</h3>

    <div class='chart-container'>
        <h4>实时数据图表</h4>
        <canvas id='modbusChart'></canvas>
    </div>

    <div class='content-container'>
        <h4>实时数据表格</h4>
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
    </div>

    <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
    <script>
        // 准备图表数据
        const addresses = [{string.Join(",", data.Select(item => $"'{item.Address}'"))}];
        const values = [{string.Join(",", data.Select(item => item.Value))}];

        // 创建图表
        const ctx = document.getElementById('modbusChart').getContext('2d');
        const chart = new Chart(ctx, {{
            type: 'line',
            data: {{
                labels: addresses,
                datasets: [{{
                    label: '寄存器值',
                    data: values,
                    borderColor: 'rgb(75, 192, 192)',
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    tension: 0.1
                }}]
            }},
            options: {{
                responsive: true,
                maintainAspectRatio: false,
                scales: {{
                    y: {{
                        beginAtZero: false
                    }}
                }}
            }}
        }});
    </script>
</body>
</html>";
      return Content(html, "text/html");
    }

    // GET: /api/modbus/chart - 返回图表数据API
    [HttpGet("chart")]
    public ActionResult<object> GetChartData()
    {
      var data = _modbusService.GetRegisterData();

      var chartData = new
      {
        Labels = data.Select(d => d.Address.ToString()).ToArray(),
        Values = data.Select(d => d.Value).ToArray(),
        Times = data.Select(d => d.Time).ToArray()
      };

      return Ok(chartData);
    }
  }
}