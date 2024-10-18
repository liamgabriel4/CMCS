using System.ComponentModel.DataAnnotations; 

namespace CMCS.Models
{
    public class Claim // Represents a claim made by a lecturer
    {
        [Key] // Indicates that ClaimId is the primary key for this entity
        public int ClaimId { get; set; } // Unique identifier for each claim

        [Required] // Indicates that LecturerName is a required field
        public string LecturerName { get; set; } // Name of the lecturer making the claim

        [Required] // Indicates that HoursWorked is a required field
        public decimal HoursWorked { get; set; } // Number of hours worked by the lecturer

        [Required] // Indicates that HourlyRate is a required field
        public decimal HourlyRate { get; set; } // Hourly rate of the lecturer

        [Required] // Indicates that Status is a required field
        public string Status { get; set; } = "Pending"; // Status of the claim, default is "Pending"

        [Required] // Indicates that SubmissionDate is a required field
        public DateTime SubmissionDate { get; set; } // Date when the claim was submitted

        public string Notes { get; set; } // Additional notes related to the claim

        [Required] // Indicates that DocumentPath is a required field
        public string DocumentPath { get; set; } // Path to the uploaded document for the claim
    }
    //Digital TechJoint (2022). ASP.NET Identity - User Registration, Login and Log-out. [online] YouTube. Available at: https://www.youtube.com/watch?v=ghzvSROMo_M [Accessed 9 Oct. 2024].
    //Digital TechJoint (2022). ASP.NET MVC - How To Implement Role Based Authorization. YouTube. Available at: https://www.youtube.com/watch?v=qvsWwwq2ynE [Accessed 10 Oct. 2024].
}
