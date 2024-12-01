using KiteConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;

namespace AlgoTradingKiteApi.Services
{  
    public class KiteService
    {
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly string _redirectUrl;
        private Kite _kite;


        public KiteService(IConfiguration configuration)
        {
            _apiKey = configuration["KiteConnect:ApiKey"]?? "";
            _apiSecret = configuration["KiteConnect:ApiSecret"]?? "";
            _redirectUrl = configuration["KiteConnect:RedirectUrl"]?? "";
            _kite = new Kite(_apiKey);
        }

        public string GetLoginUrl()
        {
            // Generate login URL for user to authorize the app
            return _kite.GetLoginURL();
        }

        public KiteConnect.User Login(string requestToken)
        {
            // Login with request token after user authorization
            
            var session= _kite.GenerateSession(requestToken, _apiSecret);
            return session;
        }

        public Profile GetProfile(string accessToken)
        {
            // Get user profile details
            _kite.SetAccessToken(accessToken);
            return _kite.GetProfile();
        }

        public Dictionary<string, Quote> GetMarketData(string accessToken, string[] instruments)
        {
            // Get market data for instruments (e.g., stock prices)
            _kite.SetAccessToken(accessToken);
            return _kite.GetQuote(instruments);
        }

      
    }
}
