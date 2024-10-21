using System;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;

namespace SecureApp
{
    // Database interaction layer
    public class Database : IDisposable // Implement IDisposable
    {
        private readonly SQLiteConnection _connection;
        private bool _disposed = false; // To track whether Dispose has been called

        public Database()
        {
            // Initialize an in-memory SQLite database
            _connection = new SQLiteConnection("Data Source=:memory:");
            _connection.Open();
            InitializeDatabase();
        }

        // Setup the database schema and insert sample data
        private void InitializeDatabase()
        {
            const string createTableQuery = @"
                CREATE TABLE users (
                    user_id INTEGER PRIMARY KEY, 
                    first_name TEXT, 
                    last_name TEXT, 
                    email TEXT, 
                    password TEXT)";

            const string insertDataQuery = @"
                INSERT INTO users (first_name, last_name, email, password) 
                VALUES 
                ('John', 'Doe', 'jdoe@1337.com', @Password1),
                ('Jane', 'Smith', 'JaneButNotMary@spidey.com', @Password2),
                ('Alice', 'Johnson', 'GDPREmail@securedomain.verysecure', @Password3)";

            using (var command = new SQLiteCommand(createTableQuery, _connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SQLiteCommand(insertDataQuery, _connection))
            {
                // Hash passwords before storing them
                command.Parameters.AddWithValue("@Password1", HashPassword("SuperJohn1212"));
                command.Parameters.AddWithValue("@Password2", HashPassword("SpideySenseFirst58"));
                command.Parameters.AddWithValue("@Password3", HashPassword("B$o$o$m$B$i$d$i$B$o$u$m$B$a$m$P$o$w$"));
                command.ExecuteNonQuery();
            }
        }

        // Hash a password using SHA256 (Use bcrypt or PBKDF2 in production)
        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var byteValue in bytes)
                {
                    builder.Append(byteValue.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Retrieve user information by user ID, avoiding SQL injection
        public (string FirstName, string LastName) GetUserById(int userId)
        {
            const string query = "SELECT first_name, last_name FROM users WHERE user_id = @UserId";
            using (var command = new SQLiteCommand(query, _connection))
            {
                // Use parameterized query to prevent SQL injection
                command.Parameters.AddWithValue("@UserId", userId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return (reader["first_name"].ToString(), reader["last_name"].ToString());
                    }
                }
            }
            return (null, null); // Return null if no user is found
        }

        // IDisposable implementation to ensure the connection is closed
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // Avoids calling the finalizer
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Close the database connection if it's still open
                    _connection?.Close();
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }

        ~Database()
        {
            Dispose(false); // Finalizer calls Dispose(false) in case Dispose() wasn't called manually
        }
    }

    // Main application class
    public class Patched
    {
        public static void Main(string[] args)
        {
            using (var db = new Database()) // Ensures proper cleanup of the database connection
            {
                while (true)
                {
                    Console.WriteLine("Enter User ID (or type 'exit' to quit): ");
                    var input = Console.ReadLine();

                    if (input?.ToLower() == "exit")
                    {
                        Console.WriteLine("Exiting the program...");
                        break;
                    }

                    // Validate the input and ensure it's a valid integer
                    if (int.TryParse(input, out var userId) && userId > 0)
                    {
                        try
                        {
                            // Fetch user data by user ID
                            var (firstName, lastName) = db.GetUserById(userId);

                            if (firstName == null || lastName == null)
                            {
                                Console.WriteLine("No user found with the given ID.");
                            }
                            else
                            {
                                Console.WriteLine($"User ID: {userId}, Name: {firstName} {lastName}");  // yuknow
                            }
                        }
                        catch (SQLiteException ex)
                        {
                            // Log the detailed error internally, but show a generic message to the user
                            Console.WriteLine("An error occurred while accessing the database. Please try again.");
                            LogError(ex); // Internal logging
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("An unexpected error occurred.");
                            LogError(ex); // Internal logging
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a valid numeric User ID.");
                    }
                }
            }
        }

        // Log detailed errors for internal use (could be extended to log to a file or external system)
        private static void LogError(Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            // In a real system, we would log this to a file or monitoring service
        }
    }
}
