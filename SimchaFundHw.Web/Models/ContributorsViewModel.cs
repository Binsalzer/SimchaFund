using SimchaFundHw.Data;

namespace SimchaFundHw.Web.Models
{
    public class ContributorsViewModel
    {
        public decimal TotalBalance { get; set; }
        public List<Person> People { get; set; }
    }
}
