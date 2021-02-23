using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace PrintAllMinionNames
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
    
                var selectMinionCmd = new SqlCommand(
                    $@"SELECT 
		                [Name], ROW_NUMBER() OVER (ORDER BY Id) as RowNum
	                FROM Minions",
                    connection);

                var dictionary = new Dictionary<long, string>();

                using (var reader = selectMinionCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dictionary.Add((long)reader["RowNum"], (string)reader["Name"]);
                    }

                    var currentFirstRow = 1;
                    var currentLastRow = dictionary.Count;
                    var difference = 0;

                    while (difference < currentLastRow / 2)
                    {
                        Console.WriteLine(dictionary[currentFirstRow + difference]);
                        Console.WriteLine(dictionary[currentLastRow - difference]);
                        difference++;
                    }

                    if (dictionary.Count % 2 != 0)
                    {
                        Console.WriteLine(dictionary[currentLastRow - difference]);
                    }
                }     
            }
        }
    }
}
