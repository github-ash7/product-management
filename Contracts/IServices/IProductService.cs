using Entities.DTOs;

namespace Contracts.IServices
{
    public interface IProductService
    {
        /// <summary>
        /// This function adds a collection of products to a database after checking for duplicate names in the given
        /// request body and existing products.
        /// </summary>
        /// <param name="products">An ICollection of ProductCreateDTO objects, which contain information about
        /// the products to be added to the database.</param>
        Task AddProductsToDBAsync(ICollection<ProductCreateDto> products);

        /// <summary>
        /// This function retrieves product information by ID, first checking if it's in cache and returning it
        /// if so, otherwise querying the database and caching the result for future requests.
        /// </summary>
        /// <param name="Guid">A unique identifier for a product, used to retrieve product information from the
        /// database or cache.</param>
        /// <returns>
        /// A `Task` that returns a `ProductResponseDto` object.
        /// </returns>
        Task<ProductResponseDto> GetProductByIDAsync(Guid productID);

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
        Task<IEnumerable<ProductResponseDto>?> GetAllProductsAsync(int? pageNumber, int? pageSize);
    }
}