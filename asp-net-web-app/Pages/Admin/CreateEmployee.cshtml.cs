using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using asp_net_web_app.Data;
using asp_net_web_app.Services;

namespace asp_net_web_app.Pages
{
    public class CreateModel : PageModel
    {

        private readonly CreateEmployeeLogic _employeeLogic;

        public CreateModel(CreateEmployeeLogic employeeLogic)
        {
            _employeeLogic = employeeLogic;
        }

        [BindProperty]
        public Employee Employee { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            _employeeLogic.CreateEmployee(Employee);

            return RedirectToPage("/Index");
        }
    }
}