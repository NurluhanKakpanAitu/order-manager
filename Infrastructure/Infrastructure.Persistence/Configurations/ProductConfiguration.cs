using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.OwnsOne(p => p.Name, name =>
        {
            name.Property(n => n.Kz).HasColumnName("NameKz").HasMaxLength(200);
            name.Property(n => n.Ru).HasColumnName("NameRu").HasMaxLength(200);
            name.Property(n => n.En).HasColumnName("NameEn").HasMaxLength(200);
        });

        builder.OwnsOne(p => p.Description, description =>
        {
            description.Property(d => d.Kz).HasColumnName("DescriptionKz").HasMaxLength(1000);
            description.Property(d => d.Ru).HasColumnName("DescriptionRu").HasMaxLength(1000);
            description.Property(d => d.En).HasColumnName("DescriptionEn").HasMaxLength(1000);
        });

        builder.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.Quantity).IsRequired();
        builder.Property(p => p.CategoryId).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.Price);
    }
}