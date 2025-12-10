using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("Contracts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.SupplierName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(c => c.SupplierUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(c => c.ContractValue)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(c => c.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.CreatedAtUtc)
            .IsRequired();
    }
}
