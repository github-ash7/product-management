using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Contracts.IRepository;
using Contracts.IServices;
using Entities.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ProductManagement.Controllers;
using Repository;
using Services;

namespace ProductManagementTests.Controllers
{
    public class ProductControllerTests : IDisposable
    {
        private ProductController controller;

        public ProductControllerTests()
        {
            IFixture fixture = new Fixture().Customize(new AutoMoqCustomization());
            IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<ProductProfile>()).CreateMapper();

            // Prep - 'Product Repository'

            ILogger<ProductRepository> loggerForRepo = fixture.Freeze<Mock<ILogger<ProductRepository>>>().Object;

            Mock<IConfiguration> configurationMock = fixture.Freeze<Mock<IConfiguration>>();

            IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ConnectionStrings:DefaultConnection", "Data Source=192.168.11.77;Initial Catalog=devtraining;TrustServerCertificate=true;User ID=hariharan.s;Password=hari@7708" },
                { "ProductTable:Name", "assignment_product_management.product_test" }
            })
            .Build();

            // Create an instance for 'Product Repository'
            IProductRepository productRepository = new ProductRepository(loggerForRepo, configuration, mapper);

            // Prep - 'Product Service'

            ILogger<ProductService> loggerForService = fixture.Freeze<Mock<ILogger<ProductService>>>().Object;

            //IMemoryCache cache = fixture.Freeze<Mock<IMemoryCache>>().Object;

            MemoryCache cache = new MemoryCache(new MemoryCacheOptions());

            // Create an instance for 'Product Service'
            IProductService productService = new ProductService(loggerForService, productRepository, mapper, cache);

            // Prep - 'Product Controller'

            ILogger<ProductController> loggerForController = fixture.Freeze<Mock<ILogger<ProductController>>>().Object;

            // Create an instance for 'Product Controller'
            controller = new ProductController(loggerForController, productService);
        }

        public void Dispose()
        {
            TestingTableManager.DeleteAllRecords();
        }

        [Fact]
        public async Task AddProducts_WhenCalledWithValidData_InsertsAllTheDataToDB()
        {
            // Arrange

            ICollection<ProductCreateDto> products = new List<ProductCreateDto>();

            products.Add
            (
                new ProductCreateDto
                {
                    ID = Guid.Parse("7487fb0d-09b1-4580-a470-66cc74bb3282"),
                    ProductName = "Samsung Galaxy S23",
                    CategoryID = Guid.Parse("63d14238-8362-4242-a4a9-ef2d9b1ce7e8"),
                    SupplierID = Guid.Parse("7487fb0d-09b1-4580-a470-66cc74bb3282"),
                    UnitPrice = 60000,
                    UnitsInStock = 100000,
                    Discontinued = false
                }
            );

            products.Add
            (
                new ProductCreateDto
                {
                    ID = Guid.Parse("a475a72e-13bf-46e8-b1f5-28f6a464235f"),
                    ProductName = "Spigen Liquid Case for Samsung Galaxy S23",
                    CategoryID = Guid.Parse("6258e123-bd24-441c-857f-cb2764ecf8f7"),
                    SupplierID = Guid.Parse("eadfe4c9-731d-4556-978c-2f9105a1550b"),
                    UnitPrice = 1600,
                    UnitsInStock = 1000,
                    Discontinued = false
                }
            );

            // Act
            var result = await controller.AddProducts(products);

            // Assert
            Assert.IsType<StatusCodeResult>(result);
            var statusCodeResult = (StatusCodeResult)result;
            Assert.Equal(StatusCodes.Status201Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task AddProducts_WhenCalledWithDuplicateProductNamesInTheRequestBody_ThrowsConflictException()
        {
            // Arrange

            ICollection<ProductCreateDto> products = new List<ProductCreateDto>();

            products.Add
            (
                new ProductCreateDto
                {
                    ID = Guid.Parse("7487fb0d-09b1-4580-a470-66cc74bb3282"),
                    ProductName = "Samsung Galaxy S23",
                    CategoryID = Guid.Parse("63d14238-8362-4242-a4a9-ef2d9b1ce7e8"),
                    SupplierID = Guid.Parse("7487fb0d-09b1-4580-a470-66cc74bb3282"),
                    UnitPrice = 60000,
                    UnitsInStock = 100000,
                    Discontinued = false
                }
            );

            products.Add
            (
                new ProductCreateDto
                {
                    ID = Guid.Parse("a475a72e-13bf-46e8-b1f5-28f6a464235f"),
                    ProductName = "Samsung Galaxy S23",
                    CategoryID = Guid.Parse("6258e123-bd24-441c-857f-cb2764ecf8f7"),
                    SupplierID = Guid.Parse("eadfe4c9-731d-4556-978c-2f9105a1550b"),
                    UnitPrice = 1600,
                    UnitsInStock = 1000,
                    Discontinued = false
                }
            );

            // Act and Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(async () => await controller.AddProducts(products));
            Assert.Equal("The provided data contains duplicate product names: Samsung Galaxy S23", exception.Message);
        }

        [Fact]
        public async Task AddProducts_WhenCalledWithExistingProductNamesInDB_ThrowsConflictException()
        {
            // Arrange
            TestingTableManager.InsertRecords();

            ICollection<ProductCreateDto> products = new List<ProductCreateDto>();

            products.Add
            (
                new ProductCreateDto
                {
                    ID = Guid.Parse("7487fb0d-09b1-4580-a470-66cc74bb3282"),
                    ProductName = "Google Pixel 7 Pro (128 GB Storage, 12 GB RAM)",
                    CategoryID = Guid.Parse("63d14238-8362-4242-a4a9-ef2d9b1ce7e8"),
                    SupplierID = Guid.Parse("7487fb0d-09b1-4580-a470-66cc74bb3282"),
                    UnitPrice = 60000,
                    UnitsInStock = 100000,
                    Discontinued = false
                }
            );

            products.Add
            (
                new ProductCreateDto
                {
                    ID = Guid.Parse("a475a72e-13bf-46e8-b1f5-28f6a464235f"),
                    ProductName = "Spigen Liquid Case for Google Pixel 7 Pro",
                    CategoryID = Guid.Parse("6258e123-bd24-441c-857f-cb2764ecf8f7"),
                    SupplierID = Guid.Parse("eadfe4c9-731d-4556-978c-2f9105a1550b"),
                    UnitPrice = 1600,
                    UnitsInStock = 1000,
                    Discontinued = false
                }
            );

            // Act and Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(async () => await controller.AddProducts(products));
            Assert.Equal("Products with names 'Google Pixel 7 Pro (128 GB Storage, 12 GB RAM), Spigen Liquid Case for Google Pixel 7 Pro' already exist", exception.Message);
        }

        [Fact]
        public async Task GetProduct_WhenTheRequestedProductIDExists_ReturnsTheCompleteProductInformation()
        {
            // Arrange
            TestingTableManager.InsertRecords();

            // Act - Gets from repository
            var actionResultFromRepo = await controller.GetProduct(Guid.Parse("5784d3df-e2da-4be7-b6e7-4a17d51ec2ac"));

            // Act - Gets from cache
            var actionResultFromCache = await controller.GetProduct(Guid.Parse("5784d3df-e2da-4be7-b6e7-4a17d51ec2ac"));

            // Assert 
            var result = Assert.IsType<ObjectResult>(actionResultFromRepo);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            var response = Assert.IsType<ProductResponseDto>(result.Value);

            Assert.Equal(Guid.Parse("5784d3df-e2da-4be7-b6e7-4a17d51ec2ac"), response.ID);
            Assert.Equal("Google Pixel 7 Pro (128 GB Storage, 12 GB RAM)", response.ProductName);
            Assert.Equal(Guid.Parse("63d14238-8362-4242-a4a9-ef2d9b1ce7e8"), response.CategoryID);
            Assert.Equal(Guid.Parse("235f43c8-6202-47d1-9954-154f0607191b"), response.SupplierID);
            Assert.Equal(80000, response.UnitPrice);
            Assert.Equal(10000, response.UnitsInStock);
            Assert.Equal(false, response.Discontinued);
        }

        [Fact]
        public async Task GetProduct_WhenTheRequestedProductIDDoesNotExists_ThrowsNotFoundException()
        {
            // Arrange
            TestingTableManager.InsertRecords();

            // Act and Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(async () => await controller.GetProduct(Guid.Parse("9784d3df-e2da-4be7-b6e7-4a17d51ec2ac")));
            Assert.Equal("No product has been found for the ID: 9784d3df-e2da-4be7-b6e7-4a17d51ec2ac", exception.Message);
        }

        [Fact]
        public async Task GetProduct_WhenADiscontinuedProductIsRequested_ThrowsNotFoundException()
        {
            // Arrange
            TestingTableManager.InsertRecords();

            // Act and Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(async () => await controller.GetProduct(Guid.Parse("120af4f8-69f8-4a41-9c3e-dd2357b4baee")));
            Assert.Equal("No product has been found for the ID: 120af4f8-69f8-4a41-9c3e-dd2357b4baee", exception.Message);
        }

        [Fact]
        public async Task GetAllProducts_WhenAtLeastOneProductExistsInDBForTheRequestedPageNumberAndPageSize_ReturnsAllTheProductRecordsForTheRequestedSize()
        {
            // Arrange
            TestingTableManager.InsertRecords();

            // Act - Gets from repository
            var actionResultFromRepo = await controller.GetAllProducts(1, 1);

            // Act - Gets from cache
            var actionResultFromCache = await controller.GetAllProducts(1, 1);

            // Assert
            var result = Assert.IsType<ObjectResult>(actionResultFromRepo);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            var response = Assert.IsAssignableFrom<IEnumerable<ProductResponseDto>>(result.Value);

            // Assert 
            var firstProduct = response.First();
            Assert.Equal(Guid.Parse("5784d3df-e2da-4be7-b6e7-4a17d51ec2ac"), firstProduct.ID);
            Assert.Equal("Google Pixel 7 Pro (128 GB Storage, 12 GB RAM)", firstProduct.ProductName);
            Assert.Equal(Guid.Parse("63d14238-8362-4242-a4a9-ef2d9b1ce7e8"), firstProduct.CategoryID);
            Assert.Equal(Guid.Parse("235f43c8-6202-47d1-9954-154f0607191b"), firstProduct.SupplierID);
            Assert.Equal(80000, firstProduct.UnitPrice);
            Assert.Equal(10000, firstProduct.UnitsInStock);
            Assert.Equal(false, firstProduct.Discontinued);
        }

        [Fact]
        public async Task GetAllProducts_WhenRequestedForAllProductsFromDB_ReturnsAllTheProductRecords()
        {
            // Arrange
            TestingTableManager.InsertRecords();

            // Act - Gets from repository
            var actionResultFromRepo = await controller.GetAllProducts(null, null);

            // Act - Gets from cache
            var actionResultFromCache = await controller.GetAllProducts(null, null);

            // Assert
            var result = Assert.IsType<ObjectResult>(actionResultFromRepo);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            var response = Assert.IsAssignableFrom<IEnumerable<ProductResponseDto>>(result.Value);

            // Assert the first product in the response
            var firstProduct = response.First();
            Assert.Equal(Guid.Parse("5784d3df-e2da-4be7-b6e7-4a17d51ec2ac"), firstProduct.ID);
            Assert.Equal("Google Pixel 7 Pro (128 GB Storage, 12 GB RAM)", firstProduct.ProductName);
            Assert.Equal(Guid.Parse("63d14238-8362-4242-a4a9-ef2d9b1ce7e8"), firstProduct.CategoryID);
            Assert.Equal(Guid.Parse("235f43c8-6202-47d1-9954-154f0607191b"), firstProduct.SupplierID);
            Assert.Equal(80000, firstProduct.UnitPrice);
            Assert.Equal(10000, firstProduct.UnitsInStock);
            Assert.Equal(false, firstProduct.Discontinued);

            // Assert the second product in the response
            var secondProduct = response.Skip(1).First();
            Assert.Equal(Guid.Parse("330b834a-ce6a-4db4-86f6-9f6ae94b8280"), secondProduct.ID);
            Assert.Equal("Spigen Liquid Case for Google Pixel 7 Pro", secondProduct.ProductName);
            Assert.Equal(Guid.Parse("6258e123-bd24-441c-857f-cb2764ecf8f7"), secondProduct.CategoryID);
            Assert.Equal(Guid.Parse("eadfe4c9-731d-4556-978c-2f9105a1550b"), secondProduct.SupplierID);
            Assert.Equal(1000, secondProduct.UnitPrice);
            Assert.Equal(1000, secondProduct.UnitsInStock);
            Assert.Equal(false, secondProduct.Discontinued);
        }

        [Fact]
        public async Task GetAllProducts_WhenNoProductRecordsExistsInDB_Returns204StatusCode()
        {
            // Act
            var result = await controller.GetAllProducts(null, null);

            // Assert
            Assert.IsType<StatusCodeResult>(result);
            var statusCodeResult = (StatusCodeResult)result;
            Assert.Equal(StatusCodes.Status204NoContent, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetAllProducts_WhenNoProductRecordsExistsForTheRequestedPageNumberAndSize_Returns204StatusCode()
        {
            // Arrange
            TestingTableManager.InsertRecords();

            // Act
            var result = await controller.GetAllProducts(2, 2);

            // Assert
            Assert.IsType<StatusCodeResult>(result);
            var statusCodeResult = (StatusCodeResult)result;
            Assert.Equal(StatusCodes.Status204NoContent, statusCodeResult.StatusCode);
        }
    }
}