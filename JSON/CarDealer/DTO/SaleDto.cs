using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarDealer.DTO
{
    public class SaleDto
    {
        [JsonProperty("car")]
        public CarDto Car { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        public string Discount { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("priceWithDiscount")]
        public string PriceWithDiscount { get; set; }
    }
}
