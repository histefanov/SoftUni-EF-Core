using SoftUni.Data;
using System;
using System.Linq;
using System.Text;

namespace SoftUni
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var db = new SoftUniContext();
            var result = GetEmployeesFullInformation(db);
            Console.WriteLine(result);
        }

        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            var employees = context.Employees
                .Select(x => new
                {
                    x.EmployeeId,
                    x.FirstName,
                    x.LastName,
                    x.MiddleName,
                    x.JobTitle,
                    x.Salary
                })
                .OrderBy(x => x.EmployeeId)            
                .ToList();

            var sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine(
                    $"{employee.FirstName} {employee.LastName} {employee.MiddleName} {employee.JobTitle} {employee.Salary:F2}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
