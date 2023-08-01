using System.Data;
using Entities.Models;
using Microsoft.Data.SqlClient;

namespace ProductManagementTests
{
    public static class TestingTableManager
    {
        const string connectionString = "Data Source=192.168.11.77;Initial Catalog=devtraining;TrustServerCertificate=true;User ID=hariharan.s;Password=hari@7708";

        public static void InsertRecords()
        {
            List<Product> products = new List<Product>();

            products.Add
            (
                new Product
                {
                    ID = Guid.Parse("5784d3df-e2da-4be7-b6e7-4a17d51ec2ac"),
                    ProductName = "Google Pixel 7 Pro (128 GB Storage, 12 GB RAM)",
                    CategoryID = Guid.Parse("63d14238-8362-4242-a4a9-ef2d9b1ce7e8"),
                    SupplierID = Guid.Parse("235f43c8-6202-47d1-9954-154f0607191b"),
                    UnitPrice = 80000,
                    UnitsInStock = 10000,
                    Discontinued = false
                }
            );

            products.Add
            (
                new Product
                {
                    ID = Guid.Parse("330b834a-ce6a-4db4-86f6-9f6ae94b8280"),
                    ProductName = "Spigen Liquid Case for Google Pixel 7 Pro",
                    CategoryID = Guid.Parse("6258e123-bd24-441c-857f-cb2764ecf8f7"),
                    SupplierID = Guid.Parse("eadfe4c9-731d-4556-978c-2f9105a1550b"),
                    UnitPrice = 1000,
                    UnitsInStock = 1000,
                    Discontinued = false
                }
            );
            
            products.Add
            (
                new Product
                {
                    ID = Guid.Parse("120af4f8-69f8-4a41-9c3e-dd2357b4baee"),
                    ProductName = "Samsung Galaxy Note 20",
                    CategoryID = Guid.Parse("6258e123-bd24-441c-857f-cb2764ecf8f7"),
                    SupplierID = Guid.Parse("eadfe4c9-731d-4556-978c-2f9105a1550b"),
                    UnitPrice = 120000,
                    UnitsInStock = 0,
                    Discontinued = true
                }
            );

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (Product product in products)
                {
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "INSERT INTO assignment_product_management.product_test (id, product_name, category_id, supplier_id, unit_price, units_in_stock, discontinued) " +
                                          $"VALUES (@ID, @ProductName, @CategoryID, @SupplierID, @UnitPrice, @UnitsInStock, @Discontinued)";

                    command.Parameters.AddWithValue("@ID", product.ID);
                    command.Parameters.AddWithValue("@ProductName", product.ProductName);
                    command.Parameters.AddWithValue("@CategoryID", product.CategoryID);
                    command.Parameters.AddWithValue("@SupplierID", product.SupplierID);
                    command.Parameters.AddWithValue("@UnitPrice", product.UnitPrice);
                    command.Parameters.AddWithValue("@UnitsInStock", product.UnitsInStock);
                    command.Parameters.AddWithValue("@Discontinued", product.Discontinued);

                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        public static void DeleteAllRecords()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("DELETE FROM assignment_product_management.product_test", connection);
                command.ExecuteNonQuery();
            }
        }
    }
}