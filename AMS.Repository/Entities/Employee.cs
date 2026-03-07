namespace AMS.Repository.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public int DepartmentId { get; set; }
        public string Designation { get; set; } = null!;
        public DateTime JoiningDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual Department? Department { get; set; }
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<Leave> Leaves { get; set; } = new List<Leave>();
    }
}
