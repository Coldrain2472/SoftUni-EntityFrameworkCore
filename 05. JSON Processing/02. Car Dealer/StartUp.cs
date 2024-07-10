using AutoMapper;
using CarDealer.Data;
using CarDealer.DTOs.Import;
using CarDealer.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CarDealer;

public class StartUp
{
    public static void Main()
    {
        CarDealerContext context = new CarDealerContext();
    }

    public static IMapper CreateMapper()
    {
        MapperConfiguration configuration = new MapperConfiguration(config =>
        {
            config.AddProfile<CarDealerProfile>();
        });

        IMapper mapper = configuration.CreateMapper();

        return mapper;
    }

    // 09. Import Suppliers
    public static string ImportSuppliers(CarDealerContext context, string inputJson)
    {
        var mapper = CreateMapper();

        ImportSupplierDto[] suppliersDtos = JsonConvert.DeserializeObject<ImportSupplierDto[]>(inputJson);

        Supplier[] suppliers = mapper.Map<Supplier[]>(suppliersDtos);

        context.Suppliers.AddRangeAsync(suppliers);
        context.SaveChanges();

        return $"Successfully imported {suppliers.Length}.";
    }

    // 10. Import Parts
    public static string ImportParts(CarDealerContext context, string inputJson)
    {
        var mapper = CreateMapper();

        ImportPartDto[] partsDtos = JsonConvert.DeserializeObject<ImportPartDto[]>(inputJson);

        ICollection<Part> parts = new List<Part>();

        foreach (var part in partsDtos)
        {
            if (context.Suppliers.Any(s => s.Id == part.SupplierId))
            {
                parts.Add(mapper.Map<Part>(part));
            }
        }

        context.Parts.AddRangeAsync(parts);
        context.SaveChanges();

        return $"Successfully imported {parts.Count}.";
    }

    // 11. Import Cars
    public static string ImportCars(CarDealerContext context, string inputJson)
    {
        var mapper = CreateMapper();

        ImportCarDto[] importCarDtos = JsonConvert.DeserializeObject<ImportCarDto[]>(inputJson);

        ICollection<Car> carsToAdd = new HashSet<Car>();

        foreach (var carDto in importCarDtos)
        {
            Car currentCar = mapper.Map<Car>(carDto);

            foreach (var id in carDto.PartsIds)
            {
                if (context.Parts.Any(p => p.Id == id))
                {
                    currentCar.PartsCars.Add(new PartCar
                    {
                        PartId = id,
                    });
                }
            }

            carsToAdd.Add(currentCar);
        }

        context.Cars.AddRange(carsToAdd);
        context.SaveChanges();

        return $"Successfully imported {carsToAdd.Count}.";
    }

    // 12. Import Customers
    public static string ImportCustomers(CarDealerContext context, string inputJson)
    {
        var mapper = CreateMapper();

        ImportCarDto[] customerDtos = JsonConvert.DeserializeObject<ImportCarDto[]>(inputJson);

        Customer[] customers = JsonConvert.DeserializeObject<Customer[]>(inputJson);

        context.Customers.AddRange(customers);
        context.SaveChanges();

        return $"Successfully imported {customers.Length}.";
    }

    // 13. Import Sales
    public static string ImportSales(CarDealerContext context, string inputJson)
    {
        ImportSaleDto[] salesDtos = JsonConvert.DeserializeObject<ImportSaleDto[]>(inputJson);

        Sale[] sales = JsonConvert.DeserializeObject<Sale[]>(inputJson);

        context.Sales.AddRange(sales);
        context.SaveChanges();

        return $"Successfully imported {sales.Length}.";
    }

    // 14. Export Ordered Customers
    public static string GetOrderedCustomers(CarDealerContext context)
    {
        var customers = context.Customers
            .AsNoTracking()
            .OrderBy(c => c.BirthDate)
            .ThenBy(c => c.IsYoungDriver)
            .Select(c => new
            {
                c.Name,
                BirthDate = c.BirthDate.ToString("dd/MM/yyyy"),
                c.IsYoungDriver
            })
            .ToArray();

        return JsonConvert.SerializeObject(customers, Formatting.Indented);
    }

    // 15. Export Cars From Make Toyota
    public static string GetCarsFromMakeToyota(CarDealerContext context)
    {
        var cars = context.Cars
           .AsNoTracking()
           .Where(c => c.Make.ToLower() == "toyota")
           .Select(c => new
           {
               c.Id,
               c.Make,
               c.Model,
               c.TraveledDistance
           })
           .OrderBy(c => c.Model)
           .ThenByDescending(c => c.TraveledDistance)
           .ToArray();

        return JsonConvert.SerializeObject(cars, Formatting.Indented);
    }

    // 16. Export Local Suppliers
    public static string GetLocalSuppliers(CarDealerContext context)
    {
        var suppliers = context.Suppliers
            .AsNoTracking()
            .Where(s => !s.IsImporter)
            .Select(s => new
            {
                s.Id,
                s.Name,
                PartsCount = s.Parts.Count,
            })
            .ToArray();

        return JsonConvert.SerializeObject(suppliers, Formatting.Indented);
    }

    // 17. Export Cars With Their List Of Parts
    public static string GetCarsWithTheirListOfParts(CarDealerContext context)
    {
        var cars = context.Cars
            .AsNoTracking()
            .Select(c => new
            {
                car = new
                {
                    c.Make,
                    c.Model,
                    c.TraveledDistance
                },
                parts = c.PartsCars
                    .Select(p => new
                    {
                        Name = p.Part.Name,
                        Price = p.Part.Price.ToString("f2"),
                    })
                    .ToArray()
            })
            .ToArray();

        return JsonConvert.SerializeObject(cars, Formatting.Indented);
    }

    // 18. Export Total Sales By Customer
    public static string GetTotalSalesByCustomer(CarDealerContext context)
    {
        var customers = context.Customers
               .Where(c => c.Sales.Count() > 0)
               .Select(c => new
               {
                   fullName = c.Name,
                   boughtCars = c.Sales.Count(),
                   spentMoney = c.Sales.Sum(s => s.Car.PartsCars.Sum(p => p.Part.Price))
               })
               .OrderByDescending(x => x.spentMoney)
               .ThenByDescending(x => x.boughtCars)
               .ToList();

        return JsonConvert.SerializeObject(customers, Formatting.Indented);
    }

    // 19. Export Sales With Applied Discount
    public static string GetSalesWithAppliedDiscount(CarDealerContext context)
    {
        var sales = context.Sales
            .Take(10)
            .Select(s => new
            {
                car = new
                {
                    Make = s.Car.Make,
                    Model = s.Car.Model,
                    TraveledDistance = s.Car.TraveledDistance
                },
                customerName = s.Customer.Name,
                discount = s.Discount.ToString("f2"),
                price = s.Car.PartsCars.Sum(pc => pc.Part.Price).ToString("f2"),
                priceWithDiscount = ((s.Car.PartsCars.Sum(pc => pc.Part.Price) * (1 - s.Discount / 100))).ToString("f2")
            })
            .ToArray();

        return JsonConvert.SerializeObject(sales, Formatting.Indented);
    }
}