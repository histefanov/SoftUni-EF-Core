using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProductShop.Data;
using ProductShop.DataTransferObjects;
using ProductShop.Models;
using Microsoft.EntityFrameworkCore;

namespace ProductShop
{
    public class StartUp
    {
        static IMapper mapper;
        static MapperConfiguration config;

        public static void Main(string[] args)
        {
            var context = new ProductShopContext();
            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();
            InitializeAutoMapper();

            //var inputJsonStringUsers = File.ReadAllText("../../../Datasets/users.json");
            //var inputJsonStringProducts = File.ReadAllText("../../../Datasets/products.json");
            //var inputJsonStringCategories = File.ReadAllText("../../../Datasets/categories.json");
            //var inputJsonStringCategoriesProducts = File.ReadAllText("../../../Datasets/categories-products.json");

            //ImportUsers(context, inputJsonStringUsers);
            //ImportProducts(context, inputJsonStringProducts);
            //ImportCategories(context, inputJsonStringCategories);
            //var result = ImportCategoryProducts(context, inputJsonStringCategoriesProducts);

            var result = GetUsersWithProducts(context);

            Console.WriteLine(result);
        }

        private static void InitializeAutoMapper()
        {
            config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductShopProfile>();
            });

            mapper = config.CreateMapper();
        }

        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            var usersDto = JsonConvert.DeserializeObject<UserInputModel[]>(inputJson);

            var users = mapper.Map<IEnumerable<User>>(usersDto);

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Count()}";
        }

        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            InitializeAutoMapper();

            var dtoProducts = JsonConvert.DeserializeObject<IEnumerable<ProductInputModel>>(inputJson);

            var products = mapper.Map<IEnumerable<Product>>(dtoProducts);

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Count()}";
        }

        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            InitializeAutoMapper();

            var dtoCategories = JsonConvert
                .DeserializeObject<IEnumerable<CategoriesInputModel>>(inputJson)
                .Where(x => x.Name != null);
            var categories = mapper.Map<IEnumerable<Category>>(dtoCategories);

            context.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Count()}";
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            InitializeAutoMapper();

            var dtoCategoriesProducts = JsonConvert.DeserializeObject<IEnumerable<CategoryProductInputModel>>(inputJson);
            var categoriesProducts = mapper.Map<IEnumerable<CategoryProduct>>(dtoCategoriesProducts);

            context.AddRange(categoriesProducts);
            context.SaveChanges();

            return $"Successfully imported {categoriesProducts.Count()}";
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            InitializeAutoMapper();

            var products = context.Products
                .Where(x => x.Price >= 500 && x.Price <= 1000)
                .ProjectTo<ProductOutputModel>(config)
                .OrderBy(x => x.Price)
                .ToList();

            var serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            var result = JsonConvert.SerializeObject(products, serializerSettings);
            return result;
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            //InitializeAutoMapper();

            var serializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            //var sellers = context.Users
            //    .Where(x => x.ProductsSold.Any(p => p.Buyer != null))
            //    .ProjectTo<SellerExportModel>(config)
            //    .OrderBy(x => x.LastName)
            //    .ThenBy(x => x.FirstName)
            //    .ToList();

            var sellers = context.Users
                .Where(x => x.ProductsSold.Any(p => p.BuyerId != null))
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    SoldProducts = x.ProductsSold
                    .Where(p => p.BuyerId != null)
                    .Select(p => new
                    {
                        p.Name,
                        p.Price,
                        BuyerFirstName = p.Buyer.FirstName,
                        BuyerLastName = p.Buyer.LastName
                    })
                })
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .ToList();

            var result = JsonConvert.SerializeObject(sellers, serializerSettings);

            return result;
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .Select(c => new
                {
                    Category = c.Name,
                    ProductsCount = c.CategoryProducts.Count,
                    AveragePrice = c.CategoryProducts.Average(cp => cp.Product.Price).ToString("F2"),
                    TotalRevenue = c.CategoryProducts.Sum(cp => cp.Product.Price).ToString("F2")
                })
                .OrderByDescending(c => c.ProductsCount)
                .ToList();

            var serializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                FloatFormatHandling = FloatFormatHandling.DefaultValue
            };     
            
            var result = JsonConvert.SerializeObject(categories, serializerSettings);
            return result;
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users
                .Include(u => u.ProductsSold)
                .ToList()
                .Where(u => u.ProductsSold.Any(p => p.BuyerId != null))
                .Select(u => new
                {
                    u.FirstName,
                    u.LastName,
                    u.Age,
                    SoldProducts = new
                    {
                        Count = u.ProductsSold.Where(p => p.BuyerId != null).Count(),
                        Products = u.ProductsSold
                            .Where(p => p.BuyerId != null)
                            .Select(p => new
                            {
                                p.Name,
                                p.Price
                            })
                    }
                })
                .OrderByDescending(u => u.SoldProducts.Products.Count())
                .ToList();

            var serializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                NullValueHandling = NullValueHandling.Ignore
            };

            var resultObj = new
            {
                UsersCount = users.Count(),
                Users = users
            };

            var result = JsonConvert.SerializeObject(resultObj, serializerSettings);
            return result;
        }
    }
}