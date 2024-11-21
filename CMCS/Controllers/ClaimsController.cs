using CMCS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMCS.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

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

    //Part 3
    [HttpPost]
    public async Task<IActionResult> SubmitClaim(Claim claim, IFormFile document)
    {
        // Validate and process the uploaded document
        if (document != null && document.Length > 0)
        {
            // Check the file extension to ensure only allowed types are uploaded
            var fileExtension = Path.GetExtension(document.FileName).ToLower();
            if (!_allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("document", "Invalid file type. Only PDF, DOCX, and XLSX files are allowed.");
                return View(claim); // Return the view with validation error
            }

            // Define the directory path for storing uploaded files
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath); // Create the directory if it doesn't exist
            }

            // Save the uploaded file to the specified directory
            var filePath = Path.Combine(uploadsPath, document.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await document.CopyToAsync(stream); // Asynchronously copy the file content
            }

            // Set the path of the uploaded document in the claim
            claim.DocumentPath = $"/uploads/{document.FileName}";
        }
        else
        {
            // Add a validation error if no document is uploaded
            ModelState.AddModelError("document", "Please upload a supporting document.");
            return View(claim);
        }

        // Define a salary limit for automatic claim rejection
        const decimal salaryLimit = 5000m; // Example limit
        if (claim.TotalSalary > salaryLimit)
        {
            claim.Status = "Rejected"; // Mark the claim as rejected
            claim.Notes = "Claim rejected due to exceeding salary limits."; // Add a note explaining the rejection
        }

        // Set the LecturerId to the current user's unique identifier
        claim.LecturerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // Add the claim to the database context for saving
        _dbContext.Claims.Add(claim);
        await _dbContext.SaveChangesAsync(); // Persist changes to the database

        // Redirect to the ClaimSubmitted confirmation view
        return RedirectToAction("ClaimSubmitted");
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

    //Part 3
    [Authorize(Roles = "Lecturer")] // Ensures only Lecturers can access this view
    [HttpGet]
    public async Task<IActionResult> TrackClaims()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value; // Get logged-in user ID
        var myClaims = await _dbContext.Claims
            .Where(c => c.LecturerId == userId)
            .ToListAsync();

        return View(myClaims); // Display only claims submitted by the logged-in user
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

    //Part 3
    [Authorize(Roles = "HR,Manager")] // Restrict access to HR and Manager roles
    [HttpGet]
    public async Task<IActionResult> HRViewClaims()
    {
        var allClaims = await _dbContext.Claims.ToListAsync(); // Fetch all claims
        return View(allClaims); // Display all claims
    }

    [Authorize(Roles = "HR,Manager")] // Restrict access to HR and Manager roles
    [HttpGet]
    public async Task<IActionResult> GenerateApprovedClaimsReport()
    {
        var approvedClaims = await _dbContext.Claims
            .Where(c => c.Status == "Approved")
            .ToListAsync();

        // Simulating report generation as a CSV
        var csvData = "ClaimId, LecturerName, HoursWorked, HourlyRate, TotalSalary, SubmissionDate\n";
        foreach (var claim in approvedClaims)
        {
            csvData += $"{claim.ClaimId}, {claim.LecturerName}, {claim.HoursWorked}, {claim.HourlyRate}, {claim.TotalSalary}, {claim.SubmissionDate}\n";
        }

        var reportPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/reports", "ApprovedClaimsReport.csv");
        if (!Directory.Exists(Path.GetDirectoryName(reportPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
        }

        await System.IO.File.WriteAllTextAsync(reportPath, csvData); // Save the CSV file

        TempData["ReportPath"] = "/reports/ApprovedClaimsReport.csv"; // Save report path for download
        return View("HRViewClaims"); // Redirect to HR view
    }

    // HR View: Displays all claims and generates a report of approved claims
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> HRView()
    {
        var allClaims = await _dbContext.Claims.ToListAsync();
        return View(allClaims);
    }

    [Authorize(Roles = "HR")]
    public async Task<IActionResult> DownloadReport()
    {
        var approvedClaims = await _dbContext.Claims
            .Where(c => c.Status == "Approved")
            .ToListAsync();

        var csvBuilder = new StringBuilder();
        csvBuilder.AppendLine("ClaimId, LecturerName, HoursWorked, HourlyRate, TotalSalary, SubmissionDate");

        foreach (var claim in approvedClaims)
        {
            csvBuilder.AppendLine($"{claim.ClaimId}, {claim.LecturerName}, {claim.HoursWorked}, {claim.HourlyRate}, {claim.TotalSalary}, {claim.SubmissionDate.ToString("yyyy-MM-dd")}");
        }

        var reportBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
        return File(reportBytes, "text/csv", "ApprovedClaimsReport.csv");
    }
    //Digital TechJoint (2022). ASP.NET Identity - User Registration, Login and Log-out. [online] YouTube. Available at: https://www.youtube.com/watch?v=ghzvSROMo_M [Accessed 9 Oct. 2024].
    //Digital TechJoint (2022). ASP.NET MVC - How To Implement Role Based Authorization. YouTube. Available at: https://www.youtube.com/watch?v=qvsWwwq2ynE [Accessed 10 Oct. 2024].
}