using CMCS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMCS.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class ClaimsController : Controller
{
    private readonly ApplicationDbContext _dbContext; // Database context for accessing claims data
    private readonly long _maxFileSize = 5 * 1024 * 1024; // Maximum file size limit (5 MB)
    private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".xlsx" }; // Allowed file extensions for uploaded documents

    // Constructor that initializes the claims controller with the database context
    public ClaimsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // GET: Display the form for submitting a claim
    [Authorize] // Requires user to be authenticated
    [HttpGet]
    public IActionResult SubmitClaim()
    {
        return View(); // Returns the view for submitting a claim
    }

    // POST: Handle the submission of a claim
    [HttpPost]
    public async Task<IActionResult> SubmitClaim(Claim claim, IFormFile document)
    {
        // Check if a document has been uploaded
        if (document != null && document.Length > 0)
        {
            var fileExtension = Path.GetExtension(document.FileName).ToLower(); // Get the file extension
            // Validate the file extension
            if (!_allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("document", "Invalid file type. Only PDF, DOCX, and XLSX files are allowed.");
                return View(claim); // Return to the view with an error message
            }

            // Define the path for uploads
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            // Create the uploads directory if it doesn't exist
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Define the file path where the document will be saved
            var filePath = Path.Combine(uploadsPath, document.FileName);
            // Save the uploaded document to the file system
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await document.CopyToAsync(stream); // Asynchronously copy the file to the specified path
            }

            // Store the document path in the claim object
            claim.DocumentPath = $"/uploads/{document.FileName}";
        }
        else
        {
            ModelState.AddModelError("document", "Please upload a supporting document."); // Error if no document was uploaded
            return View(claim); // Return to the view with an error message
        }

        // Add the claim to the database and save changes
        _dbContext.Claims.Add(claim);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction("ClaimSubmitted"); // Redirect to the confirmation page after submission
    }

    // GET: Display the claim submission confirmation page
    public IActionResult ClaimSubmitted()
    {
        return View(); // Returns the view for successful claim submission
    }

    // GET: View all pending claims for approval
    [Authorize(Roles = "Coordinator,Manager")] // Requires the user to have the specified roles
    [HttpGet]
    public async Task<IActionResult> ViewPendingClaims()
    {
        try
        {
            // Retrieve all claims with "Pending" status from the database
            var pendingClaims = await _dbContext.Claims.Where(c => c.Status == "Pending").ToListAsync();
            return View(pendingClaims); // Return the view with the list of pending claims
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while fetching the claims. Please try again later.");
            Console.WriteLine(ex.Message); // Log the error message
            return View("Error"); // Return an error view if something goes wrong
        }
    }

    // POST: Approve a claim based on its ID
    [Authorize(Roles = "Co-ordinator,Manager")] // Requires the user to have the specified roles
    [HttpPost]
    public async Task<IActionResult> ApproveClaim(int id)
    {
        try
        {
            // Find the claim by its ID
            var claim = await _dbContext.Claims.FindAsync(id);
            if (claim != null)
            {
                claim.Status = "Approved"; // Update the status of the claim to "Approved"
                await _dbContext.SaveChangesAsync(); // Save changes to the database
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Claim not found."); // Error if the claim is not found
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while approving the claim. Please try again.");
            Console.WriteLine(ex.Message); // Log the error message
        }
        return RedirectToAction("ViewPendingClaims"); // Redirect back to the pending claims view
    }

    // POST: Reject a claim based on its ID
    [Authorize(Roles = "Co-ordinator,Manager")] // Requires the user to have the specified roles
    [HttpPost]
    public async Task<IActionResult> RejectClaim(int id)
    {
        try
        {
            // Find the claim by its ID
            var claim = await _dbContext.Claims.FindAsync(id);
            if (claim != null)
            {
                claim.Status = "Rejected"; // Update the status of the claim to "Rejected"
                await _dbContext.SaveChangesAsync(); // Save changes to the database
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Claim not found."); // Error if the claim is not found
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while rejecting the claim. Please try again.");
            Console.WriteLine(ex.Message); // Log the error message
        }
        return RedirectToAction("ViewPendingClaims"); // Redirect back to the pending claims view
    }

    // GET: Track all claims (for all users with specific roles)
    [Authorize(Roles = "Co-ordinator,Manager,Lecturer")] // Requires the user to have one of the specified roles
    [HttpGet]
    public async Task<IActionResult> TrackClaims()
    {
        try
        {
            // Retrieve all claims from the database
            var allClaims = await _dbContext.Claims.ToListAsync();
            return View(allClaims); // Return the view with the list of all claims
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while fetching the claims. Please try again later.");
            Console.WriteLine(ex.Message); // Log the error message
            return View("Error"); // Return an error view if something goes wrong
        }
    }

    // POST: Delete a claim based on its ID
    [Authorize(Roles = "Co-ordinator,Manager")] // Requires the user to have the specified roles
    [HttpPost]
    public async Task<IActionResult> DeleteClaim(int id)
    {
        var claim = await _dbContext.Claims.FindAsync(id); // Find the claim by its ID
        if (claim != null)
        {
            _dbContext.Claims.Remove(claim); // Remove the claim from the database
            await _dbContext.SaveChangesAsync(); // Save changes to the database
            return RedirectToAction("TrackClaims"); // Redirect back to the track claims view
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Claim not found."); // Error if the claim is not found
            return View("Error"); // Return an error view if the claim is not found
        }
    }
    //Digital TechJoint (2022). ASP.NET Identity - User Registration, Login and Log-out. [online] YouTube. Available at: https://www.youtube.com/watch?v=ghzvSROMo_M [Accessed 9 Oct. 2024].
    //Digital TechJoint (2022). ASP.NET MVC - How To Implement Role Based Authorization. YouTube. Available at: https://www.youtube.com/watch?v=qvsWwwq2ynE [Accessed 10 Oct. 2024].
}
