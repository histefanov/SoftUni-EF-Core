﻿namespace ProductShop.DataTransferObjects
{
    public class SoldProductExportModel
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public string BuyerFirstName { get; set; }

        public string BuyerLastName { get; set; }
    }
}