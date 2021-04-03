using AutoMapper;
using AutoMapper.QueryableExtensions;
using CarDealer.Data;
using CarDealer.DTO.Export;
using CarDealer.DTO.Import;
using CarDealer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            //var supplierXml = File.ReadAllText("../../../Datasets/suppliers.xml");
            //var partsXml = File.ReadAllText("../../../Datasets/parts.xml");
            //var carsXml = File.ReadAllText("../../../Datasets/cars.xml");
            //var customersXml = File.ReadAllText("../../../Datasets/customers.xml");
            //var salesXml = File.ReadAllText("../../../Datasets/sales.xml");

            //ImportSuppliers(context, supplierXml);
            //ImportParts(context, partsXml);
            //ImportCars(context, carsXml);
            //ImportCustomers(context, customersXml);
            //ImportSales(context, salesXml);

            var result = GetSalesWithAppliedDiscount(context);

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
            var validPartIds = context.Parts
                .Select(x => x.Id);

            XmlSerializer xmlSerializer = new XmlSerializer(
                typeof(CarImportDto[]), new XmlRootAttribute("Cars"));

            var carDtos = (CarImportDto[])xmlSerializer.Deserialize(new StringReader(inputXml));
            var cars = new List<Car>();

            foreach (var carDto in carDtos)
            {
                var car = new Car
                {
                    Make = carDto.Make,
                    Model = carDto.Model,
                    TravelledDistance = carDto.TravelledDistance
                };

                var parts = carDto.Parts
                    .Select(x => x.Id)
                    .Where(x => validPartIds.Contains(x))
                    .Distinct();

                foreach (var partId in parts)
                {
                    car.PartCars.Add(new PartCar
                    {
                        PartId = partId
                    });
                }

                cars.Add(car);
            }

            context.Cars.AddRange(cars);
            context.SaveChanges();

            return $"Successfully imported {cars.Count()}";
        }

        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            InitializeMapper();

            var xmlSerializer = new XmlSerializer(
                typeof(CustomerImportDto[]), new XmlRootAttribute("Customers"));

            var customerDtos = (CustomerImportDto[])xmlSerializer.Deserialize(new StringReader(inputXml));
            var customers = mapper.Map<IEnumerable<Customer>>(customerDtos);

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count()}";
        }

        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            InitializeMapper();

            var validCarIds = context.Cars
                .Select(x => x.Id);

            var xmlSerializer = new XmlSerializer(
                typeof(SaleImportDto[]), new XmlRootAttribute("Sales"));

            var saleDtos = (SaleImportDto[])xmlSerializer
                .Deserialize(new StringReader(inputXml));

            var sales = mapper.Map<IEnumerable<Sale>>(
                saleDtos.Where(x => validCarIds.Contains(x.CarId)));

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Count()}";
        }

        public static string GetCarsWithDistance(CarDealerContext context)
        {
            InitializeMapper();

            var cars = context.Cars
                .Where(x => x.TravelledDistance > 2000000)
                .ProjectTo<CarExportDto>(config)
                .OrderBy(x => x.Make)
                .ThenBy(x => x.Model)
                .Take(10)
                .ToArray();

            var xmlSerializer = new XmlSerializer(typeof(CarExportDto[]), new XmlRootAttribute("cars"));

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");

            xmlSerializer.Serialize(new StringWriter(sb), cars, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            InitializeMapper();

            var bmwCars = context.Cars
                .Where(x => x.Make == "BMW")
                .ProjectTo<BmwExportDto>(config)
                .OrderBy(x => x.Model)
                .ThenByDescending(x => x.TravelledDistance)
                .ToArray();

            var xmlSerializer = new XmlSerializer(typeof(BmwExportDto[]), new XmlRootAttribute("cars"));
            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");

            xmlSerializer.Serialize(new StringWriter(sb), bmwCars, namespaces);

            return sb.ToString();
        }

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            InitializeMapper();

            var localSuppliers = context.Suppliers
                .Where(x => !x.IsImporter)
                .ProjectTo<LocalSupplierDto>(config)
                .ToArray();

            var xmlSerializer = new XmlSerializer(
                typeof(LocalSupplierDto[]), new XmlRootAttribute("suppliers"));

            var sb = new StringBuilder();
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");

            xmlSerializer.Serialize(new StringWriter(sb), localSuppliers, namespaces);
            return sb.ToString();
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var carsWithParts = context.Cars
                .Select(x => new CarWithPartsExportDto
                {
                    Make = x.Make,
                    Model = x.Model,
                    TravelledDistance = x.TravelledDistance,
                    Parts = x.PartCars.Select(pc => new PartExportDto
                    {
                        Name = pc.Part.Name,
                        Price = pc.Part.Price
                    })
                    .OrderByDescending(p => p.Price)
                    .ToArray()
                })
                .OrderByDescending(x => x.TravelledDistance)
                .ThenBy(x => x.Model)
                .Take(5)
                .ToArray();

            var xmlSerializer = new XmlSerializer(
                typeof(CarWithPartsExportDto[]), new XmlRootAttribute("cars"));

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");

            xmlSerializer.Serialize(new StringWriter(sb), carsWithParts, namespaces);

            return sb.ToString();
        }

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            InitializeMapper();

            var customers = context.Customers
                .ProjectTo<CustomerExportDto>(config)
                .Where(x => x.BoughtCars > 0)
                .OrderByDescending(x => x.SpentMoney)
                .ToArray();

            var xmlSerializer = new XmlSerializer(
                typeof(CustomerExportDto[]), new XmlRootAttribute("customers"));

            var sb = new StringBuilder();

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            xmlSerializer.Serialize(new StringWriter(sb), customers, ns);

            return sb.ToString();
        }

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context.Sales
                .Select(s => new SaleExportDto
                {
                    Car = new CarSaleExportDto
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TravelledDistance = s.Car.TravelledDistance
                    },
                    Discount = s.Discount,
                    CustomerName = s.Customer.Name,
                    Price = s.Car.PartCars.Sum(pc => pc.Part.Price),
                    PriceWithDiscount = s.Car.PartCars.Sum(c => c.Part.Price) -
                                        s.Car.PartCars.Sum(c => c.Part.Price) * s.Discount / 100
                })
                .ToArray();

            var xmlSerializer = new XmlSerializer(
                typeof(SaleExportDto[]), new XmlRootAttribute("sales"));

            var sb = new StringBuilder();

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            xmlSerializer.Serialize(new StringWriter(sb), sales, ns);

            return sb.ToString();
        }
    }
}