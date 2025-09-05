using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.OwnsOne(c => c.Name, name =>
        {
            name.Property(n => n.Kz).HasColumnName("NameKz").HasMaxLength(200);
            name.Property(n => n.Ru).HasColumnName("NameRu").HasMaxLength(200);
            name.Property(n => n.En).HasColumnName("NameEn").HasMaxLength(200);
        });

        builder.OwnsOne(c => c.Description, description =>
        {
            description.Property(d => d.Kz).HasColumnName("DescriptionKz").HasMaxLength(1000);
            description.Property(d => d.Ru).HasColumnName("DescriptionRu").HasMaxLength(1000);
            description.Property(d => d.En).HasColumnName("DescriptionEn").HasMaxLength(1000);
        });

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();

        builder.HasMany(c => c.Products)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.Id }).IsUnique();
    }
}