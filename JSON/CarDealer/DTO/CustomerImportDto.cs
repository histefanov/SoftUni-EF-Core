using System;
using System.Collections.Generic;
using System.Text;

namespace CarDealer.DTO
{
    public class CustomerImportDto
    {
        public string Name { get; set; }

        public DateTime dateTime { get; set; }

        public bool IsYoungDriver { get; set; }
    }
}
