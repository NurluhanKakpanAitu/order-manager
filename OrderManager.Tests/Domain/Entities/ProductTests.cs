using Domain.Entities;
using Domain.Entities.Base;
using FluentAssertions;

namespace OrderManager.Tests.Domain.Entities;

public class ProductTests
{
    [Fact]
    public void Product_Constructor_ShouldCreateProductWithCorrectProperties()
    {
        // Arrange
        var name = Translation.Create("Товар", "Product", "Product");
        var price = 100.50m;
        var quantity = 10;
        var categoryId = Guid.NewGuid();
        
        // Act
        var product = new Product(name, price, quantity, categoryId);
        
        // Assert
        product.Id.Should().NotBeEmpty();
        product.Name.Should().Be(name);
        product.Price.Should().Be(price);
        product.Quantity.Should().Be(quantity);
        product.CategoryId.Should().Be(categoryId);
    }
    
    [Fact]
    public void UpdateProduct_ShouldUpdateAllProperties()
    {
        // Arrange
        var product = CreateTestProduct();
        var newName = Translation.Create("Новый товар", "New Product", "New Product");
        var newPrice = 200.75m;
        var newQuantity = 20;
        
        // Act
        product.UpdateProduct(newName, newPrice, newQuantity);
        
        // Assert
        product.Name.Should().Be(newName);
        product.Price.Should().Be(newPrice);
        product.Quantity.Should().Be(newQuantity);
    }
    
    [Fact]
    public void UpdateQuantity_WithValidQuantity_ShouldUpdateQuantity()
    {
        // Arrange
        var product = CreateTestProduct();
        var newQuantity = 25;
        
        // Act
        product.UpdateQuantity(newQuantity);
        
        // Assert
        product.Quantity.Should().Be(newQuantity);
    }
    
    [Fact]
    public void UpdateQuantity_WithNegativeQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var product = CreateTestProduct();
        
        // Act & Assert
        var action = () => product.UpdateQuantity(-1);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Quantity cannot be negative");
    }
    
    [Fact]
    public void IsAvailable_WithPositiveQuantity_ShouldReturnTrue()
    {
        // Arrange
        var product = CreateTestProduct();
        product.UpdateQuantity(5);
        
        // Act & Assert
        product.IsAvailable().Should().BeTrue();
    }
    
    [Fact]
    public void IsAvailable_WithZeroQuantity_ShouldReturnFalse()
    {
        // Arrange
        var product = CreateTestProduct();
        product.UpdateQuantity(0);
        
        // Act & Assert
        product.IsAvailable().Should().BeFalse();
    }
    
    private static Product CreateTestProduct()
    {
        var name = Translation.Create("Тест", "Test", "Test");
        return new Product(name, 100m, 10, Guid.NewGuid());
    }
}