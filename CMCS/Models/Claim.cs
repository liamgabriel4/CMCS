using System.ComponentModel.DataAnnotations; 

namespace CMCS.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        [Required]
        public string LecturerName { get; set; }

        [Required]
        public decimal HoursWorked { get; set; }

        [Required]
        public decimal HourlyRate { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        [Required]
        public DateTime SubmissionDate { get; set; }

        public string Notes { get; set; }

        [Required]
        public string DocumentPath { get; set; }

        [Required] //Added: To associate the claim with a user
        public string LecturerId { get; set; }

        //Computed property for the total salary
        public decimal TotalSalary => HoursWorked * HourlyRate;
    }
    //Digital TechJoint (2022). ASP.NET Identity - User Registration, Login and Log-out. [online] YouTube. Available at: https://www.youtube.com/watch?v=ghzvSROMo_M [Accessed 9 Oct. 2024].
    //Digital TechJoint (2022). ASP.NET MVC - How To Implement Role Based Authorization. YouTube. Available at: https://www.youtube.com/watch?v=qvsWwwq2ynE [Accessed 10 Oct. 2024].
}
