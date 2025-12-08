using Domain.Entities;
using Domain.Entities.Procurement;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Workflow> Workflows => Set<Workflow>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<Rfx> Rfxes => Set<Rfx>();
    public DbSet<RfxEvaluationCriterion> RfxEvaluationCriteria => Set<RfxEvaluationCriterion>();
    public DbSet<RfxCommitteeMember> RfxCommitteeMembers => Set<RfxCommitteeMember>();
    public DbSet<SupplierBid> SupplierBids => Set<SupplierBid>();
    public DbSet<BidReview> BidReviews => Set<BidReview>();
    public DbSet<ProcurementPermissionDefinition> ProcurementPermissionDefinitions => Set<ProcurementPermissionDefinition>();
    public DbSet<ProcurementRoleTemplate> ProcurementRoleTemplates => Set<ProcurementRoleTemplate>();
    public DbSet<ProcurementRoleAvatar> ProcurementRoleAvatars => Set<ProcurementRoleAvatar>();
    public DbSet<ProcurementRolePermission> ProcurementRolePermissions => Set<ProcurementRolePermission>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        builder.Entity<SupplierBid>()
            .Property(bid => bid.BidAmount)
            .HasPrecision(18, 2);

        builder.Entity<SupplierBid>()
            .HasOne<Rfx>()
            .WithMany()
            .HasForeignKey(bid => bid.RfxId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SupplierBid>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(bid => bid.SubmittedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BidReview>()
            .HasOne<SupplierBid>()
            .WithMany()
            .HasForeignKey(review => review.BidId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<BidReview>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(review => review.ReviewerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
