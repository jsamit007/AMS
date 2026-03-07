using AMS.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMS.Repository.Configuration
{
    public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.EmployeeId)
                .IsRequired();

            builder.Property(a => a.Date)
                .IsRequired();

            builder.Property(a => a.CheckInTime)
                .IsRequired();

            builder.Property(a => a.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Absent");

            builder.Property(a => a.Remarks)
                .HasMaxLength(500);

            builder.Property(a => a.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(a => new { a.EmployeeId, a.Date })
                .IsUnique()
                .HasName("IX_Attendance_Employee_Date_Unique");

            builder.HasIndex(a => a.Date)
                .HasName("IX_Attendance_Date");

            builder.HasIndex(a => a.Status)
                .HasName("IX_Attendance_Status");

            builder.HasIndex(a => a.EmployeeId)
                .HasName("IX_Attendance_EmployeeId");

            // Relationships
            builder.HasOne(a => a.Employee)
                .WithMany(e => e.Attendances)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Attendance_Employee");

            // Table
            builder.ToTable("Attendances", t => t.HasComment("Daily attendance records"));
        }
    }
}
