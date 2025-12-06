using Domain.Entities.Procurement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class ProcurementRoleAvatarConfiguration : IEntityTypeConfiguration<ProcurementRoleAvatar>
{
    public void Configure(EntityTypeBuilder<ProcurementRoleAvatar> builder)
    {
        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(256);
    }
}
