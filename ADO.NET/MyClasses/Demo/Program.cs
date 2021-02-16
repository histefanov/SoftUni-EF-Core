using Microsoft.Data.SqlClient;
using System;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            // Open connection:
            string connectionString = "Server=.\\SQLEXPRESS;Integrated Security=true;Database=SoftUni";
            var connection = new SqlConnection(connectionString);
            connection.Open();
        }
    }
}
