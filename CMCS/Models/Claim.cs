using System.ComponentModel.DataAnnotations; 

namespace CMCS.Models
{
    public class Claim
    {
        // Primary key for the Claim entity
        [Key]
        public int ClaimId { get; set; }

        // The name of the lecturer submitting the claim (required field)
        [Required]
        public string LecturerName { get; set; }

        // The number of hours worked by the lecturer (required field)
        [Required]
        public decimal HoursWorked { get; set; }

        // The hourly rate for the lecturer (required field)
        [Required]
        public decimal HourlyRate { get; set; }

        // Status of the claim (e.g., Pending, Approved, Rejected). Defaults to "Pending".
        [Required]
        public string Status { get; set; } = "Pending";

        // The date when the claim was submitted (required field)
        [Required]
        public DateTime SubmissionDate { get; set; }

        // Optional notes for the claim, such as reasons for approval or rejection
        public string Notes { get; set; }

        // The file path to the uploaded supporting document (required field)
        [Required]
        public string DocumentPath { get; set; }

        // Added: A required field to associate the claim with the submitting lecturer's unique ID
        [Required]
        public string LecturerId { get; set; }

        // A computed property that calculates the total salary for the claim
        // This is derived by multiplying HoursWorked by HourlyRate
        public decimal TotalSalary => HoursWorked * HourlyRate;
    }
    //Digital TechJoint (2022). ASP.NET Identity - User Registration, Login and Log-out. [online] YouTube. Available at: https://www.youtube.com/watch?v=ghzvSROMo_M [Accessed 9 Oct. 2024].
    //Digital TechJoint (2022). ASP.NET MVC - How To Implement Role Based Authorization. YouTube. Available at: https://www.youtube.com/watch?v=qvsWwwq2ynE [Accessed 10 Oct. 2024].
}
