using System;
using System.Data.SQLite;

class Vulnerable
{
    static void Main(string[] args)
    {
        // Open an in-memory database. So anyone can run it.
        using (SQLiteConnection connection = new SQLiteConnection("Data Source=:memory:"))
        {
            connection.Open();

            // Creates the table "users" with columns "first_name", "last_name", "email" and "password".
            // Values are then set to get data to dump.
            string createTableQuery = "CREATE TABLE users (user_id INTEGER PRIMARY KEY, first_name TEXT, last_name TEXT, email TEXT, password TEXT)";
            string insertDataQuery = @"
                INSERT INTO users (first_name, last_name, email, password) VALUES ('John', 'Doe', 'jdoe@1337.com', 'SuperJohn1212');
                INSERT INTO users (first_name, last_name, email, password) VALUES ('Jane', 'Smith', 'JaneButNotMary@spidey.com', 'SpideySenseFirst58');
                INSERT INTO users (first_name, last_name, email, password) VALUES ('Alice', 'Johnson', 'GDPREmail@securedomain.verysecure', 'B$o$o$m$B$i$d$i$B$o$u$m$B$a$m$P$o$w$');
            ";


            using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            using (SQLiteCommand command = new SQLiteCommand(insertDataQuery, connection))
            {
                command.ExecuteNonQuery();
            }


            // Now here's the super secure code

            bool validInput = false;

            while (true)
            {
                // "Indeed I use a try / catch to make sure everything is under control"
                try
                {

                    // "I then a User ID to request in the DB to the user"
                    Console.WriteLine("Enter User ID: ");
                    string userInput = Console.ReadLine();

                    if (userInput.ToLower() == "exit")
                    {
                        Console.WriteLine("Exiting the program...");
                        break;
                    }

                    // "The given User ID is then added to my request. But I made sure to only get the first_name and last_name columns so sensitive data cannot be leak :D"
                    string query = $"SELECT first_name, last_name FROM users WHERE user_id = {userInput}";


                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            // "I make sure to only return data if there's at least one !" 
                            if (!reader.HasRows)
                            {
                                Console.WriteLine("No results found.");
                            }

                            // "Now I loop through each entry, this way I can build the string to be returned with all the data at once ! Now THAT is efficiency !"
                            while (reader.Read())
                            {
                                // Construction dynamique de l'affichage des colonnes
                                Console.WriteLine($"Results for user_id = {userInput}:");
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string columnName = reader.GetName(i); // "Column name. But anyway my SQL request only gets first_name and last_name lol"
                                    string columnValue = reader[i].ToString(); // "Now I retrieve the column value, names aren't sensitive anyway"
                                    Console.WriteLine($"{columnName}: {columnValue}"); // "I then print the data only once <:D>"
                                }
                            }
                        }
                    }

                    
                    validInput = true;
                }
                catch (SQLiteException ex)
                {
                    Console.WriteLine($"SQL Error: {ex.Message}");
                    Console.WriteLine("Please enter a valid User ID.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}