using Domain.Entities.Procurement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class ProcurementRoleTemplateConfiguration : IEntityTypeConfiguration<ProcurementRoleTemplate>
{
    public void Configure(EntityTypeBuilder<ProcurementRoleTemplate> builder)
    {
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(512);

        builder.HasMany(r => r.Avatars)
            .WithOne(a => a.ProcurementRoleTemplate)
            .HasForeignKey(a => a.ProcurementRoleTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Permissions)
            .WithOne(p => p.ProcurementRoleTemplate)
            .HasForeignKey(p => p.ProcurementRoleTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
