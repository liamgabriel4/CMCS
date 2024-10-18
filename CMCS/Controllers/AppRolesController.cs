using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Identity; 

namespace CMCS.Controllers
{
    // Apply authorization to restrict access to users with "Coordinator" or "Manager" roles
    [Authorize(Roles = "Coordinator,Manager")]
    public class AppRolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager; // Role manager to handle role operations

        // Constructor to inject the RoleManager dependency
        public AppRolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager; // Assign the injected RoleManager to the private field
        }

        // Action to list all roles created by users
        public IActionResult Index()
        {
            var roles = _roleManager.Roles; // Retrieve the list of roles from the role manager
            return View(roles); // Pass the roles to the view for display
        }

        // GET action to display the role creation form
        [HttpGet]
        public IActionResult Create()
        {
            return View(); // Return the view for creating a new role
        }

        // POST action to handle the role creation form submission
        [HttpPost]
        public async Task<IActionResult> Create(IdentityRole model)
        {
            // Check if the role already exists to avoid duplicate roles
            if (!_roleManager.RoleExistsAsync(model.Name).GetAwaiter().GetResult())
            {
                // Create a new role using the provided model name
                await _roleManager.CreateAsync(new IdentityRole(model.Name));
            }

            return RedirectToAction("Index"); // Redirect to the Index action after creating the role
        }
    }
    //Digital TechJoint (2022). ASP.NET Identity - User Registration, Login and Log-out. [online] YouTube. Available at: https://www.youtube.com/watch?v=ghzvSROMo_M [Accessed 9 Oct. 2024].
    //Digital TechJoint (2022). ASP.NET MVC - How To Implement Role Based Authorization. YouTube. Available at: https://www.youtube.com/watch?v=qvsWwwq2ynE [Accessed 10 Oct. 2024].
}
