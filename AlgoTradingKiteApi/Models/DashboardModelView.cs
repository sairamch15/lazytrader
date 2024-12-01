using KiteConnect;

namespace AlgoTradingKiteApi.Models
{
    public class DashboardModelView
    {
        public List<OptionData> InstrumentList { get; set; }  
        
        public List<TickModelView> TickList { get; set; }

        public Tick curTick { get; set; }   
    }

    public class TickModelView
    {
        public KiteConnect.Tick Tick { get; set; }

        public decimal benchPrice { get; set; }

        public decimal change { get; set; }

        public decimal netChange { get; set; }

        public string status { get; set; }

        public uint InstrumentToken { get; set; }
    }
}
