using Microsoft.AspNetCore.SignalR;

namespace VodenkoWeb.Hubs
{
    public class CacheHub:Hub
    {

        public const string Url = "/cache";

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
