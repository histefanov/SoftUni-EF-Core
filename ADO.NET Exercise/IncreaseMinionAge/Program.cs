using Microsoft.Data.SqlClient;
using System;
using System.Linq;

namespace IncreaseMinionAge
{
    class Program
    {
        static void Main(string[] args)
        {
            var minionIds = String.Join(", ", 
                Console.ReadLine()
                .Split()
                .Select(int.Parse)
                .ToArray());

            var connection = new SqlConnection(
                "Server=.\\SQLEXPRESS;Integrated Security=true;Database=MinionsDB");

            using (connection)
            {
                connection.Open();

                var updateCmd = new SqlCommand(
                    $@"UPDATE Minions
                    SET Age += 1,
	                    [Name] = UPPER(LEFT([Name], 1)) +
			                     LOWER(RIGHT([Name], LEN([Name]) - 1))
                    WHERE Id IN ({minionIds})",
                    connection);

                updateCmd.ExecuteNonQuery();

                var selectCmd = new SqlCommand(
                    @"SELECT [Name], Age
                      FROM Minions",
                    connection);

                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine(reader["Name"] + " " + reader["Age"]);
                    }
                }
            }
        }
    }
}
