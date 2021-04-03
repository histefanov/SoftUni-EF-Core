using AutoMapper;
using CarDealer.DTO.Export;
using CarDealer.DTO.Import;
using CarDealer.Models;
using System.Linq;

namespace CarDealer
{
    public class CarDealerProfile : Profile
    {
        public CarDealerProfile()
        {
            this.CreateMap<PartImportDto, Part>();
            this.CreateMap<CustomerImportDto, Customer>();
            this.CreateMap<SaleImportDto, Sale>();

            this.CreateMap<Car, CarExportDto>();
            this.CreateMap<Car, BmwExportDto>();
            this.CreateMap<Supplier, LocalSupplierDto>()
                .ForMember(x => x.PartsCount, opts =>
                    opts.MapFrom(src => src.Parts.Count()));

            this.CreateMap<Car, CarWithPartsExportDto>()
                .ForMember(x => x.Parts, opts =>
                    opts.MapFrom(src => src.PartCars.Select(pc => pc.Part)));

            this.CreateMap<Customer, CustomerExportDto>()
                .ForMember(x => x.BoughtCars, opts =>
                    opts.MapFrom(src => src.Sales.Count()))
                .ForMember(x => x.SpentMoney, opts =>
                    opts.MapFrom(src => src.Sales
                                               .Select(s => s.Car)
                                               .SelectMany(c => c.PartCars)
                                               .Sum(c => c.Part.Price)));

        }
    }
}
