using AutoMapper;
using CarDealer.Data;
using CarDealer.DTOs.Export;
using CarDealer.DTOs.Import;
using CarDealer.Models;
using CarDealer.Utilities;
using System.Xml.Serialization;

namespace CarDealer;

public class StartUp
{
    public static void Main()
    {
        CarDealerContext context = new CarDealerContext();

        string inputXml = File.ReadAllText("../../../Datasets/suppliers.xml");

        string result = GetSalesWithAppliedDiscount(context);

        Console.WriteLine(result);
    }

    private static IMapper CreateMapper()
    {
        MapperConfiguration config = new MapperConfiguration(config =>
        {
            config.AddProfile<CarDealerProfile>();
        });

        var mapper = config.CreateMapper();

        return mapper;
    }

    // 09.Import Suppliers
    public static string ImportSuppliers(CarDealerContext context, string inputXml)
    {
        var mapper = CreateMapper();
        var xmlParser = new XmlParser();

        ImportSupplierDto[] supplierDtos = xmlParser.Deserialize<ImportSupplierDto[]>(inputXml, "Suppliers");

        Supplier[] suppliers = mapper.Map<Supplier[]>(supplierDtos);

        context.Suppliers.AddRange(suppliers);
        context.SaveChanges();

        return $"Successfully imported {suppliers.Length}";
    }

    // 10. Import Parts
    public static string ImportParts(CarDealerContext context, string inputXml)
    {
        var mapper = CreateMapper();
        var xmlParser = new XmlParser();

        ImportPartDto[] partDtos = xmlParser.Deserialize<ImportPartDto[]>(inputXml, "Parts");

        var allSuppliers = context.Suppliers.ToArray();
        var supplierIds = context.Suppliers.Select(s => s.Id).ToArray();
        List<Part> parts = new List<Part>();

        foreach (var partDto in partDtos)
        {
            if (supplierIds.Contains(partDto.SupplierId))
            {
                parts.Add(mapper.Map<Part>(partDto));
            }
        }

        context.Parts.AddRange(parts);
        context.SaveChanges();

        return $"Successfully imported {parts.Count}";
    }

    // 11. Import Cars
    public static string ImportCars(CarDealerContext context, string inputXml)
    {
        var mapper = CreateMapper();
        var xmlParser = new XmlParser();

        ImportCarDto[] carsDtos = xmlParser.Deserialize<ImportCarDto[]>(inputXml, "Cars");

        List<Car> cars = new List<Car>();
        List<PartCar> partCars = new List<PartCar>();
        int[] allPartIds = context.Parts.Select(p => p.Id).ToArray();
        int carId = 1;

        foreach (var dto in carsDtos)
        {
            Car car = new Car()
            {
                Make = dto.Make,
                Model = dto.Model,
                TraveledDistance = dto.TraveledDistance
            };

            cars.Add(car);

            foreach (int partId in dto.Parts
                .Where(p => allPartIds.Contains(p.PartId))
                .Select(p => p.PartId)
                .Distinct())
            {
                PartCar partCar = new PartCar()
                {
                    CarId = carId,
                    PartId = partId
                };
                partCars.Add(partCar);
            }
            carId++;
        }

        context.Cars.AddRange(cars);
        context.PartsCars.AddRange(partCars);
        context.SaveChanges();

        return $"Successfully imported {cars.Count}";
    }

    // 12. Import Customers
    public static string ImportCustomers(CarDealerContext context, string inputXml)
    {
        var mapper = CreateMapper();
        var xmlParser = new XmlParser();

        ImportCustomerDto[] customerDtos = xmlParser.Deserialize<ImportCustomerDto[]>(inputXml, "Customers");

        Customer[] customers = mapper.Map<Customer[]>(customerDtos);

        context.Customers.AddRange(customers);
        context.SaveChanges();

        return $"Successfully imported {customers.Length}";
    }

    // 13. Import Sales
    public static string ImportSales(CarDealerContext context, string inputXml)
    {
        var mapper = CreateMapper();
        var xmlParser = new XmlParser();

        ImportSaleDto[] saleDtos = xmlParser.Deserialize<ImportSaleDto[]>(inputXml, "Sales");

        HashSet<int> carIds = context.Cars.Select(c => c.Id).ToHashSet<int>();
        var sales = new List<Sale>();

        foreach (var saleDto in saleDtos)
        {
            if (carIds.Contains(saleDto.CarId))
            {
                sales.Add(mapper.Map<Sale>(saleDto));
            }
        }

        context.Sales.AddRange(sales);
        context.SaveChanges();

        return $"Successfully imported {sales.Count}";
    }

