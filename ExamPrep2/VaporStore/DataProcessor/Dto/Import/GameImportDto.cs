using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VaporStore.Data.Models;

namespace VaporStore.DataProcessor.Dto.Import
{
    public class GameImportDto
    {
        [Required]
        public string Name { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal Price { get; set; }

        [Required]
        public DateTime? ReleaseDate { get; set; }

        [Required]
        public string Developer { get; set; }

        [Required]
        public string Genre { get; set; }

        public List<string> Tags { get; set; }
    }
}

/*
    • \\\Id – integer, Primary Key
    • \\\Name – text (required)
    • \\\Price – decimal (non-negative, minimum value: 0) (required)
    • \\\ReleaseDate – Date (required)
    • \\\DeveloperId – integer, foreign key (required)
    • \\\Developer – the game’s developer (required)
    • \\\GenreId – integer, foreign key (required)
    • \\\Genre – the game’s genre (required)
    • \\\Purchases - collection of type Purchase
    • GameTags - collection of type GameTag. Each game must have at least one tag.*/

