using Microsoft.Data.SqlClient;
using System;

namespace Insert
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

                string insertionCmdString = @"USE MinionsDB

											INSERT INTO Countries([Name])
												VALUES
												('Bulgaria'),
												('Switzerland'),
												('Norway'),
												('USA'),
												('Pakistan')

											INSERT INTO EvilnessFactors([Name])
												VALUES
												('Naughty'),
												('Slightly bad'),
												('Malicious'),
												('Evil'),
												('Antichrist')

											INSERT INTO Towns([Name])
												VALUES
												('Silistra'),
												('Zuerich'),
												('Oslo'),
												('Las Vegas'),
												('Islamabad')

											INSERT INTO Minions([Name], Age, TownId)
												VALUES
												('Banana', 5, 4),
												('Bob', 4, 1),
												('Stuart', 6, 3),
												('Kevin', 5, 2),
												('Gosho', 2, 1)

											INSERT INTO Villains([Name], EvilnessFactorId)
												VALUES
												('Gru', 1),
												('Scarlett Overkill', 4),
												('Herb Overkill', 3),
												('Walter Jr', 2),
												('Folabi', 5)

											INSERT INTO MinionsVillains(MinionId, VillainId)
												VALUES
												(1, 2),
												(2, 1),
												(3, 1),
												(4, 1),
												(5, 5)";

				var cmd = new SqlCommand(insertionCmdString, connection);
                Console.WriteLine(cmd.ExecuteNonQuery());
            }
        }
    }
}