    // 14. Export Cars With Distance
    public static string GetCarsWithDistance(CarDealerContext context)
    {
        var xmlParser = new XmlParser();

        var carDtos = context.Cars
            .Where(c => c.TraveledDistance > 2000000)
            .OrderBy(c => c.Make)
            .ThenBy(c => c.Model)
            .Take(10)
            .Select(c => new ExportCarDistanceDto
            {
                Make = c.Make,
                Model = c.Model,
                TraveledDistance = c.TraveledDistance
            })
            .ToArray();

        return xmlParser.Serialize<ExportCarDistanceDto[]>(carDtos, "cars");
    }

    // 15. Export Cars From Make BMW
    public static string GetCarsFromMakeBmw(CarDealerContext context)
    {
        var xmlParser = new XmlParser();

        var carDtos = context.Cars
            .Where(c => c.Make.ToUpper() == "BMW")
            .OrderBy(c => c.Model)
            .ThenByDescending(c => c.TraveledDistance)
            .Select(c => new ExportCarMake
            {
                Id = c.Id,
                Model = c.Model,
                TraveledDistance = c.TraveledDistance
            })
            .ToArray();

        return xmlParser.Serialize<ExportCarMake[]>(carDtos, "cars");
    }

    // 16. Export Local Suppliers
    public static string GetLocalSuppliers(CarDealerContext context)
    {
        var xmlParser = new XmlParser();

        var supplierDtos = context.Suppliers
            .Where(s => s.IsImporter == false)
            .Select(s => new ExportLocalSupplierDto
            {
                Id = s.Id,
                Name = s.Name,
                PartsCount = s.Parts.Count()
            })
            .ToArray();

        return xmlParser.Serialize<ExportLocalSupplierDto[]>(supplierDtos, "suppliers");
    }

    // 17. Export Cars With Their List Of Parts
    public static string GetCarsWithTheirListOfParts(CarDealerContext context)
    {
        var xmlParser = new XmlParser();

        var carsPartsDtos = context.Cars
             .OrderByDescending(c => c.TraveledDistance)
             .ThenBy(c => c.Model)
             .Take(5)
             .Select(c => new ExportCarPartsDto()
             {
                 Make = c.Make,
                 Model = c.Model,
                 TraveledDistance = c.TraveledDistance,
                 Parts = c.PartsCars.Select(pc => new PartsDto()
                 {
                     Name = pc.Part.Name,
                     Price = pc.Part.Price
                 })
                    .OrderByDescending(p => p.Price)
                    .ToArray()
             })
                .ToArray();

        return xmlParser.Serialize<ExportCarPartsDto[]>(carsPartsDtos, "cars");
    }

    // 18. Export Total Sales By Customer
    public static string GetTotalSalesByCustomer(CarDealerContext context)
    {
        var xmlParser = new XmlParser();

        var dto = context.Customers
               .Where(c => c.Sales.Any())
               .Select(c => new
               {
                   FullName = c.Name,
                   BoughtCars = c.Sales.Count(),
                   SalesInfo = c.Sales.Select(s => new
                   {
                       Prices = c.IsYoungDriver
                           ? s.Car.PartsCars.Sum(p => Math.Round((double)p.Part.Price * 0.95, 2))
                           : s.Car.PartsCars.Sum(p => (double)p.Part.Price)
                   }).ToArray(),
               })
               .ToArray();

        ExportTotalSalesByCustomerDto[] totalSalesDtos = dto
            .OrderByDescending(t => t.SalesInfo.Sum(s => s.Prices))
            .Select(t => new ExportTotalSalesByCustomerDto()
            {
                FullName = t.FullName,
                BoughtCars = t.BoughtCars,
                SpentMoney = t.SalesInfo.Sum(s => s.Prices).ToString("f2")
            })
            .ToArray();

        return xmlParser.Serialize<ExportTotalSalesByCustomerDto[]>(totalSalesDtos, "customers");
    }

    // 19. Export Sales With Applied Discount
    public static string GetSalesWithAppliedDiscount(CarDealerContext context)
    {
        var xmlParser = new XmlParser();

        ExportSalesWithAppliedDiscountDto[] salesDtos = context
                .Sales
                .Select(s => new ExportSalesWithAppliedDiscountDto()
                {
                    SingleCar = new SingleCar()
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TraveledDistance = s.Car.TraveledDistance
                    },
                    Discount = (int)s.Discount,
                    CustomerName = s.Customer.Name,
                    Price = s.Car.PartsCars.Sum(p => p.Part.Price),
                    PriceWithDiscount = Math.Round((double)(s.Car.PartsCars.Sum(p => p.Part.Price) * (1 - (s.Discount / 100))), 4)
                })
                .ToArray();

        return xmlParser.Serialize<ExportSalesWithAppliedDiscountDto[]>(salesDtos, "sales");
    }
}