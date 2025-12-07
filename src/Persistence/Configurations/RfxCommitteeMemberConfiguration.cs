using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class RfxCommitteeMemberConfiguration : IEntityTypeConfiguration<RfxCommitteeMember>
{
    public void Configure(EntityTypeBuilder<RfxCommitteeMember> builder)
    {
        builder.ToTable("RfxCommitteeMembers");

        builder.HasKey(member => member.Id);

        builder.Property(member => member.DisplayName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(member => member.UserId)
            .HasMaxLength(450);

        builder.Property(member => member.IsApproved)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(member => member.Rfx)
            .WithMany(rfx => rfx.CommitteeMembers)
            .HasForeignKey(member => member.RfxId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
