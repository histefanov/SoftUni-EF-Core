using AutoMapper;
using ProductShop.DataTransferObjects;
using ProductShop.Models;

namespace ProductShop
{
    public class ProductShopProfile : Profile
    {
        public ProductShopProfile()
        {
            this.CreateMap<UserInputModel, User>();
            this.CreateMap<ProductInputModel, Product>();
            this.CreateMap<CategoriesInputModel, Category>();
            this.CreateMap<CategoryProductInputModel, CategoryProduct>();
            this.CreateMap<Product, ProductOutputModel>()
                .ForMember(x => x.Seller,
                           options => options.MapFrom(src =>
                               $"{src.Seller.FirstName} {src.Seller.LastName}"));

            this.CreateMap<User, SellerExportModel>()
                .ForMember(x => x.SoldProducts,
                           options => options.MapFrom(src =>
                               src.ProductsSold));

            this.CreateMap<Product, SoldProductExportModel>()
                .ForMember(x => x.BuyerFirstName,
                           options => options.MapFrom(src =>
                               src.Buyer.FirstName))
                .ForMember(x => x.BuyerLastName,
                           options => options.MapFrom(src =>
                               src.Buyer.LastName));
        }
    }
}
