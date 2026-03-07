using AMS.Repository.Configuration;
using AMS.Repository.Entities;
using Microsoft.EntityFrameworkCore;

namespace AMS.Repository.Context
{
    public class AttendanceManagementContext : DbContext
    {
        public AttendanceManagementContext(DbContextOptions<AttendanceManagementContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Attendance> Attendances { get; set; } = null!;
        public DbSet<Leave> Leaves { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configuration classes
            modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
            modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
            modelBuilder.ApplyConfiguration(new AttendanceConfiguration());
            modelBuilder.ApplyConfiguration(new LeaveConfiguration());
        }
    }
}
