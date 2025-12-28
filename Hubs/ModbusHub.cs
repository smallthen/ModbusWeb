using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ModbusIndustrialAPI.Hubs
{
  public class ModbusHub : Hub
  {
    public async Task JoinGroup(string groupName)
    {
      await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
      await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SendDataToAll(string data)
    {
      await Clients.All.SendAsync("ReceiveData", data);
    }

    public async Task SendDataToGroup(string groupName, string data)
    {
      await Clients.Group(groupName).SendAsync("ReceiveData", data);
    }
  }
}