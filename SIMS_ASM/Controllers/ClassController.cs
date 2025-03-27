using Microsoft.AspNetCore.Mvc;

namespace SIMS_ASM.Controllers
{
    public class ClassController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
