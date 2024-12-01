using AlgoTradingKiteApi.Models;
using AlgoTradingKiteApi.Services;
using KiteConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using System.Security.Policy;
using System.Text.Json;
using WebSocketSharp;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AlgoTradingKiteApi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly KiteService _kiteService;
        private readonly WebSocketService _webSocketService;
        private static Timer _timer;
        private static bool _exitConditionMet = false;
        private static string _accessToken;
        private static string _userId;
        private static Profile _user;
        private static List<OptionData> _instruments;

        private static List<TickModelView> tickList = new List<TickModelView>();
        
        public HomeController(ILogger<HomeController> logger,KiteService kiteService,
            WebSocketService webSocketService)
        {
            _logger = logger;
            _kiteService = kiteService;
            _webSocketService = webSocketService;
        }

        public void SetDefaultValueTickList()
        {
            if (tickList != null && tickList.Count > 0)
                return;

            tickList = new List<TickModelView>();

            
        }

        public IActionResult Index()
        {
             

            if(_user.Email == null)
            {
                return View();
            }
            else
            {
                 return RedirectToAction("MarketData");
            }

        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Route for login
        public IActionResult Login()
        {
            var loginUrl = _kiteService.GetLoginUrl();
            return Redirect(loginUrl); // Redirect to Kite's login page
        }

        // Callback from Kite after user login
        public IActionResult Callback(string request_token)
        {
           
            _accessToken = "";
            tickList=new List<TickModelView>();
            var session = _kiteService.Login(request_token);
            _accessToken= session.AccessToken;
            _userId = session.UserId;
            _user = _kiteService.GetProfile(_accessToken);
            return RedirectToAction("Dashboard");
        }

        // Display user profile after login
        public  IActionResult Dashboard()
        {
            var accessToken = _accessToken;//  HttpContext.Session.GetString("AccessToken");
            if (accessToken == null) return RedirectToAction("Login");

         

            if(_instruments == null  || _instruments.Count ==0)
            {
              
                _timer = new Timer(TimerCallback, null, 0, 1000);
            }

            DashboardModelView dashboardModelView = new DashboardModelView()
            {
                InstrumentList = _instruments == null ? new List<OptionData>():_instruments,
                TickList = tickList
            };
            return View(dashboardModelView); // Return profile details to view
        }

        private async void TimerCallback(object state)
        {
            var currentTime = DateTime.UtcNow;
            var currentISTTime=currentTime.AddHours(5).AddMinutes(30);
            if (currentISTTime.Hour == 13 && currentISTTime.Minute == 49 && currentISTTime.Second==10)
            {
                // This is where we call GetInstruments periodically
                
                _timer?.DisposeAsync();
             await  GetInstruments();

            }
           
        }

        private async Task<int> GetInstruments()
        {
            var accessToken = _accessToken;//HttpContext.Session.GetString("AccessToken");
            
            _instruments = await _webSocketService.GetInstrumentTokens(accessToken);


            // Call the POST action method manually (without form submission)
            return 0;

        }
        [HttpPost]
        public DashboardModelView Dashboard([FromBody] DashboardModelView dashboardModelView)
        {
            if (dashboardModelView == null)
            {
                return new DashboardModelView();
            }

           
            if (tickList == null || tickList.Count == 0)
            {
                SetDefaultValueTickList();
            }

            var curTickExists = tickList.Where(t => t.Tick.InstrumentToken == dashboardModelView.curTick.InstrumentToken).FirstOrDefault();

            if(curTickExists!= null)
            {
                foreach (var tick in tickList)
                {
                    if (tick.Tick.InstrumentToken == dashboardModelView.curTick.InstrumentToken)
                    {
                        tick.Tick = dashboardModelView.curTick;
                        tick.change = dashboardModelView.curTick.LastPrice - tick.benchPrice;
                        break;
                    }
                }

                for (var i = 0; i < tickList.Count; i++)
                {
                    tickList[i].netChange = 0;
                    foreach (var tick in tickList)
                    {
                        tickList[i].netChange += tick.change;
                    }
                }

            }
            else
            {
                tickList.Add(new TickModelView()
                {
                    Tick = dashboardModelView.curTick,
                    benchPrice = dashboardModelView.curTick.LastPrice,
                    change = 0,
                    netChange = 0,
                    status = "Waiting",
                    InstrumentToken = dashboardModelView.curTick.InstrumentToken
                });
            }

            


            dashboardModelView.InstrumentList = _instruments;
            dashboardModelView.TickList = tickList;
            return dashboardModelView;
        }

        // Get market data
        public IActionResult MarketData()
        {  
            if (_accessToken == null) return RedirectToAction("Login");

        
            return View(); // Return market data to view
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
