using Microsoft.AspNetCore.SignalR;

namespace VodenkoWeb.Hubs
{
    public class LogHub : Hub
    {
        public const string Url = "/logHub";

        public async Task SendLogMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveLogMessage", message);
        }
    }
}
