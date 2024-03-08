using SimchaFundHw.Data;

namespace SimchaFundHw.Web.Models
{
    public class IndexViewModel
    {
        public int TotalPeopleInDataBaseCount { get; set; }
        public List<Simcha> Simchas { get; set; }
    }
}
