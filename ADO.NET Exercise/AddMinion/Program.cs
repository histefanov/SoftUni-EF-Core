using Microsoft.Data.SqlClient;
using System;

namespace AddMinion
{
    class Program
    {
        static void Main(string[] args)
        {
            var minionData = Console.ReadLine()
                .Split(": ")[1]
                .Split();
            var villain = Console.ReadLine().Split(": ")[1];

            var minionName = minionData[0];
            var minionAge = int.Parse(minionData[1]);
            var minionTown = minionData[2];

            var connection = new SqlConnection(
                "Server=.\\SQLEXPRESS;Integrated Security=true;Database=MinionsDB");

            using (connection)
            {
                connection.Open();
                var townsCountCmd = new SqlCommand(
                    $@"SELECT Count(*)
                       FROM Towns",
                    connection);

                var villainsCountCmd = new SqlCommand(
                    $@"SELECT Count(*)
                       FROM Villains",
                    connection);

                var minionsCountCmd = new SqlCommand(
                    $@"SELECT Count(*)
                       FROM Minions",
                    connection);

                var initialTownsCount = (int)townsCountCmd.ExecuteScalar();
                var initialVillainsCount = (int)villainsCountCmd.ExecuteScalar();
                var initialMinionsCount = (int)minionsCountCmd.ExecuteScalar();

                var transactionCmdString =
                    $@"BEGIN TRANSACTION
	                    IF NOT EXISTS(SELECT *
				                      FROM Towns
				                      WHERE [Name] = '{minionTown}')
		                    INSERT INTO Towns([Name])
			                    VALUES
			                    ('{minionTown}');

	                    IF NOT EXISTS(SELECT *
				                      FROM Villains
				                      WHERE [Name] = '{villain}')
		                    INSERT INTO Villains([Name], EvilnessFactorId)
			                    VALUES
			                    ('{villain}', 4);

	                    DECLARE @TownId INT = (SELECT Id FROM Towns WHERE [Name] = '{minionTown}');

	                    INSERT INTO Minions([Name], Age, TownID)
		                    VALUES
		                    ('{minionName}', {minionAge}, @TownId);

	                    DECLARE @MinionId INT = (SELECT TOP(1) Id FROM Minions ORDER BY Id DESC);
	                    DECLARE @VillainId INT = (SELECT Id FROM Villains WHERE [Name] = '{villain}');

	                    INSERT INTO MinionsVillains(MinionId, VillainId)
		                    VALUES(@MinionId, @VillainId);
                    COMMIT";

                var cmd = new SqlCommand(transactionCmdString, connection);
                cmd.ExecuteNonQuery();

                var finalTownsCount = (int)townsCountCmd.ExecuteScalar();
                var finalVilainsCount = (int)villainsCountCmd.ExecuteScalar();
                var finalMinionsCount = (int)minionsCountCmd.ExecuteScalar();

                if (initialTownsCount < finalTownsCount)
                {
                    Console.WriteLine($"Town {minionTown} was added to the database.");
                }

                if (initialVillainsCount < finalVilainsCount)
                {
                    Console.WriteLine($"Villain {villain} was added to the database.");
                }

                if (initialMinionsCount < finalMinionsCount)
                {
                    Console.WriteLine($"Successfully added {minionName} to be minion of {villain}.");
                }
            }
        }
    }
}
