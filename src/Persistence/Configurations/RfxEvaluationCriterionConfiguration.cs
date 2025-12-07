using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class RfxEvaluationCriterionConfiguration : IEntityTypeConfiguration<RfxEvaluationCriterion>
{
    public void Configure(EntityTypeBuilder<RfxEvaluationCriterion> builder)
    {
        builder.ToTable("RfxEvaluationCriteria");

        builder.HasKey(criterion => criterion.Id);

        builder.Property(criterion => criterion.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(criterion => criterion.Description)
            .HasMaxLength(2000);

        builder.Property(criterion => criterion.Type)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasOne(criterion => criterion.Rfx)
            .WithMany(rfx => rfx.EvaluationCriteria)
            .HasForeignKey(criterion => criterion.RfxId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
