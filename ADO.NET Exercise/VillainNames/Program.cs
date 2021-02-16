using Microsoft.Data.SqlClient;
using System;

namespace VillainNames
{
    class Program
    {
        static void Main(string[] args)
        {
            var connection = new SqlConnection(
                "Server=.\\SQLEXPRESS;Integrated Security=true;Database=MinionsDB");

            using (connection)
            {
                connection.Open();
                string cmdString = @"SELECT 
                                        v.[Name], COUNT(*) AS MinionCount 
                                     FROM Villains v
                                        JOIN MinionsVillains mv ON v.Id = mv.VillainId
                                        JOIN Minions m ON mv.MinionId = m.Id
                                     GROUP BY v.[Name]
                                     ORDER BY MinionCount DESC";

                var sqlCmd = new SqlCommand(cmdString, connection);

                using (SqlDataReader sqlReader = sqlCmd.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        Console.WriteLine($"{sqlReader["Name"]} - {sqlReader["MinionCount"]}");
                    }
                }
            }
        }
    }
}
