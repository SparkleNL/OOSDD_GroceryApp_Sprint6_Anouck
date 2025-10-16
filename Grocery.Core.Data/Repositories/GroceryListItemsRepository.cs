using Grocery.Core.Data.Helpers;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;

namespace Grocery.Core.Data.Repositories
{
    public class GroceryListItemsRepository : DatabaseConnection, IGroceryListItemsRepository
    {
        private readonly List<GroceryListItem> groceryListItems = [];

        public GroceryListItemsRepository()
        {
            CreateTable(@"CREATE TABLE IF NOT EXISTS GroceryListItem (
                            [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            [GroceryListId] INTEGER NOT NULL,
                            [ProductId] INTEGER NOT NULL,
                            [Amount] INTEGER NOT NULL)");
            List<string> insertQueries = [
                @"INSERT OR IGNORE INTO GroceryListItem(Id, GroceryListId, ProductId, Amount) VALUES(1, 1, 1, 3)",
                @"INSERT OR IGNORE INTO GroceryListItem(Id, GroceryListId, ProductId, Amount) VALUES(2, 1, 2, 1)",
                @"INSERT OR IGNORE INTO GroceryListItem(Id, GroceryListId, ProductId, Amount) VALUES(3, 1, 3, 4)",
                @"INSERT OR IGNORE INTO GroceryListItem(Id, GroceryListId, ProductId, Amount) VALUES(4, 2, 1, 2)",
                @"INSERT OR IGNORE INTO GroceryListItem(Id, GroceryListId, ProductId, Amount) VALUES(5, 2, 2, 5)"
            ];
            InsertMultipleWithTransaction(insertQueries);
            GetAll();
        }

        public List<GroceryListItem> GetAll()
        {
            groceryListItems.Clear();
            string selectQuery = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem";
            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    int groceryListId = reader.GetInt32(1);
                    int productId = reader.GetInt32(2);
                    int amount = reader.GetInt32(3);
                    groceryListItems.Add(new(id, groceryListId, productId, amount));
                }
            }
            CloseConnection();
            return groceryListItems;
        }

        public List<GroceryListItem> GetAllOnGroceryListId(int id)
        {
            List<GroceryListItem> items = [];
            string selectQuery = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem WHERE GroceryListId = @GroceryListId";
            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                command.Parameters.AddWithValue("GroceryListId", id);
                SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int itemId = reader.GetInt32(0);
                    int groceryListId = reader.GetInt32(1);
                    int productId = reader.GetInt32(2);
                    int amount = reader.GetInt32(3);
                    items.Add(new(itemId, groceryListId, productId, amount));
                }
            }
            CloseConnection();
            return items;
        }

        public GroceryListItem Add(GroceryListItem item)
        {
            string insertQuery = @"INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(@GroceryListId, @ProductId, @Amount) RETURNING RowId;";
            OpenConnection();
            using (SqliteCommand command = new(insertQuery, Connection))
            {
                command.Parameters.AddWithValue("GroceryListId", item.GroceryListId);
                command.Parameters.AddWithValue("ProductId", item.ProductId);
                command.Parameters.AddWithValue("Amount", item.Amount);

                item.Id = Convert.ToInt32(command.ExecuteScalar());
            }
            CloseConnection();
            return item;
        }

        public GroceryListItem? Delete(GroceryListItem item)
        {
            string deleteQuery = $"DELETE FROM GroceryListItem WHERE Id = {item.Id};";
            OpenConnection();
            Connection.ExecuteNonQuery(deleteQuery);
            CloseConnection();
            return item;
        }

        public GroceryListItem? Get(int id)
        {
            string selectQuery = $"SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem WHERE Id = {id}";
            GroceryListItem? p = null;
            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    int itemId = reader.GetInt32(0);
                    int groceryListId = reader.GetInt32(1);
                    int productId = reader.GetInt32(2);
                    int amount = reader.GetInt32(3);
                    p = new(itemId, groceryListId, productId, amount);
                }
            }
            CloseConnection();
            return p;
        }

        public GroceryListItem? Update(GroceryListItem item)
        {
            int recordsAffected;
            string updateQuery = $"UPDATE GroceryListItem SET GroceryListId = @GroceryListId, ProductId = @ProductId, Amount = @Amount WHERE Id = {item.Id};";
            OpenConnection();
            using (SqliteCommand command = new(updateQuery, Connection))
            {
                command.Parameters.AddWithValue("GroceryListId", item.GroceryListId);
                command.Parameters.AddWithValue("ProductId", item.ProductId);
                command.Parameters.AddWithValue("Amount", item.Amount);

                recordsAffected = command.ExecuteNonQuery();
            }
            CloseConnection();
            return item;
        }
    }
}