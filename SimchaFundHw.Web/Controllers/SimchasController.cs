using Microsoft.AspNetCore.Mvc;
using SimchaFundHw.Data;
using SimchaFundHw.Web.Models;

namespace SimchaFundHw.Web.Controllers
{
    public class SimchasController : Controller
    {
        private readonly string _connection = @"Data Source=.\sqlexpress; Initial Catalog=SimchaFund; Integrated Security=true";

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult New(DateTime date, string name)
        {
            Manager mgr = new(_connection);
            mgr.AddSimcha(date, name);
            return Redirect("/Home");
        }

        public IActionResult Contributions(int simchaId)
        {
            Manager mgr = new(_connection);
            var vm = new ContributionsViewModel { Contributors=mgr.GetContributionsById(simchaId)};
            return View(vm);
        }
    }
}
