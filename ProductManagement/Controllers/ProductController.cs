using Contracts.IServices;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace ProductManagement.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IProductService _productService;

        public ProductController(ILogger<ProductController> logger, IProductService productService)
        {
            _logger = logger;
            _productService = productService;
        }

        /// <summary>
        /// API that receives a collection of product data, adds it to a database, and returns
        /// a status code indicating success.
        /// </summary>
        /// <param name="products">An ICollection of ProductCreateDTO objects that are received in the request
        /// body. This method is an HTTP POST method that creates new products in the database using the data
        /// provided in the ProductCreateDTO objects. The method logs information about the request and the
        /// success of the operation and returns a status code of</param>
        /// <returns>
        /// The method is returning a `StatusCodeResult` with a status code of `201 Created`.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> AddProducts([FromBody] ICollection<ProductCreateDto> products)
        {
            _logger.LogInformation("Received request to add new product");

            await _productService.AddProductsToDBAsync(products);

            _logger.LogInformation("Successfully added product(s) to the database");

            return StatusCode(StatusCodes.Status201Created);
        }

        /// <summary>
        /// API that retrieves product information by ID and returns it as a response.
        /// </summary>
        /// <param name="Guid">Guid stands for Globally Unique Identifier, which is a 128-bit integer used to
        /// identify unique objects or entities. In this case, it is used to identify a specific product by its
        /// unique ID.</param>
        /// <returns>
        /// The method is returning an IActionResult object with a status code of 200 (OK) and a
        /// ProductResponseDto object containing information about a product with the specified ID.
        /// </returns>
        [HttpGet("{productID}")]
        public async Task<IActionResult> GetProduct([FromRoute] Guid productID)
        {
            _logger.LogInformation("Received request to get product information for the ID: " + productID);

            ProductResponseDto productAtID = await _productService.GetProductByIDAsync(productID);

            _logger.LogInformation("Returning the complete product information for: " + productID);

            return StatusCode(StatusCodes.Status200OK, productAtID);
        }

        /// <summary>
        /// This function retrieves all products with pagination and returns a 204 status code if no products
        /// are found.
        /// </summary>
        /// <param name="pageNumber">The page number of the products to be retrieved. It is an optional
        /// parameter with a default value of 1.</param>
        /// <param name="pageSize">The number of products to be returned per page. It is an optional parameter
        /// with a default value of 10.</param>
        /// <returns>
        /// The method is returning a collection of ProductResponseDto objects with a status code of 200 OK if
        /// products are found. If no products are found, it returns a status code of 204 No Content.
        /// </returns>s
        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] int? pageNumber, [FromQuery] int? pageSize)
        {
            _logger.LogInformation($"Received request to get all products. PageNumber: {pageNumber}, PageSize: {pageSize}");

            IEnumerable<ProductResponseDto>? products = await _productService.GetAllProductsAsync(pageNumber, pageSize);

            if (products == null)
            {
                _logger.LogInformation("No product records found. Returning 0 records.");

                return StatusCode(StatusCodes.Status204NoContent);
            }

            _logger.LogInformation($"Returning all products at PageNumber: {pageNumber}, PageSize: {pageSize}");

            return StatusCode(StatusCodes.Status200OK, products);
        }
    }
}