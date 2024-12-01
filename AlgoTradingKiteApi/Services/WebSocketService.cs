using System;
using System.Security.Policy;
using System.Threading.Tasks;
using AlgoTradingKiteApi.Models;
using CsvHelper;
using KiteConnect;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using WebSocketSharp;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static AlgoTradingKiteApi.Services.WebSocketService;

namespace AlgoTradingKiteApi.Services
{
    public class WebSocketService
    {
        private readonly IHubContext<MarketDataHub> _hubContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private WebSocket _ws;
        private static  string _accessToken;
        private readonly string _userId;
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly string _redirectUrl;
        private Kite _kite;

        public WebSocketService(IHubContext<MarketDataHub> hubContext,IHttpContextAccessor httpContextAccessor,IConfiguration configuration)
        {
            _hubContext = hubContext;
            _apiKey = configuration["KiteConnect:ApiKey"]?? "";
            _httpContextAccessor= httpContextAccessor;
            _apiSecret = configuration["KiteConnect:ApiSecret"] ?? "";
            _redirectUrl = configuration["KiteConnect:RedirectUrl"] ?? "";
            _kite = new Kite(_apiKey);
            
        }

        public void StartWebSocket(List<int> instruments)
        {

            // Create an instance of KiteTicker
            var kiteTicker = new Ticker(_apiKey, _accessToken);

            // Attach event handlers for WebSocket events
            kiteTicker.OnTick += OnTick;         // When new ticks are received
            kiteTicker.OnConnect += OnConnect;    // When WebSocket connection is established
            kiteTicker.OnClose += OnClose;         // When WebSocket connection is closed
            kiteTicker.OnError += OnError;         // When error occurs
            kiteTicker.OnReconnect += OnReconnect; // When WebSocket reconnects

            

            // Subscribe to instruments
            kiteTicker.Subscribe(Tokens: instruments.Select(x => Convert.ToUInt32(x)).ToArray());

           // Set mode to full for more data
            // Connect to the WebSocket server
            kiteTicker.Connect();

        }

        private static void OnTokenExpire()
        {
            Console.WriteLine("Need to login again");
        }

        private static void OnConnect()
        {
          
            Console.WriteLine("Connected ticker");
        }

        private static void OnClose()
        {
            Console.WriteLine("Closed ticker");
        }

        private static void OnError(string Message)
        {
            Console.WriteLine("Error: " + Message);
        }

        private static void OnNoReconnect()
        {
            Console.WriteLine("Not reconnecting");
        }

        private static void OnReconnect()
        {
            Console.WriteLine("Reconnecting");
        }

        private  async void OnTick(Tick TickData)
        {
            var tickJson = JsonConvert.SerializeObject(TickData);
            await _hubContext.Clients.All.SendAsync("ReceiveMarketData", tickJson);
            
        }

        private static void OnOrderUpdate(Order OrderData)
        {
            Console.WriteLine("OrderUpdate " + Utils.JsonSerialize(OrderData));
        }

        public async Task<List<OptionData>> GetInstrumentTokens(string accessToken)
        {
            _accessToken = accessToken;
            // Assuming you are using HttpClient for Kite API requests
            HttpClient client = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(3)  // Set timeout to 30 seconds
            };

            // If you're making the request directly:
            var response = await client.GetAsync("https://api.kite.trade/instruments");

            response.EnsureSuccessStatusCode();
            // Read the response content
            var content = await response.Content.ReadAsStringAsync();

            var deserializer = new CSVHelper();
            var instruments = deserializer.DeserializeCsv(content);

            
            instruments= instruments.Where(x => x.name.ToLower().Equals("nifty") &&  !string.IsNullOrEmpty(x.expiry)).OrderBy(x => x.expiry).ToList();
            var distinctExpiryDates = instruments.Select(x => x.expiry).Distinct().Take(2).LastOrDefault();

            var atmStrick =await getAtmStrickPrice();
            instruments = distinctExpiryDates != null ? instruments.Where(x => x.expiry == distinctExpiryDates && (Convert.ToInt32(x.strike) == atmStrick)).OrderBy(x => Convert.ToInt32(x.strike)).ToList() : instruments;

            var intrumentId=instruments.Select(x =>Convert.ToInt32(x.instrument_token)).Distinct().ToList();
            StartWebSocket(intrumentId);
            return instruments;
        }

        public async Task<int> getAtmStrickPrice()
        {
            HttpClient client = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(3)  // Set timeout to 30 seconds
            };

            // If you're making the request directly:
            var response = await client.GetAsync("https://api.kite.trade/instruments");

            response.EnsureSuccessStatusCode();
            // Read the response content
            var content = await response.Content.ReadAsStringAsync();

            var deserializer = new CSVHelper();
            var instruments = deserializer.DeserializeCsv(content);
            var curniftyInstrument = instruments.Where(x => x.name.ToLower().Equals("nifty 50") && string.IsNullOrEmpty(x.expiry)).FirstOrDefault();
            var instrument_token = curniftyInstrument.instrument_token;
            string[] InstrumentId = new string[300];
            InstrumentId[0] = instrument_token.ToString();
            _kite.SetAccessToken(_accessToken ?? "");
            var market_price_data = _kite.GetLTP(InstrumentId);
           var atmStrick = market_price_data[instrument_token.ToString()].LastPrice;
            var atmStrickRound =Math.Round(atmStrick / 50) * 50;
            return (int)atmStrickRound;
        }

        // Instrument class for parsing the instruments API response
        public class Instrument
        {
            public string tradingsymbol { get; set; }
            public string exchange_token { get; set; }
            public int instrument_token { get; set; }
        }

        // Define a class to match the incoming tick data (simplified example)
        public class TickData
        {
            public int InstrumentToken { get; set; }
            public double LastPrice { get; set; }
        }
    }


}
