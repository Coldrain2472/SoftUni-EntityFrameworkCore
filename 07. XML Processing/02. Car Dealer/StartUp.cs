using AutoMapper;
using CarDealer.Data;
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

        string result = ImportSuppliers(context, inputXml);

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
}