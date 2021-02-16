using Microsoft.Data.SqlClient;
using System;

namespace Initial_Setup
{
    class Program
    {
        static void Main(string[] args)
        {
            var connection = new SqlConnection(
                "Server=.\\SQLEXPRESS;Integrated Security=true;Database=master");

            using (connection)
            {
                connection.Open();

				string dbCreationCmdString = @"CREATE DATABASE MinionsDB";
				string tablesCreationCmdString =
                                        @"USE MinionsDB

                                        CREATE TABLE Countries
                                        (
                                            Id INT PRIMARY KEY IDENTITY,
                                            [Name] VARCHAR(25) NOT NULL
										)

										CREATE TABLE Towns
                                        (
                                            Id INT PRIMARY KEY IDENTITY,

                                            [Name] VARCHAR(25) NOT NULL,
                                            CountryId INT FOREIGN KEY REFERENCES Countries(Id)
                                        )


                                        CREATE TABLE EvilnessFactors
                                        (
                                            Id INT PRIMARY KEY IDENTITY,

                                            [Name] VARCHAR(50)
                                        )


                                        CREATE TABLE Minions
                                        (
                                            Id INT PRIMARY KEY IDENTITY,

                                            [Name] VARCHAR(50) NOT NULL,
                                            Age INT NOT NULL,
                                            TownID INT NOT NULL FOREIGN KEY REFERENCES Towns(Id)
                                        )


                                        CREATE TABLE Villains
                                        (
                                            Id INT PRIMARY KEY IDENTITY,

                                            [Name] VARCHAR(50) NOT NULL,
                                            EvilnessFactorId INT NOT NULL FOREIGN KEY REFERENCES EvilnessFactors(Id)
                                        )


                                        CREATE TABLE MinionsVillains
                                        (
                                            MinionId INT NOT NULL FOREIGN KEY REFERENCES Minions(Id),
                                            VillainId INT NOT NULL FOREIGN KEY REFERENCES Villains(Id),
                                            PRIMARY KEY(MinionId, VillainId)
                                        )";

                var dbCreationCmd = new SqlCommand(dbCreationCmdString, connection);
                var tablesCreationCmd = new SqlCommand(tablesCreationCmdString, connection);
                Console.WriteLine(dbCreationCmd.ExecuteNonQuery());
                Console.WriteLine(tablesCreationCmd.ExecuteNonQuery());
			}
        }
    }
}
