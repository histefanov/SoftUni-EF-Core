﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TeisterMask.DataProcessor.ImportDto
{
    public class EmployeeInputModel
    {
        [Required]
        [StringLength(40, MinimumLength = 3)]
        [RegularExpression("[A-Za-z0-9]+")]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression("[0-9]{3}-[0-9]{3}-[0-9]{4}")]
        public string Phone { get; set; }

        public List<int> Tasks { get; set; }
    }
}

/*    • Id - integer, Primary Key
    • Username - text with length [3, 40]. Should contain only lower or upper case letters and/or digits. (required)
    • Email – text (required). Validate it! There is attribute for this job.
    • Phone - text. Consists only of three groups (separated by '-'), the first two consist of three digits and the last one - of 4 digits. (required)
    • EmployeesTasks - collection of type EmployeeTask*/

