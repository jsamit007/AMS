using AMS.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMS.Repository.Configuration
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.Code)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(d => d.Description)
                .HasMaxLength(500);

            builder.Property(d => d.IsActive)
                .HasDefaultValue(true);

            builder.Property(d => d.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(d => d.Code)
                .IsUnique()
                .HasName("IX_Department_Code_Unique");

            builder.HasIndex(d => d.IsActive)
                .HasName("IX_Department_IsActive");

            // Relationships
            builder.HasMany(d => d.Employees)
                .WithOne(e => e.Department)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Employee_Department");

            // Table
            builder.ToTable("Departments", t => t.HasComment("Department master data"));
        }
    }
}
