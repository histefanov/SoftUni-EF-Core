namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using SoftJail.Data.Models.Enums;
    using SoftJail.DataProcessor.ImportDto;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Deserializer
    {
        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var sb = new StringBuilder();
            var departments = new List<Department>();

            var departmentCells =
                JsonConvert.DeserializeObject<IEnumerable<DepartmentCellsInputModel>>
                (jsonString);

            foreach (var departmentCell in departmentCells)
            {
                if (!IsValid(departmentCell) || 
                    !departmentCell.Cells.All(IsValid) ||
                    departmentCell.Cells.Count == 0)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var department = new Department
                {
                    Name = departmentCell.Name,
                    Cells = departmentCell.Cells.Select(c => new Cell
                    {
                        CellNumber = c.CellNumber,
                        HasWindow = c.HasWindow,
                    })
                    .ToList()
                };

                departments.Add(department);

                sb.AppendLine($"Imported {department.Name} with {department.Cells.Count()} cells");
            }

            context.Departments.AddRange(departments);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var outputSb = new StringBuilder();
            var prisoners = new List<Prisoner>();

            var prisonerMails =
                JsonConvert.DeserializeObject<IEnumerable<PrisonerMailsInputModel>>
                (jsonString);

            foreach (var prisonerDto in prisonerMails)
            {
                if (!IsValid(prisonerDto) ||
                    !prisonerDto.Mails.All(IsValid))

                {
                    outputSb.AppendLine("Invalid Data");
                    continue;
                }

                var isValidIncDate = DateTime.TryParseExact(prisonerDto.IncarcerationDate,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime incDate);

                var isValidReleaseDate = DateTime.TryParseExact(prisonerDto.ReleaseDate,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime releaseDate);

                var prisoner = new Prisoner()
                {
                    FullName = prisonerDto.FullName,
                    Nickname = prisonerDto.Nickname,
                    Age = prisonerDto.Age,
                    IncarcerationDate = incDate,
                    ReleaseDate = isValidReleaseDate ? (DateTime?)releaseDate : null,
                    Bail = prisonerDto.Bail,
                    CellId = prisonerDto.CellId,
                    Mails = prisonerDto.Mails.Select(m => new Mail
                    {
                        Description = m.Description,
                        Sender = m.Sender,
                        Address = m.Address
                    })
                    .ToList()
                };

                prisoners.Add(prisoner);
                outputSb.AppendLine($"Imported {prisoner.FullName} {prisoner.Age} years old");
            }

            context.Prisoners.AddRange(prisoners);
            context.SaveChanges();

            return outputSb.ToString().TrimEnd();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            var sb = new StringBuilder();
            var officers = new List<Officer>();

            var xmlSerializer = new XmlSerializer(
                typeof(OfficersPrisonersInputModel[]), new XmlRootAttribute("Officers"));

            var officerDtos = (OfficersPrisonersInputModel[])xmlSerializer.Deserialize(new StringReader(xmlString));

            foreach (var officerDto in officerDtos)
            {
                if (!IsValid(officerDto) ||
                    !officerDto.Prisoners.All(IsValid))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var officer = new Officer()
                {
                    FullName = officerDto.Name,
                    Salary = officerDto.Salary,
                    Position = Enum.Parse<Position>(officerDto.Position),
                    Weapon = Enum.Parse<Weapon>(officerDto.Weapon),
                    DepartmentId = officerDto.DepartmentId,
                    OfficerPrisoners = officerDto.Prisoners.Select(x => new OfficerPrisoner
                    {
                        PrisonerId = x.Id
                    })
                    .ToList()
                };

                officers.Add(officer);
                sb.AppendLine($"Imported {officer.FullName} ({officer.OfficerPrisoners.Count} prisoners)");
            }

            context.Officers.AddRange(officers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}