using Microsoft.EntityFrameworkCore;
using SoftUni.Data;
using SoftUni.Models;
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
            var result = RemoveTown(db);
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

        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(x => x.Salary > 50000)
                .Select(x => new { x.FirstName, x.Salary })
                .OrderBy(x => x.FirstName)
                .ToList();

            var sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} - {employee.Salary:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(x => x.Department.Name == "Research and Development")
                .Select(x => new { x.FirstName, x.LastName, DepartmentName = x.Department.Name, x.Salary })
                .OrderBy(x => x.Salary)
                .ThenByDescending(x => x.FirstName)
                .ToList();

            var sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} from {employee.DepartmentName} - ${employee.Salary:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            var newAddress = new Address() { AddressText = "Vitoshka 15", TownId = 4 };

            var targetEmployee = context.Employees
                .Where(x => x.LastName == "Nakov")
                .FirstOrDefault();

            targetEmployee.Address = newAddress;

            context.SaveChanges();

            var addresses = context.Employees
                .Select(x => new { x.AddressId, x.Address.AddressText })
                .OrderByDescending(x => x.AddressId)
                .Take(10)
                .ToList();

            var sb = new StringBuilder();

            foreach (var address in addresses)
            {
                sb.AppendLine(address.AddressText);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            var employees = context.Employees
                .Include(x => x.Manager)
                .Include(x => x.EmployeesProjects)
                .ThenInclude(x => x.Project)
                .Where(x => x.EmployeesProjects
                    .Any(ep => ep.Project.StartDate.Year >= 2001 && ep.Project.StartDate.Year <= 2003))
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    ManagerFn = x.Manager.FirstName,
                    ManagerLn = x.Manager.LastName,
                    Projects = x.EmployeesProjects.Select(p => new
                    {
                        ProjectName = p.Project.Name,
                        p.Project.StartDate,
                        p.Project.EndDate
                    })
                })
                .Take(10)
                .ToList();

            var sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} - Manager: {employee.ManagerFn} {employee.ManagerLn}");

                foreach (var ep in employee.Projects)
                {
                    string startDate = $"{ep.StartDate:M/d/yyyy h:mm:ss tt}";
                    string endDate;

                    if (ep.EndDate == null)
                    {
                        endDate = "not finished";
                    }
                    else
                    {
                        endDate = $"{ep.EndDate:M/d/yyyy h:mm:ss tt}";
                    }

                    sb.AppendLine(
                        $"--{ep.ProjectName} - {startDate} - {endDate}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetAddressesByTown(SoftUniContext context)
        {
            var addresses = context.Addresses
                .Include(a => a.Employees)
                .Select(a => new
                {
                    a.AddressText,
                    TownName = a.Town.Name,
                    EmployeeCount = a.Employees.Count
                })
                .OrderByDescending(a => a.EmployeeCount)
                .ThenBy(a => a.TownName)
                .ThenBy(a => a.AddressText)
                .Take(10)
                .ToList();

            var sb = new StringBuilder();

            foreach (var address in addresses)
            {
                sb.AppendLine($"{address.AddressText}, {address.TownName} - {address.EmployeeCount} employees");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetEmployee147(SoftUniContext context)
        {
            var employee = context.Employees
                .Include(x => x.EmployeesProjects)
                .ThenInclude(x => x.Project)
                .FirstOrDefault(x => x.EmployeeId == 147);

            var sb = new StringBuilder();
            sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle}");

            foreach (var ep in employee.EmployeesProjects.OrderBy(x => x.Project.Name))
            {
                sb.AppendLine(ep.Project.Name);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var departments = context.Departments
                .Where(x => x.Employees.Count > 5)
                .OrderBy(x => x.Employees.Count)
                .ThenBy(x => x.Name)
                .Select(x => new
                {
                    DepartmentName = x.Name,
                    ManagerFirstName = x.Manager.FirstName,
                    ManagerLastName = x.Manager.LastName,
                    Employees = x.Employees.Select(e => new
                    {
                        e.FirstName,
                        e.LastName,
                        e.JobTitle
                    }),
                })
                .ToList();

            var sb = new StringBuilder();

            foreach (var department in departments)
            {
                sb.AppendLine($"{department.DepartmentName} - {department.ManagerFirstName} {department.ManagerLastName}");

                foreach (var employee in department.Employees.OrderBy(x => x.FirstName).ThenBy(x => x.LastName))
                {
                    sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetLatestProjects(SoftUniContext context)
        {
            var projects = context.Projects
                .Select(x => new
                {
                    x.Name,
                    x.Description,
                    x.StartDate
                })
                .OrderByDescending(x => x.StartDate)              
                .Take(10)
                .OrderBy(x => x.Name)
                .ToList();

            var sb = new StringBuilder();

            foreach (var project in projects)
            {
                sb.AppendLine(project.Name)
                    .AppendLine(project.Description)
                    .AppendLine(project.StartDate.ToString("M/d/yyyy h:mm:ss tt"));
            }

            return sb.ToString().TrimEnd();
        }

        public static string IncreaseSalaries(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(x => x.Department.Name == "Engineering" ||
                            x.Department.Name == "Tool Design" ||
                            x.Department.Name == "Marketing" ||
                            x.Department.Name == "Information Services")
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName);

            var sb = new StringBuilder();

            foreach (var employee in employees)
            {
                employee.Salary *= 1.12m;

                sb.AppendLine($"{employee.FirstName} {employee.LastName} (${employee.Salary:F2})");
            }

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(x => x.FirstName.ToLower().StartsWith("sa"))
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    x.JobTitle,
                    x.Salary
                })
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToList();

            var sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle} - (${employee.Salary:F2})");
            }

            return sb.ToString().TrimEnd();
        }

        public static string DeleteProjectById(SoftUniContext context)
        {
            var project = context.Projects.FirstOrDefault(x => x.ProjectId == 2);
            var ep = context.EmployeesProjects.Where(x => x.ProjectId == 2);

            context.EmployeesProjects.RemoveRange(ep);
            context.Projects.Remove(project);

            context.SaveChanges();

            var projects = context.Projects
                .Select(x => x.Name)
                .Take(10)
                .ToList();

            var result = String.Join(Environment.NewLine, projects);
            return result;
        }

        public static string RemoveTown(SoftUniContext context)
        {
            var town = context.Towns.FirstOrDefault(x => x.Name == "Seattle");
            var addresses = context.Addresses.Where(x => x.Town.Name == "Seattle").ToList();
            var employees = context.Employees.Where(x => addresses.Contains(x.Address));

            foreach (var employee in employees)
            {
                employee.AddressId = null;
            }

            context.Addresses.RemoveRange(addresses);
            context.Towns.Remove(town);

            context.SaveChanges();

            return $"{addresses.Count} addresses in Seattle were deleted";
        }
    }
}
