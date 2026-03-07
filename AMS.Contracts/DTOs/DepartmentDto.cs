namespace AMS.Contracts.DTOs
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string? Description { get; set; }
        public int? ManagerId { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateDepartmentDto
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string? Description { get; set; }
        public int? ManagerId { get; set; }
    }

    public class UpdateDepartmentDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? ManagerId { get; set; }
        public bool? IsActive { get; set; }
    }
}
