using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;


namespace Grocery.Core.Data.Repositories
{
    public class ProductRepository : DatabaseConnection, IProductRepository
    {
        private readonly List<Product> products = [];
        public ProductRepository()
        {
            CreateTable(@"CREATE TABLE IF NOT EXISTS Product (
                            [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            [Name] TEXT NOT NULL,
                            [Stock] INTEGER NOT NULL,
                            [Date] DATE NOT NULL,
                            [Price] FLOAT NOT NULL)");
            List<string> insertQueries = [@"INSERT OR IGNORE INTO Product(Id, Name, Stock, Date, Price) VALUES(1, 'Melk', 300, '2025-09-25', 0.95)",
                                          @"INSERT OR IGNORE INTO Product(Id, Name, Stock, Date, Price) VALUES(2, 'Kaas', 100, '2025-09-30', 7.98)",
                                          @"INSERT OR IGNORE INTO Product(Id, Name, Stock, Date, Price) VALUES(3, 'Brood', 400, '2025-09-12', 2.19)",
                                          @"INSERT OR IGNORE INTO Product(Id, Name, Stock, Date, Price) VALUES(4, 'Cornflakes', 0, '2025-12-31', 1.48)"];
            InsertMultipleWithTransaction(insertQueries);
            GetAll();
        }
        public List<Product> GetAll()
        {
            products.Clear();
            string selectQuery = "SELECT Id, Name, Stock, date(Date), Price FROM Product";
            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    int stock = reader.GetInt32(2);
                    DateOnly shelfLife = DateOnly.FromDateTime(reader.GetDateTime(3));
                    Decimal price = reader.GetDecimal(4);
                    products.Add(new(id, name, stock, shelfLife, price));
                }
            }
            CloseConnection();
            return products;
        }

        public Product? Get(int id)
        {
            string selectQuery = $"SELECT Id, Name, Stock, date(Date), Price FROM Product WHERE Id = {id}";
            Product? p = null;
            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    int productId = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    int stock = reader.GetInt32(2);
                    DateOnly shelfLife = DateOnly.FromDateTime(reader.GetDateTime(3));
                    Decimal price = reader.GetDecimal(4);
                    p = new(productId, name, stock, shelfLife, price);
                }
            }
            CloseConnection();
            return p;
        }

        public Product Add(Product item)
        {
            string insertQuery = "INSERT INTO Product(Name, Stock, Date, Price) VALUES(@Name, @Stock, @Date, @Price)";
            OpenConnection();
            using (SqliteCommand command = new(insertQuery, Connection))
            {
                command.Parameters.AddWithValue("@Name", item.Name);
                command.Parameters.AddWithValue("@Stock", item.Stock);
                command.Parameters.AddWithValue("@Date", item.ShelfLife.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@Price", item.Price);

                command.ExecuteNonQuery();
                
                // Get the last inserted ID
                command.CommandText = "SELECT last_insert_rowid()";
                item.Id = Convert.ToInt32(command.ExecuteScalar());
            }
            CloseConnection();
            
            return item;
        }

        public Product? Delete(Product item)
        {
            throw new NotImplementedException();
        }

        public Product? Update(Product item)
        {
            Product? product = products.FirstOrDefault(p => p.Id == item.Id);
            if (product == null) return null;
            product.Id = item.Id;
            return product;
        }
    }
}
