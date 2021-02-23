using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace ChangeTownNamesCasing
{
    class Program
    {
        static void Main(string[] args)
        {
            var countryName = Console.ReadLine();

            var connection = new SqlConnection
                ("Server=.\\SQLEXPRESS;Integrated Security=true;Database=MinionsDB");

            using (connection)
            {
                connection.Open();

                var townsCountCmd = new SqlCommand(
                    $@"SELECT COUNT(*)
                       FROM Towns t
	                       JOIN Countries c ON t.CountryId = c.Id
                       WHERE c.[Name] = '{countryName}'",
                    connection);

                var townsCount = (int)townsCountCmd.ExecuteScalar();

                if (townsCount > 0)
                {
                    var convertTownsToUpperCmd = new SqlCommand(
                        $@"UPDATE Towns
                        SET [Name] = UPPER([Name])
                        WHERE [Name] IN
	                        (SELECT 
		                        t.[Name]
	                        FROM Towns t
		                        JOIN Countries c ON t.CountryId = c.Id
	                        WHERE c.[Name] = '{countryName}')",
                        connection);

                    convertTownsToUpperCmd.ExecuteNonQuery();

                    var getTownNamesCmd = new SqlCommand(
                        $@"SELECT t.[Name]
                           FROM Towns t
                               JOIN Countries c ON t.CountryId = c.Id
                           WHERE c.[Name] = '{countryName}'",
                        connection);

                    var townsChanged = new List<string>();

                    using (var reader = getTownNamesCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            townsChanged.Add((string)reader["Name"]);
                        }
                    }

                    Console.WriteLine($"{townsCount} town names were affected.");
                    Console.WriteLine($"[{String.Join(", ", townsChanged)}]");
                }
                else
                {
                    Console.WriteLine("No town names were affected.");
                }
            }
        }
    }
}
