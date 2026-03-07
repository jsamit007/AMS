using AMS.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMS.Repository.Configuration
{
    public class LeaveConfiguration : IEntityTypeConfiguration<Leave>
    {
        public void Configure(EntityTypeBuilder<Leave> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.EmployeeId)
                .IsRequired();

            builder.Property(l => l.LeaveType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(l => l.FromDate)
                .IsRequired();

            builder.Property(l => l.ToDate)
                .IsRequired();

            builder.Property(l => l.NumberOfDays)
                .IsRequired();

            builder.Property(l => l.Reason)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(l => l.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            builder.Property(l => l.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(l => new { l.EmployeeId, l.Status })
                .HasName("IX_Leave_Employee_Status");

            builder.HasIndex(l => l.Status)
                .HasName("IX_Leave_Status");

            builder.HasIndex(l => l.FromDate)
                .HasName("IX_Leave_FromDate");

            builder.HasIndex(l => l.ToDate)
                .HasName("IX_Leave_ToDate");

            builder.HasIndex(l => l.EmployeeId)
                .HasName("IX_Leave_EmployeeId");

            builder.HasIndex(l => l.LeaveType)
                .HasName("IX_Leave_LeaveType");

            // Relationships
            builder.HasOne(l => l.Employee)
                .WithMany(e => e.Leaves)
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Leave_Employee");

            builder.HasOne(l => l.ApprovedByEmployee)
                .WithMany()
                .HasForeignKey(l => l.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Leave_ApprovedByEmployee");

            // Table
            builder.ToTable("Leaves", t => t.HasComment("Leave requests and history"));
        }
    }
}
