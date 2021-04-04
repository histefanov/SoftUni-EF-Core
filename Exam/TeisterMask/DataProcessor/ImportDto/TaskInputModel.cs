using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using TeisterMask.Data.Models.Enums;

namespace TeisterMask.DataProcessor.ImportDto
{
    [XmlType("Task")]
    public class TaskInputModel
    {
        [XmlElement("Name")]
        [Required]
        [StringLength(40, MinimumLength = 2)]
        public string Name { get; set; }

        [XmlElement("OpenDate")]
        [Required]
        public string OpenDate { get; set; } //na niki magiite, vnimavai!

        [XmlElement("DueDate")]
        [Required]
        public string DueDate { get; set; } //na niki magiite, vnimavai!

        [XmlElement("ExecutionType")]
        [Range(0, 3)]
        public int ExecutionType { get; set; } //na niki magiite, vnimavai!

        [XmlElement("LabelType")]
        [Range(0, 4)]
        public int LabelType { get; set; } //pak na niki magiite...
    }
}

/*    • Id - integer, Primary Key
    • Name - text with length [2, 40] (required)
    • OpenDate - date and time (required)
    • DueDate - date and time (required)
    • ExecutionType - enumeration of type ExecutionType, with possible values (ProductBacklog, SprintBacklog, InProgress, Finished) (required)
    • LabelType - enumeration of type LabelType, with possible values (Priority, CSharpAdvanced, JavaAdvanced, EntityFramework, Hibernate) (required)
    • ProjectId - integer, foreign key (required)
    • Project - Project 
    • EmployeesTasks - collection of type EmployeeTask*/