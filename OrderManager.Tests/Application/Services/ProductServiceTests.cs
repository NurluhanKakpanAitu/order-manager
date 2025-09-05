using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Base;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace OrderManager.Tests.Application.Services;

public class ProductServiceTests
{
    private readonly Mock<IGenericRepository<Product>> _productRepositoryMock;
    private readonly Mock<IGenericRepository<Category>> _categoryRepositoryMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _productRepositoryMock = new Mock<IGenericRepository<Product>>();
        _categoryRepositoryMock = new Mock<IGenericRepository<Category>>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        var unitOfWork = unitOfWorkMock.Object;
        var loggerMock = new Mock<ILogger<ProductService>>();
        var logger = loggerMock.Object;
        var mapperMock = new Mock<AutoMapper.IMapper>();
        mapperMock.Setup(m => m.Map<Product>(It.IsAny<CreateProductRequest>()))
            .Returns((CreateProductRequest request) => new Product(request.Name, request.Price, request.Quantity, request.CategoryId) { Description = request.Description });
        var mapper = mapperMock.Object;
        var localizationServiceMock = new Mock<ILocalizationService>();
        localizationServiceMock.Setup(x => x.GetCurrentLanguageCode()).Returns("en");
        _productService = new ProductService(_productRepositoryMock.Object, _categoryRepositoryMock.Object, unitOfWork, logger, mapper, localizationServiceMock.Object);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithExistingProduct_ShouldReturnProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = CreateTestProduct(productId, categoryId);
        
        var categoryVm = new CategoryVm(product.CategoryId, Translation.Create("Тест категория", "Test Category", "Test Category"), null);
        _productRepositoryMock.Setup(x => x.GetByIdMappedAsync<ProductVm>(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductVm(productId, product.Name, product.Description, product.Price, product.Quantity, categoryVm, DateTime.UtcNow, null));
        
        // Act
        var result = await _productService.GetByIdAsync(productId);
        
        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productId);
        result.Name.En.Should().Be("Test Product");
        result.Price.Should().Be(100m);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithNonExistingProduct_ShouldThrowArgumentException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _productRepositoryMock.Setup(x => x.GetByIdMappedAsync<ProductVm>(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductVm?)null);
        
        // Act & Assert
        var action = async () => await _productService.GetByIdAsync(productId);
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Product not found");
    }
    
    [Fact]
    public async Task CreateAsync_WithValidProduct_ShouldCreateProductAndReturnProduct()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var name = Translation.Create("Тест товар", "Test Product", "Test Product");
        var description = Translation.Create("Описание", "Description", "Description");
        var createProductRequest = new CreateProductRequest(name, description, 150m, 20, categoryId);
        
        _categoryRepositoryMock.Setup(x => x.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _productRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) => p);
        
        // Act
        await _productService.CreateAsync(createProductRequest);
        
        // Assert
        _productRepositoryMock.Verify(x => x.AddAsync(It.Is<Product>(p => p.Name.En == "Test Product" && p.Price == 150m && p.Quantity == 20 && p.CategoryId == categoryId), It.IsAny<CancellationToken>()), Times.Once);
        
        _categoryRepositoryMock.Verify(x => x.ExistsAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task CreateAsync_WithNonExistingCategory_ShouldThrowArgumentException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var name = Translation.Create("Тест товар", "Test Product", "Test Product");
        var createProductRequest = new CreateProductRequest(name, null, 150m, 20, categoryId);
        
        _categoryRepositoryMock.Setup(x => x.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        // Act & Assert
        var action = async () => await _productService.CreateAsync(createProductRequest);
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Category not found");
    }
    
    [Fact]
    public async Task UpdateAsync_WithValidProduct_ShouldUpdateProductAndReturnProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = CreateTestProduct(productId, categoryId);
        var updatedName = Translation.Create("Обновленный товар", "Updated Product", "Updated Product");
        var updatedDescription = Translation.Create("Обновленное описание", "Updated Description", "Updated Description");
        var updateProductRequest = new UpdateProductRequest(updatedName, updatedDescription, 200m, 30);
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        
        // Act
        await _productService.UpdateAsync(productId, updateProductRequest);
        
    }
    
    private static Product CreateTestProduct(Guid? id = null, Guid? categoryId = null)
    {
        var name = Translation.Create("Тест товар", "Test Product", "Test Product");
        var product = new Product(name, 100m, 10, categoryId ?? Guid.NewGuid());
        if (id.HasValue)
        {
            typeof(Product).GetProperty("Id")?.SetValue(product, id.Value);
        }
        return product;
    }
    
    private static Category CreateTestCategory(Guid? id = null)
    {
        var name = Translation.Create("Тест категория", "Test Category", "Test Category");
        var category = new Category(name);
        if (id.HasValue)
        {
            typeof(Category).GetProperty("Id")?.SetValue(category, id.Value);
        }
        return category;
    }
}