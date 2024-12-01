namespace AlgoTradingKiteApi.Models
{
    public class OptionData
    {
       
        public string instrument_type { get; set; }

        public string name { get; set; }

        public string last_price { get; set; }

        public string exchange { get; set; }
        public string segment { get; set; }
        public string tick_size { get; set; }

        public string lot_size { get; set; }
        public string expiry { get; set; }

        public string strike { get; set; }
        public string tradingsymbol { get; set; }
        public string exchange_token { get; set; }
        public string instrument_token { get; set; }
    }

}
