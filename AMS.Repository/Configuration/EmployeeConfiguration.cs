using AMS.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMS.Repository.Configuration
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.EmployeeCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.Designation)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.JoiningDate)
                .IsRequired();

            builder.Property(e => e.IsActive)
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(e => e.EmployeeCode)
                .IsUnique()
                .HasName("IX_Employee_Code_Unique");

            builder.HasIndex(e => e.Email)
                .IsUnique()
                .HasName("IX_Employee_Email_Unique");

            builder.HasIndex(e => e.DepartmentId)
                .HasName("IX_Employee_DepartmentId");

            builder.HasIndex(e => e.IsActive)
                .HasName("IX_Employee_IsActive");

            // Relationships
            builder.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Employee_Department");

            builder.HasMany(e => e.Attendances)
                .WithOne(a => a.Employee)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Attendance_Employee");

            builder.HasMany(e => e.Leaves)
                .WithOne(l => l.Employee)
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Leave_Employee");

            // Table
            builder.ToTable("Employees", t => t.HasComment("Employee master data"));
        }
    }
}
