using System;
using System.Collections.Generic;
using System.Text;

namespace ProductShop.DataTransferObjects
{
    public class SellerExportModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public List<SoldProductExportModel> SoldProducts { get; set; }
    }
}
