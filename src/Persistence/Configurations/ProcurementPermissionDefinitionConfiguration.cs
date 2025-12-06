using Domain.Entities.Procurement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class ProcurementPermissionDefinitionConfiguration : IEntityTypeConfiguration<ProcurementPermissionDefinition>
{
    public void Configure(EntityTypeBuilder<ProcurementPermissionDefinition> builder)
    {
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(256);
    }
}
