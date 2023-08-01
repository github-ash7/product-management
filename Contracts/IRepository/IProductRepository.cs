using Entities.DTOs;
using Entities.Models;

namespace Contracts.IRepository
{
    public interface IProductRepository
    {
        /// <summary>
        /// This function checks if a collection of product names already exists in a database and returns a
        /// HashSet of existing product names.
        /// </summary>
        /// <param name="products">An ICollection of ProductCreateDTO objects, which contains information about
        /// the products to be checked for existing names.</param>
        /// <returns>
        /// A `HashSet<string>` containing the names of existing products that match the names of the products
        /// in the `products` collection.
        /// </returns>
        Task<HashSet<string>> IsProductNameExistsAsync(ICollection<ProductCreateDto> products);

        /// <summary>
        /// This function inserts a collection of new products into a SQL database using bulk copy.
        /// </summary>
        /// <param name="newProducts">A collection of ProductCreateDTO objects representing the new products to
        /// be inserted into the database.</param>
        Task BulkInsertProductsAsync(ICollection<ProductCreateDto> newProducts);

        /// <summary>
        /// This function retrieves a product from a database by its ID asynchronously.
        /// </summary>
        /// <param name="Guid">A unique identifier that represents a specific product in the database. It is
        /// used as a parameter to retrieve the product information from the database.</param>
        /// <returns>
        /// The method returns a `Task` that may contain a `Product` object if a product with the specified
        /// `productID` is found in the database, or `null` if no such product exists.
        /// </returns>
        Task<Product?> GetProductByIdAsync(Guid productID);

        /// <summary>
        /// This function retrieves all products from a database asynchronously and returns them as a list of
        /// Product objects.
        /// </summary>
        /// <returns>
        /// The method is returning an asynchronous task that returns an IEnumerable of Product objects.
        /// </returns>
        Task<IEnumerable<Product>> GetAllProductsAsync();
    }
}