namespace AMS.Repository.Entities
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public int? ManagerId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
