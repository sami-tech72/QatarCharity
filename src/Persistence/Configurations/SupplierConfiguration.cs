using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(supplier => supplier.Id);

        builder.Property(supplier => supplier.SupplierCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(supplier => supplier.SupplierCode)
            .IsUnique();

        builder.Property(supplier => supplier.CompanyName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(supplier => supplier.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(supplier => supplier.PrimaryContactName)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(supplier => supplier.PrimaryContactEmail)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(supplier => supplier.PrimaryContactPhone)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(supplier => supplier.CompanyAddress)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(supplier => supplier.Website)
            .HasMaxLength(200);

        builder.Property(supplier => supplier.Category)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(supplier => supplier.ContactPerson)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(supplier => supplier.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(supplier => supplier.PortalUserEmail)
            .HasMaxLength(150);

        builder.Property(supplier => supplier.BusinessCategories)
            .IsRequired()
            .HasMaxLength(600);

        builder.Property(supplier => supplier.UploadedDocuments)
            .IsRequired()
            .HasMaxLength(600);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(supplier => supplier.PortalUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
