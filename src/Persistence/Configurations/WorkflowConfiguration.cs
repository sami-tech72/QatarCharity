using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.ToTable("Workflows");

        builder.HasKey(workflow => workflow.Id);

        builder.Property(workflow => workflow.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(workflow => workflow.Description)
            .HasMaxLength(1000);

        builder.Property(workflow => workflow.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(workflow => workflow.CreatedAt)
            .IsRequired();

        builder.Property(workflow => workflow.LastModified)
            .IsRequired();

        builder.HasIndex(workflow => workflow.Name)
            .IsUnique();
    }
}
