using Domain.Entities;
using Domain.Entities.Base;
using FluentAssertions;
using Infrastructure.Persistence.Data;
using Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace OrderManager.Tests.Infrastructure;

public class AuditableEntityInterceptorTests
{
    [Fact]
    public async Task SaveChangesAsync_WhenAddingEntity_ShouldSetCreatedAtAndUpdatedAt()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<OrderManagerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(new AuditableEntityInterceptor())
            .Options;

        await using var context = new OrderManagerDbContext(options);
        var name = Translation.Create("Test", "Test", "Test");
        var category = new Category(name);
        var beforeAdd = DateTime.UtcNow;

        // Act
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        var afterAdd = DateTime.UtcNow;

        // Assert
        category.CreatedAt.Should().BeAfter(beforeAdd.AddSeconds(-1));
        category.CreatedAt.Should().BeBefore(afterAdd.AddSeconds(1));
        category.UpdatedAt.Should().BeAfter(beforeAdd.AddSeconds(-1));
        category.UpdatedAt.Should().BeBefore(afterAdd.AddSeconds(1));
        category.CreatedAt.Should().Be(category.UpdatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenUpdatingEntity_ShouldUpdateUpdatedAtOnly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<OrderManagerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(new AuditableEntityInterceptor())
            .Options;

        await using var context = new OrderManagerDbContext(options);
        var name = Translation.Create("Test", "Test", "Test");
        var category = new Category(name);
        
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        
        var originalCreatedAt = category.CreatedAt;
        var originalUpdatedAt = category.UpdatedAt;
        
        // Wait a moment to ensure different timestamps
        await Task.Delay(100);

        // Act - force an update by modifying a property directly
        var newName = Translation.Create("Updated", "Updated", "Updated");
        category.Name = newName;
        context.Categories.Update(category);
        await context.SaveChangesAsync();

        // Assert
        category.CreatedAt.Should().Be(originalCreatedAt);
        category.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }
}