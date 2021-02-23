using Microsoft.Data.SqlClient;
using System;

namespace IncreaseAgeStoredProcedure
{
    class Program
    {
        static void Main(string[] args)
        {
            var minionId = int.Parse(Console.ReadLine());

            var connection = new SqlConnection(
                "Server=.\\SQLEXPRESS;Integrated Security=true;Database=MinionsDB");

            using (connection)
            {
                connection.Open();

                var storedProcCmd = new SqlCommand(
                    $@"EXEC dbo.usp_GetOlder {minionId}",
                    connection);

                storedProcCmd.ExecuteNonQuery();

                var selectMinionCmd = new SqlCommand(
                    $@"SELECT [Name], Age
                       FROM Minions
                       WHERE Id = {minionId}",
                    connection);

                using (var reader = selectMinionCmd.ExecuteReader())
                {
                    reader.Read();
                    Console.WriteLine($"{reader["Name"]} - {reader["Age"]} years old");
                }
            }
        }
    }
}
