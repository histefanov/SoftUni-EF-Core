using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VaporStore.DataProcessor.Dto.Import
{
    public class GenreImportDto
    {
        [Required]
        public string Name { get; set; }
    }
}

/*    • Id – integer, Primary Key
    • Name – text (required)
    • Games - collection of type Game*/