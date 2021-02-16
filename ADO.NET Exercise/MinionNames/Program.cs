using Microsoft.Data.SqlClient;
using System;

namespace MinionNames
{
    class Program
    {
        static void Main(string[] args)
        {
            var connection = new SqlConnection(
                "Server=.\\SQLEXPRESS;Integrated Security=true;Database=MinionsDB");

            Console.WriteLine("Please enter Villain ID:");
            var villainId = int.Parse(Console.ReadLine());

            using (connection)
            {
                connection.Open();

                var villainNameCmd = new SqlCommand(
                    $@"SELECT [Name]
                       FROM Villains
                       WHERE Id = {villainId}",
                    connection);

                var minionsCmd = new SqlCommand(
                    $@"SELECT [Name], Age
                       FROM Minions m
                           JOIN MinionsVillains mv ON m.Id = mv.MinionId
                       WHERE mv.VillainId = {villainId}
                       ORDER BY [Name]",
                    connection);

                string villainName = (string)villainNameCmd.ExecuteScalar();

                if (villainName == null)
                {
                    Console.WriteLine($"No villain with ID {villainId} exists in the database.");
                }
                else
                {
                    Console.WriteLine($"Villain: {villainName}");

                    using (var minionsReader = minionsCmd.ExecuteReader())
                    {
                        if (minionsReader.HasRows)
                        {
                            int rowNumber = 1;
                            while (minionsReader.Read())
                            {
                                Console.WriteLine($"{rowNumber}. {minionsReader["Name"]} {minionsReader["Age"]}");
                                rowNumber++;
                            }
                        }
                        else
                        {
                            Console.WriteLine("(no minions)");
                        }
                    }
                }
            }
        }
    }
}
