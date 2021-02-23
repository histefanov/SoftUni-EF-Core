using Microsoft.Data.SqlClient;
using System;

namespace RemoveVillain
{
    class Program
    {
        static void Main(string[] args)
        {
            var villainId = int.Parse(Console.ReadLine());
            var connection = new SqlConnection(
                "Server=.\\SQLEXPRESS;Integrated Security=true;Database=MinionsDB");

            using (connection)
            {
                connection.Open();

                var villainName = new SqlCommand(
                    $@"SELECT [Name]
                       FROM Villains
                       WHERE Id = {villainId}",
                    connection)
                        .ExecuteScalar() as string;

                if (villainName != null)
                {
                    var minionCountCmd = new SqlCommand(
                        $@"SELECT Count(*)
                           FROM MinionsVillains
                           WHERE VillainId = {villainId}",
                        connection);

                    var initialMinionsCount = (int)minionCountCmd.ExecuteScalar();

                    var deleteCmd = new SqlCommand(
                        $@"BEGIN TRANSACTION
	                        DELETE FROM MinionsVillains
	                        WHERE VillainId = {villainId};

	                        DELETE FROM Villains
	                        WHERE Id = {villainId};
                        COMMIT",
                        connection);

                    deleteCmd.ExecuteNonQuery();

                    var finalMinionsCount = (int)minionCountCmd.ExecuteScalar();

                    Console.WriteLine($"{villainName} was deleted.");
                    Console.WriteLine($"{initialMinionsCount - finalMinionsCount} minions were released.");
                }
                else
                {
                    Console.WriteLine("No such villain was found.");
                }
            }
        }
    }
}
