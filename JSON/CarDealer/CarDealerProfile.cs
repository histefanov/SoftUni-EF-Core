using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using CarDealer.DTO;
using CarDealer.Models;

namespace CarDealer
{
    public class CarDealerProfile : Profile
    {
        public CarDealerProfile()
        {
            this.CreateMap<SupplierImportDto, Supplier>();
            this.CreateMap<PartImportDto, Part>();
            this.CreateMap<CarImportDto, Car>();
            this.CreateMap<CustomerImportDto, Customer>();
            this.CreateMap<SaleImportDto, Sale>();

            this.CreateMap<Customer, CustomerSaleDto>()
                .ForMember(x => x.FullName, opts => opts.MapFrom(src => src.Name))
                .ForMember(x => x.BoughtCars, opts => opts.MapFrom(src => src.Sales.Count))
                .ForMember(x => x.SpentMoney, opts => opts.MapFrom(src => src.Sales
                                                                       .Select(s => s.Car.PartCars
                                                                           .Select(pc => pc.Part)
                                                                           .Sum(p => p.Price))
                                                                       .Sum()));

            this.CreateMap<Sale, SaleDto>()
                .ForMember(x => x.Price, opts => opts.MapFrom(src => src.Car.PartCars
                                                                   .Select(pc => pc.Part.Price)
                                                                   .Sum()))
                .ForMember(x => x.PriceWithDiscount, opts => opts.MapFrom(src => src.Car.PartCars
                                                                               .Select(pc => pc.Part.Price)
                                                                               .Sum() * (src.Discount / 100)));
        }
    }
}
