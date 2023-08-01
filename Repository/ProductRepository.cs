using System.Data;
using AutoMapper;
using Contracts.IRepository;
using Entities.DTOs;
using Entities.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Repository
{
    /// <summary>
    /// Handles all the query operations for the Product table
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly ILogger<ProductRepository> _logger;
        private readonly string? connectionString;
        private readonly string? tableName;
        private readonly IMapper _mapper;

        public ProductRepository(ILogger<ProductRepository> logger, IConfiguration configuration, IMapper mapper)
        {
            _logger = logger;
            connectionString = configuration.GetConnectionString("DefaultConnection");
            tableName = configuration.GetSection("ProductTable:Name").Value;
            _mapper = mapper;
        }

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
        public async Task<HashSet<string>> IsProductNameExistsAsync(ICollection<ProductCreateDto> products)
        {
            _logger.LogDebug("Received request to check if product name(s) already exists");

            HashSet<string> existingProductNames = new HashSet<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;

                    command.CommandText =
                        $"SELECT product_name FROM {tableName} WHERE product_name IN ("
                        + string.Join(",", products.Select(p => $"'{p.ProductName}'"))
                        + ") AND discontinued = 0";

                    SqlDataReader? reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        existingProductNames.Add(reader.GetString(0));
                    }

                    reader.Close();
                }
            }

            _logger.LogDebug("New product(s) list has been validated");

            return existingProductNames;
        }

        /// <summary>
        /// This function inserts a collection of new products into a SQL database using bulk copy.
        /// </summary>
        /// <param name="newProducts">A collection of ProductCreateDTO objects representing the new products to
        /// be inserted into the database.</param>
        public async Task BulkInsertProductsAsync(ICollection<ProductCreateDto> newProducts)
        {
            _logger.LogDebug("Received request to add new product(s) to the database");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        bulkCopy.DestinationTableName = $"{tableName}";

                        var dataTable = new DataTable();
                        dataTable.Columns.Add("id", typeof(Guid));
                        dataTable.Columns.Add("product_name", typeof(string));
                        dataTable.Columns.Add("category_id", typeof(Guid));
                        dataTable.Columns.Add("supplier_id", typeof(Guid));
                        dataTable.Columns.Add("unit_price", typeof(decimal));
                        dataTable.Columns.Add("units_in_stock", typeof(int));
                        dataTable.Columns.Add("discontinued", typeof(bool));

                        foreach (ProductCreateDto product in newProducts)
                        {
                            dataTable.Rows.Add
                            (
                                product.ID == null ? Guid.NewGuid() : product.ID, product.ProductName, product.CategoryID,
                                product.SupplierID, product.UnitPrice,
                                product.UnitsInStock, product.Discontinued
                            );
                        }

                        bulkCopy.BatchSize = 100;
                        await bulkCopy.WriteToServerAsync(dataTable);
                    }
                    transaction.Commit();
                }
            }
            _logger.LogDebug("Successfully added new product(s) to the database");
        }

        /// <summary>
        /// This function retrieves a product from a database by its ID asynchronously.
        /// </summary>
        /// <param name="Guid">A unique identifier that represents a specific product in the database. It is
        /// used as a parameter to retrieve the product information from the database.</param>
        /// <returns>
        /// The method returns a `Task` that may contain a `Product` object if a product with the specified
        /// `productID` is found in the database, or `null` if no such product exists.
        /// </returns>
        public async Task<Product?> GetProductByIdAsync(Guid productID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand($"SELECT * FROM {tableName} WHERE id = @ID AND discontinued = 0", connection))
                {
                    command.Parameters.AddWithValue("@ID", productID);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return _mapper.Map<Product>(reader);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This function retrieves all products from a database asynchronously and returns them as a list of
        /// Product objects.
        /// </summary>
        /// <returns>
        /// The method is returning an asynchronous task that returns an IEnumerable of Product objects.
        /// </returns>
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            List<Product> products = new List<Product>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand($"SELECT * FROM {tableName} WHERE discontinued = 0", connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(_mapper.Map<Product>(reader));
                        }
                    }
                }
            }
            return products;
        }
    }
}
