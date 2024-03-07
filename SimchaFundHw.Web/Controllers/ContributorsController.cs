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
            ContributorsViewModel vm = new() { People = mgr.GetPeopleInfo() };

            return View(vm);
        }

        [HttpPost] 
        public IActionResult NewPerson(Person person, decimal ammount, string phoneNumber)
        {
            Manager mgr = new(_connection);
            var p = person;
            p.PhoneNumber = phoneNumber;
            int personId = mgr.AddPerson(p);
            Deposit initialDeposit = new() { Ammount=ammount, PersonId=personId, Date=person.CreatedDate};
            
            mgr.AddDeposit(initialDeposit);
            return Redirect("/Contributors");
        }
    }
}
