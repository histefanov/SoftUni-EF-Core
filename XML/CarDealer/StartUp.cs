using AutoMapper;
using CarDealer.Data;
using CarDealer.DTO.Import;
using CarDealer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        static MapperConfiguration config;
        static IMapper mapper;

        public static void Main(string[] args)
        {
            var context = new CarDealerContext();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var supplierXml = File.ReadAllText("../../../Datasets/suppliers.xml");
            var partsXml = File.ReadAllText("../../../Datasets/parts.xml");
            var carsXml = File.ReadAllText("../../../Datasets/cars.xml");

            ImportSuppliers(context, supplierXml);
            ImportParts(context, partsXml);
            var result = ImportCars(context, carsXml);

            Console.WriteLine(result);
        }

        private static void InitializeMapper()
        {
            config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CarDealerProfile>();
            });

            mapper = config.CreateMapper();
        }

        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(SupplierImportDto[]), new XmlRootAttribute("Suppliers"));

            var supplierDtos = (SupplierImportDto[])xmlSerializer.Deserialize(new StringReader(inputXml));
            var suppliers = new List<Supplier>();

            foreach (var supplierDto in supplierDtos)
            {
                Supplier supplier = new Supplier()
                {
                    Name = supplierDto.Name,
                    IsImporter = supplierDto.IsImporter
                };

                suppliers.Add(supplier);
            }

            context.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Count}";
        }

        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            InitializeMapper();

            var validSupplierIds = context.Suppliers
                .Select(x => x.Id);

            var xmlSerializer = new XmlSerializer(typeof(PartImportDto[]), new XmlRootAttribute("Parts"));

            var partDtos = (PartImportDto[])xmlSerializer.Deserialize(new StringReader(inputXml));
            var parts = mapper.Map<IEnumerable<Part>>(partDtos)
                .Where(p => validSupplierIds.Contains(p.SupplierId));

            //var parts = partDtos.Select(x => new Part
            //{
            //    Name = x.Name,
            //    Price = x.Price,
            //    Quantity = x.Quantity,
            //    SupplierId = x.SupplierId
            //});

            context.Parts.AddRange(parts);
            context.SaveChanges();

            var result = $"Successfully imported {parts.Count()}";
            return result;
        }

        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            InitializeMapper();

            XmlSerializer xmlSerializer = new XmlSerializer()
        }
    }
}