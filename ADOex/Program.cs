using Microsoft.Data.Sqlite;
using System;

namespace ADOex
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ///Opens a new connection to our sqlite db in file HelloWorld.db
            using (var connection = new SqliteConnection(
                new SqliteConnectionStringBuilder { DataSource = "HelloWorld.db" }.ToString()))
            {
                connection.Open();
                String tblExists = "SELECT count(*) FROM sqlite_master WHERE type = 'table' AND name = 'People'";
                var checkTableExistsCmd = connection.CreateCommand();
                checkTableExistsCmd.CommandText = tblExists;
                var count = checkTableExistsCmd.ExecuteScalar();
                if ((Int64)count <= 0)
                {
                    var CreateTableCmd = connection.CreateCommand();
                    CreateTableCmd.CommandText = "Create table People (ID INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE, FirstName TEXT,LastName TEXT)";
                    CreateTableCmd.ExecuteNonQuery();
                }

                Console.Write("Enter a name: ");
                String Name = Console.ReadLine();

                String[] SplitName = Name.Split(" ");
                if (SplitName.Length <= 1)
                    throw new Exception("Not a full name dummy!");
                using (var transaction = connection.BeginTransaction())
                {
                    //Query to see if person exists in db...
                    var selectCommand = connection.CreateCommand();
                    selectCommand.Transaction = transaction;
                    selectCommand.CommandText = "SELECT * FROM People where FirstName=$first and LastName=$last";
                    selectCommand.Parameters.AddWithValue("$first", SplitName[0]);
                    selectCommand.Parameters.AddWithValue("$last", SplitName[1]);
                    bool found = false;
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        //returns true if there is a row, and each subsequent call
                        //reads the next row of the resultset.
                        while (reader.Read())
                        {
                            found = true; //by definition we found a row
                            var FirstName = reader["FirstName"].ToString();
                            var LastName = reader["LastName"].ToString();
                            var ID = reader["ID"];
                            Console.WriteLine(ID + ". " + FirstName + " " + LastName);
                        }
                    }
                    //if we didnt find the person, insert them...
                    if (!found)
                    {
                        var insertCommand = connection.CreateCommand();
                        insertCommand.Transaction = transaction;
                        insertCommand.CommandText = "INSERT INTO People ( FirstName,LastName) VALUES ( $First,$Last )";
                        insertCommand.Parameters.AddWithValue("$First", SplitName[0]);
                        insertCommand.Parameters.AddWithValue("$Last", SplitName[1]);
                        insertCommand.ExecuteNonQuery();

                    }
                    transaction.Commit();
                }
            }

        }
    }
}
