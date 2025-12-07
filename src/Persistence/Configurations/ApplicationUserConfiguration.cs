using Domain.Entities;
using Domain.Entities.Procurement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder
            .HasOne<ProcurementRoleTemplate>(user => user.ProcurementRoleTemplate)
            .WithMany()
            .HasForeignKey(user => user.ProcurementRoleTemplateId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
