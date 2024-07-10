namespace CarDealer;

using AutoMapper;
using CarDealer.DTOs.Import;
using CarDealer.Models;

public class CarDealerProfile : Profile
{
    public CarDealerProfile()
    {
        this.CreateMap<ImportSupplierDto, Supplier>();

        this.CreateMap<ImportPartDto, Part>();

        this.CreateMap<ImportCarDto, Car>();

        this.CreateMap<ImportCustomerDto, Customer>();

        this.CreateMap<ImportSaleDto, Sale>();
    }
}
