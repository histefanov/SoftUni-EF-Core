using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CarDealer.Data;
using CarDealer.DTO;
using CarDealer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        static IMapper mapper;
        static MapperConfiguration config;

        public static void Main(string[] args)
        {
            var context = new CarDealerContext();
            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            //var suppliersImportJson = File.ReadAllText("../../../Datasets/suppliers.json");
            //var partsImportJson = File.ReadAllText("../../../Datasets/parts.json");
            //var carsImportJson = File.ReadAllText("../../../Datasets/cars.json");
            //var customersImportJson = File.ReadAllText("../../../Datasets/customers.json");
            //var salesImportJson = File.ReadAllText("../../../Datasets/sales.json");

            //ImportSuppliers(context, suppliersImportJson);
            //ImportParts(context, partsImportJson);
            //ImportCars(context, carsImportJson);
            //ImportCustomers(context, customersImportJson);
            //ImportSales(context, salesImportJson);
            //var result = ImportPartCars(context);

            var result = GetSalesWithAppliedDiscount(context);
            Console.WriteLine(result);
        }

        public static void InitializeAutoMapper()
        {
            config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CarDealerProfile>();
            });

            mapper = config.CreateMapper();
        }

        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            InitializeAutoMapper();

            var supplierDtos = JsonConvert.DeserializeObject<IEnumerable<SupplierImportDto>>(inputJson);
            var suppliers = mapper.Map<IEnumerable<Supplier>>(supplierDtos);

            context.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Count()}.";
        }

        public static string ImportParts(CarDealerContext context, string inputJson)
        {
            InitializeAutoMapper();

            var validSupplierIds = context.Suppliers
                .Select(s => s.Id)
                .ToList();

            var partDtos = JsonConvert
                .DeserializeObject<IEnumerable<PartImportDto>>(inputJson)
                .Where(p => validSupplierIds.Contains(p.SupplierId));
            var parts = mapper.Map<IEnumerable<Part>>(partDtos);         

            context.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Count()}.";
        }

        public static string ImportCars(CarDealerContext context, string inputJson)
        {
            InitializeAutoMapper();

            var carDtos = JsonConvert.DeserializeObject<IEnumerable<CarImportDto>>(inputJson);

            var listOfCars = new List<Car>();

            foreach (var car in carDtos)
            {
                var currentCar = new Car()
                {
                    Make = car.Make,
                    Model = car.Model,
                    TravelledDistance = car.TravelledDistance
                };

                foreach (var partId in car?.PartsId.Distinct())
                {
                    currentCar.PartCars.Add(new PartCar
                    {
                        PartId = partId
                    });
                }

                listOfCars.Add(currentCar);
            }

            context.AddRange(listOfCars);
            context.SaveChanges();

            return $"Successfully imported {listOfCars.Count()}.";
        }

        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {
            InitializeAutoMapper();

            var customerDtos = JsonConvert
                .DeserializeObject<IEnumerable<CustomerImportDto>>(inputJson);

            var customers = mapper.Map<IEnumerable<Customer>>(customerDtos);

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count()}.";
        }

        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            InitializeAutoMapper();

            var saleDtos = JsonConvert
                .DeserializeObject<IEnumerable<SaleImportDto>>(inputJson);

            var sales = mapper.Map<IEnumerable<Sale>>(saleDtos);

            context.Sales.AddRange(sales);
            context.SaveChanges();

            var result = $"Successfully imported {sales.Count()}.";
            return result;
        }

        public static string ImportPartCars(CarDealerContext context)
        {
            var carsCount = context.Cars.Count();
            var partsCount = context.Parts.Count();
            var partCars = new List<PartCar>();
            var rand = new Random();

            for (int i = 1; i <= carsCount / 2; i++)
            {
                var partCar = new PartCar
                {
                    CarId = i,
                    PartId = rand.Next(1, partsCount)
                };

                partCars.Add(partCar);
            }

            context.PartCars.AddRange(partCars);
            context.SaveChanges();

            return $"Successfully generated and imported {partCars.Count()} PartCar objects.";
        }

        public static string GetOrderedCustomers(CarDealerContext context)
        {
            var customers = context.Customers
                .OrderBy(x => x.BirthDate)
                .ThenBy(x => x.IsYoungDriver)
                .Select(x => new
                {
                    x.Name,
                    BirthDate = x.BirthDate.ToString("dd/MM/yyyy"),
                    x.IsYoungDriver
                })
                .ToList();

            var result = JsonConvert.SerializeObject(customers, Formatting.Indented);

            return result;
        }

        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var toyotaCars = context.Cars
                .Where(x => x.Make == "Toyota")
                .Select(x => new
                {
                    x.Id,
                    x.Make,
                    x.Model,
                    x.TravelledDistance
                })
                .OrderBy(x => x.Model)
                .ThenByDescending(x => x.TravelledDistance)
                .ToList();

            var result = JsonConvert.SerializeObject(toyotaCars, Formatting.Indented);

            return result;
        }

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
                .Where(x => !x.IsImporter)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    PartsCount = x.Parts.Count()
                })
                .ToList();

            var result = JsonConvert.SerializeObject(suppliers, Formatting.Indented);

            return result;
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var carsWithParts = context.Cars
                .Select(x => new
                {
                    car = new
                    {
                        Make = x.Make,
                        Model = x.Model,
                        TravelledDistance = x.TravelledDistance
                    },
                    parts = x.PartCars.Select(p => new
                    {
                        Name = p.Part.Name,
                        Price = p.Part.Price.ToString("F2")
                    })
                })
                .ToList();

            var result = JsonConvert.SerializeObject(carsWithParts, Formatting.Indented);

            return result;
        }

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            InitializeAutoMapper();

            var customersWithSales = context.Customers
                .ProjectTo<CustomerSaleDto>(config)
                .Where(c => c.BoughtCars > 0)
                .OrderByDescending(c => c.SpentMoney)
                .ThenByDescending(c => c.BoughtCars)
                .ToList();

            var serializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            var result = JsonConvert.SerializeObject(customersWithSales, serializerSettings);

            return result;
        }

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            InitializeAutoMapper();

            var sales = context.Sales
                .Take(10)
                .Select(x => new SaleDto()
                {
                    Car = new CarDto()
                    {
                        Make = x.Car.Make,
                        Model = x.Car.Model,
                        TravelledDistance = x.Car.TravelledDistance
                    },
                    CustomerName = x.Customer.Name,
                    Discount = x.Discount.ToString("F2"),
                    Price = x.Car.PartCars.Sum(pc => pc.Part.Price).ToString("F2"),
                    PriceWithDiscount = (x.Car.PartCars.Sum(pc => pc.Part.Price) * ((100 - x.Discount) / 100)).ToString("F2")
                })
                .ToList();

            var serializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
            };

            var result = JsonConvert.SerializeObject(sales, serializerSettings);
            return result;
        }
    }
}