using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.ToTable("WorkflowSteps");

        builder.HasKey(step => step.Id);

        builder.Property(step => step.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(step => step.StepType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(step => step.Order)
            .IsRequired();

        builder.HasOne(step => step.Workflow)
            .WithMany(workflow => workflow.Steps)
            .HasForeignKey(step => step.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(step => step.Assignee)
            .WithMany()
            .HasForeignKey(step => step.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(step => new { step.WorkflowId, step.Order })
            .IsUnique();
    }
}
