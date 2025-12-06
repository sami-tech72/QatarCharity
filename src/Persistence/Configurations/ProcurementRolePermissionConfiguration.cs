using Domain.Entities.Procurement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class ProcurementRolePermissionConfiguration : IEntityTypeConfiguration<ProcurementRolePermission>
{
    public void Configure(EntityTypeBuilder<ProcurementRolePermission> builder)
    {
        builder.HasOne(p => p.ProcurementPermissionDefinition)
            .WithMany(d => d.RolePermissions)
            .HasForeignKey(p => p.ProcurementPermissionDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
