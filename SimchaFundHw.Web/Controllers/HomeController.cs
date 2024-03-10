using Microsoft.AspNetCore.Mvc;
using SimchaFundHw.Data;
using SimchaFundHw.Web.Models;
using System.Diagnostics;

namespace SimchaFundHw.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connection = @"Data Source=.\sqlexpress; Initial Catalog=SimchaFund; Integrated Security=true";


        public IActionResult Index()
        {
            Manager mgr = new(_connection);
            var vm = new IndexViewModel {Simchas=mgr.GetAllSimchas(),
                TotalPeopleInDataBaseCount=mgr.GetAmmountOfPeopleInDB() };
            return View(vm);
        }

        public IActionResult Contributions(int simchaId)
        {
            return View();
        }

    }
}