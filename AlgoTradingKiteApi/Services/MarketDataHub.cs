using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace AlgoTradingKiteApi.Services
{
    public class MarketDataHub : Hub
    {
        public async Task SendMarketData(string marketData)
        {
            await Clients.All.SendAsync("ReceiveMarketData", marketData);
        }
    }
}
