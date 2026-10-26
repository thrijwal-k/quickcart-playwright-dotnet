using Microsoft.Data.Sqlite;

namespace QuickCart.Tests.Support;

/// <summary>
/// An isolated SQLite reference database, seeded from the Excel test data, that UI and API
/// tests assert against with plain SQL. Each test run gets its own in-memory copy.
/// </summary>
public static class ReferenceDatabase
{
    private static readonly Lazy<SqliteConnection> Connection = new(CreateAndSeed);

    public static IReadOnlyList<ProductRow> QueryProducts(string whereClause = "1 = 1")
    {
        using var command = Connection.Value.CreateCommand();
        command.CommandText =
            $"SELECT id, name, category, price, stock FROM products WHERE {whereClause} ORDER BY id";

        var rows = new List<ProductRow>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new ProductRow(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetDouble(3),
                reader.GetInt32(4)));
        }

        return rows;
    }

    public static long Count(string sql)
    {
        using var command = Connection.Value.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static SqliteConnection CreateAndSeed()
    {
        var connection = new SqliteConnection("Data Source=quickcart-reference;Mode=Memory;Cache=Shared");
        connection.Open();

        using (var create = connection.CreateCommand())
        {
            create.CommandText = """
                CREATE TABLE IF NOT EXISTS products (
                    id INTEGER PRIMARY KEY,
                    name TEXT NOT NULL,
                    category TEXT NOT NULL,
                    price REAL NOT NULL CHECK (price > 0),
                    stock INTEGER NOT NULL CHECK (stock >= 0)
                );
                DELETE FROM products;
                """;
            create.ExecuteNonQuery();
        }

        foreach (var product in ExcelTestData.GetProducts())
        {
            using var insert = connection.CreateCommand();
            insert.CommandText =
                "INSERT INTO products (id, name, category, price, stock) VALUES ($id, $name, $category, $price, $stock)";
            insert.Parameters.AddWithValue("$id", product.Id);
            insert.Parameters.AddWithValue("$name", product.Name);
            insert.Parameters.AddWithValue("$category", product.Category);
            insert.Parameters.AddWithValue("$price", product.Price);
            insert.Parameters.AddWithValue("$stock", product.Stock);
            insert.ExecuteNonQuery();
        }

        using (var count = connection.CreateCommand())
        {
            count.CommandText = "SELECT COUNT(*) FROM products";
            TestLog.Logger.Information("Reference database seeded with {Count} products", Convert.ToInt64(count.ExecuteScalar()));
        }

        return connection;
    }
}
