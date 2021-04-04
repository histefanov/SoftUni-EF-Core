namespace TeisterMask.DataProcessor
{
    using System;
    using System.Collections.Generic;

    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Castle.Core.Internal;
    using Data;
    using Newtonsoft.Json;
    using TeisterMask.Data.Models;
    using TeisterMask.Data.Models.Enums;
    using TeisterMask.DataProcessor.ImportDto;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            var output = new StringBuilder();
            var xmlSerializer = new XmlSerializer(
                typeof(ProjectInputModel[]), new XmlRootAttribute("Projects"));

            var data = (ProjectInputModel[])xmlSerializer.Deserialize(new StringReader(xmlString));

            foreach (var projectModel in data)
            {
                if (!IsValid(projectModel))
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                var isPODValid = DateTime.TryParseExact(
                    projectModel.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out DateTime projectOpenDate);

                var isPDDValid = DateTime.TryParseExact(
                    projectModel.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out DateTime projectDueDate);               

                if (!isPODValid)
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                var project = new Project
                {
                    Name = projectModel.Name,
                    OpenDate = projectOpenDate, //possible issues
                    DueDate = projectDueDate,
                };

                if (projectModel.DueDate.IsNullOrEmpty())
                {
                    project.DueDate = null;
                }

                var validTasks = new List<Task>();

                foreach (var taskModel in projectModel.Tasks)
                {
                    var isTODValid = DateTime.TryParseExact(
                    taskModel.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out DateTime taskOpenDate);

                    var isTDDValid = DateTime.TryParseExact(
                    taskModel.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out DateTime taskDueDate);

                    if (!IsValid(taskModel) ||
                        !isTODValid || !isTDDValid ||
                        taskOpenDate < projectOpenDate ||
                        taskDueDate > (project.DueDate ?? DateTime.MaxValue))
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    validTasks.Add(new Task
                    {
                        Name = taskModel.Name,
                        OpenDate = taskOpenDate,
                        DueDate = taskDueDate,
                        ExecutionType = (ExecutionType)taskModel.ExecutionType,
                        LabelType = (LabelType)taskModel.LabelType
                    });
                }

                project.Tasks = validTasks;

                context.Projects.Add(project);
                context.SaveChanges();
                output.AppendLine(String.Format(SuccessfullyImportedProject, project.Name, project.Tasks.Count));
            }

            return output.ToString().TrimEnd();
        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {
            var output = new StringBuilder();
            var data = JsonConvert.DeserializeObject<IEnumerable<EmployeeInputModel>>(jsonString);

            foreach (var employeeModel in data)
            {
                if (!IsValid(employeeModel))
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                var employee = new Employee
                {
                    Username = employeeModel.Username,
                    Email = employeeModel.Email,
                    Phone = employeeModel.Phone
                };

                foreach (var taskId in employeeModel.Tasks.Distinct())
                {
                    var contextTask = context.Tasks.FirstOrDefault(x => x.Id == taskId);

                    if (contextTask == null)
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }

                    employee.EmployeesTasks.Add(new EmployeeTask
                    {
                        TaskId = taskId
                    });
                }

                context.Employees.Add(employee);
                context.SaveChanges();
                output.AppendLine(String.Format(SuccessfullyImportedEmployee, employee.Username, employee.EmployeesTasks.Count));
            }

            return output.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}