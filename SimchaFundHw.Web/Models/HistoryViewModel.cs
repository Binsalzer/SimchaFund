using SimchaFundHw.Data;

namespace SimchaFundHw.Web.Models
{
    public class HistoryViewModel
    {
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public List<HistoryEvent> Events { get; set; }
    }
}
