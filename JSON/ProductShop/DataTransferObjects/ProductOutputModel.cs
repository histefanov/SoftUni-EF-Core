using System;
using System.Collections.Generic;
using System.Text;

namespace ProductShop.DataTransferObjects
{
    public class ProductOutputModel
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Seller { get; set; }
    }
}
