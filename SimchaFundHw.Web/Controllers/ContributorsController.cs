using Microsoft.AspNetCore.Mvc;
using SimchaFundHw.Data;
using SimchaFundHw.Web.Models;

namespace SimchaFundHw.Web.Controllers
{
    public class ContributorsController : Controller
    {
        private readonly string _connection = @"Data Source=.\sqlexpress; Initial Catalog=SimchaFund; Integrated Security=true";


        public IActionResult Index()
        {
            Manager mgr = new(_connection);
            ContributorsViewModel vm = new() { People = mgr.GetPeopleInfo(),
                TotalBalance=mgr.GetTotalBalanceForFund() };

            return View(vm);
        }

        [HttpPost]
        public IActionResult NewPerson(Person person, decimal amount, string phoneNumber)
        {
            Manager mgr = new(_connection);
            var p = person;
            p.PhoneNumber = phoneNumber;
            int personId = mgr.AddPerson(p);
            Deposit initialDeposit = new() { Amount = amount, PersonId = personId, Date = person.CreatedDate };

            mgr.AddDeposit(initialDeposit);
            return Redirect("/Contributors");
        }

        public IActionResult History(int id)
        {
            var mgr = new Manager(_connection);
            var vm = new HistoryViewModel
            {
                Name = mgr.GetFullNameForPersonId(id),
                Balance=mgr.GetTotalBalanceForPerson(id),
                Events = mgr.GetHistoryByPersonId(id)
            };
            return View(vm);
        }


        [HttpPost]
        public IActionResult Deposit(Deposit deposit)
        {
            var mgr = new Manager(_connection);
            mgr.AddDeposit(deposit);
            return Redirect("/Contributors");
        }
    }
}
