using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class RfxConfiguration : IEntityTypeConfiguration<Rfx>
{
    public void Configure(EntityTypeBuilder<Rfx> builder)
    {
        builder.ToTable("Rfxes");

        builder.HasKey(rfx => rfx.Id);

        builder.Property(rfx => rfx.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(rfx => rfx.RfxType)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(rfx => rfx.Category)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(rfx => rfx.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(rfx => rfx.Department)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(rfx => rfx.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(rfx => rfx.EstimatedBudget)
            .HasPrecision(18, 2);

        builder.Property(rfx => rfx.Currency)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(rfx => rfx.Priority)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(rfx => rfx.ContactPerson)
            .HasMaxLength(256);

        builder.Property(rfx => rfx.ContactEmail)
            .HasMaxLength(256);

        builder.Property(rfx => rfx.ContactPhone)
            .HasMaxLength(64);

        builder.Property(rfx => rfx.Scope)
            .HasMaxLength(4000);

        builder.Property(rfx => rfx.TechnicalSpecification)
            .HasMaxLength(4000);

        builder.Property(rfx => rfx.Deliverables)
            .HasMaxLength(4000);

        builder.Property(rfx => rfx.Timeline)
            .HasMaxLength(4000);

        builder.Property(rfx => rfx.RequiredDocuments)
            .HasMaxLength(1000);

        builder.Property(rfx => rfx.EvaluationNotes)
            .HasMaxLength(2000);

        builder.Property(rfx => rfx.Status)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(rfx => rfx.CreatedAt)
            .IsRequired();

        builder.Property(rfx => rfx.LastModified)
            .IsRequired();

        builder.HasIndex(rfx => rfx.ReferenceNumber)
            .IsUnique();

        builder.HasOne(rfx => rfx.Workflow)
            .WithMany()
            .HasForeignKey(rfx => rfx.WorkflowId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
