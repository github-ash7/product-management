using AutoMapper;
using Contracts.IRepository;
using Contracts.IServices;
using Entities.DTOs;
using Entities.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Services
{
    public class ProductService : IProductService
    {
        private readonly ILogger<ProductService> _logger;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public ProductService(ILogger<ProductService> logger, IProductRepository productRepository,
                              IMapper mapper, IMemoryCache cache)
        {
            _logger = logger;
            _productRepository = productRepository;
            _mapper = mapper;
            _cache = cache;
        }

        /// <summary>
        /// This function adds a collection of products to a database after checking for duplicate names in the given
        /// request body and existing products.
        /// </summary>
        /// <param name="products">An ICollection of ProductCreateDTO objects, which contain information about
        /// the products to be added to the database.</param>
        public async Task AddProductsToDBAsync(ICollection<ProductCreateDto> products)
        {
            _logger.LogDebug("Received request to add products");

            IEnumerable<string> duplicateNames = products.GroupBy(p => p.ProductName)
                                                .Where(g => g.Count() > 1)
                                                .Select(g => g.Key);

            // Checks if the given request body data has duplicate product names before checking the database
            if (duplicateNames.Any())
            {
                throw new ConflictException($"The provided data contains duplicate product names: {string.Join(", ", duplicateNames)}");
            }

            HashSet<string> existingProductNames = await _productRepository.IsProductNameExistsAsync(products);

            // Throws a conflict custom exception if the product name(s) already exists in the database
            if (existingProductNames.Any())
            {
                string existingNames = string.Join(", ", existingProductNames);

                _logger.LogError($"Products with names '{existingNames}' already exist");

                throw new ConflictException($"Products with names '{existingNames}' already exist");
            }

            _logger.LogInformation("Successfully added product(s) to the database");

            await _productRepository.BulkInsertProductsAsync(products);
        }

        /// <summary>
        /// This function retrieves product information by ID, first checking if it's in cache and returning it
        /// if so, otherwise querying the database and caching the result for future requests.
        /// </summary>
        /// <param name="Guid">A unique identifier for a product, used to retrieve product information from the
        /// database or cache.</param>
        /// <returns>
        /// A `Task` that returns a `ProductResponseDto` object.
        /// </returns>
        public async Task<ProductResponseDto> GetProductByIDAsync(Guid productID)
        {
            _logger.LogDebug($"Received request to get product information for the ID: {productID}");

            string cacheKey = $"{productID}";

            // If the product is already in cache, returns the product information without calling the database
            if (_cache.TryGetValue(cacheKey, out Product? product))
            {
                _logger.LogDebug($"Returning the product information from cache for the ID: {productID}");

                return _mapper.Map<ProductResponseDto>(product);
            }

            Product? productFromDb = await _productRepository.GetProductByIdAsync(productID);

            // Throws a not found custom exception if no product is found in database
            if (productFromDb == null)
            {
                _logger.LogError($"No product has been found for the ID: {productID}");

                throw new NotFoundException($"No product has been found for the ID: {productID}");
            }

            // If the product is found and retrieved from database, adds the product to cache
            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            _cache.Set(cacheKey, productFromDb, cacheEntryOptions);

            return _mapper.Map<ProductResponseDto>(productFromDb);
        }

        /// <summary>
        /// This function retrieves a paginated list of products from a database, caches the results, and
        /// returns the mapped response DTO.
        /// </summary>
        /// <param name="pageNumber">The page number of the products to retrieve. This is used for pagination
        /// purposes.</param>
        /// <param name="pageSize">The number of products to be returned per page.</param>
        /// <returns>
        /// The method returns an asynchronous task that returns an IEnumerable of ProductResponseDto objects or
        /// null if no products are found in the database. The IEnumerable contains the products for the
        /// requested page number and page size, which are either retrieved from the cache or from the database
        /// and then added to the cache.
        /// </returns>
        public async Task<IEnumerable<ProductResponseDto>?> GetAllProductsAsync(int? pageNumber, int? pageSize)
        {
            _logger.LogInformation($"Received request to get all products. PageNumber: {pageNumber}, PageSize: {pageSize}");

            string cacheKey = $"{pageNumber}_{pageSize}";

            // If all the products for the requested page number and page size is in cache, returns the product information without calling the database
            if (_cache.TryGetValue(cacheKey, out IEnumerable<Product>? products))
            {
                _logger.LogDebug($"Returning the product information from cache. PageNumber: {pageNumber}, PageSize: {pageSize}");

                return _mapper.Map<IEnumerable<ProductResponseDto>>(products);
            }

            IEnumerable<Product> productsFromDb = await _productRepository.GetAllProductsAsync();

            // Returns null, if no products has been found in the database
            if (!productsFromDb.Any())
            {
                _logger.LogDebug($"No products has been found in the database");

                return null;
            }

            if (pageNumber == null || pageSize == null)
            {
                return _mapper.Map<IEnumerable<ProductResponseDto>>(productsFromDb);
            }

            IEnumerable<Product> paginatedProducts = productsFromDb.Skip(((int)pageNumber - 1) * (int)pageSize).Take((int)pageSize);
            
            // Returns null, if no products has been found for the requested page number and page size
            if (!paginatedProducts.Any())
            {
                _logger.LogDebug($"No product records has been found for PageNumber: {pageNumber}, PageSize: {pageSize}");

                return null;
            }

            // Adds all the products for the requested page number and page size to cache
            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            _cache.Set(cacheKey, paginatedProducts, cacheEntryOptions);

            return _mapper.Map<IEnumerable<ProductResponseDto>>(paginatedProducts);
        }
    }
}