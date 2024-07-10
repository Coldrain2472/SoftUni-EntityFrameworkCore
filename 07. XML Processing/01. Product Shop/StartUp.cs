using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProductShop.Data;
using ProductShop.DTOs.Export;
using ProductShop.DTOs.Import;
using ProductShop.Models;
using ProductShop.Utilities;

namespace ProductShop;

public class StartUp
{
    public static void Main()
    {
        ProductShopContext context = new ProductShopContext();

        // string inputXml = File.ReadAllText("../../../Datasets/products.xml");

        string result = GetUsersWithProducts(context);

        Console.WriteLine(result);
    }

    private static IMapper CreateMapper()
    {
        MapperConfiguration config = new MapperConfiguration(config =>
        {
            config.AddProfile<ProductShopProfile>();
        });

        var mapper = config.CreateMapper();

        return mapper;
    }

    // 01. Import Users
    public static string ImportUsers(ProductShopContext context, string inputXml)
    {
        var mapper = CreateMapper();
        var xmlParser = new XmlParser();

        ImportUserDto[] userDtos = xmlParser.Deserialize<ImportUserDto[]>(inputXml, "Users");

        User[] users = mapper.Map<User[]>(userDtos);

        context.Users.AddRange(users);
        context.SaveChanges();

        return $"Successfully imported {users.Length}";
    }

    // 02. Import Products
    public static string ImportProducts(ProductShopContext context, string inputXml)
    {
        var mapper = CreateMapper();
        var xmlParser = new XmlParser();

        ImportProductDto[] productDtos = xmlParser.Deserialize<ImportProductDto[]>(inputXml, "Products");

        Product[] products = mapper.Map<Product[]>(productDtos);

        context.Products.AddRange(products);
        context.SaveChanges();

        return $"Successfully imported {products.Length}";
    }

    // 03. Import Categories
    public static string ImportCategories(ProductShopContext context, string inputXml)
    {
        var mapper = CreateMapper();
        var xmlParser = new XmlParser();

        ImportCategoryDto[] categoryDtos = xmlParser.Deserialize<ImportCategoryDto[]>(inputXml, "Categories");

        List<Category> categories = new List<Category>();

        foreach (var categoryDto in categoryDtos)
        {
            if (categoryDto.Name != null)
            {
                Category category = mapper.Map<Category>(categoryDto);
                categories.Add(category);
            }
        }

        context.Categories.AddRange(categories);
        context.SaveChanges();

        return $"Successfully imported {categories.Count}";
    }

    // 04. Import Categories and Products
    public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
    {
        var mapper = CreateMapper();
        var xmlParser = new XmlParser();

        ImportCategoryProductDto[] categoryProductDtos = xmlParser.Deserialize<ImportCategoryProductDto[]>(inputXml, "CategoryProducts");

        List<CategoryProduct> categoryProducts = new List<CategoryProduct>();
        HashSet<int> productIds = context.Products.Select(p => p.Id).ToHashSet<int>();
        HashSet<int> categoryIds = context.Categories.Select(c => c.Id).ToHashSet<int>();

        foreach (var dto in categoryProductDtos)
        {
            if (productIds.Contains(dto.ProductId) && categoryIds.Contains(dto.CategoryId))
            {
                var categoryProduct = mapper.Map<CategoryProduct>(dto);
                categoryProducts.Add(categoryProduct);
            }
        }

        context.CategoryProducts.AddRange(categoryProducts);
        context.SaveChanges();

        return $"Successfully imported {categoryProducts.Count}";
    }

    // 05. Export Products In Range
    public static string GetProductsInRange(ProductShopContext context)
    {
        var xmlParser = new XmlParser();

        var products = context.Products
        .Where(p => p.Price >= 500 && p.Price <= 1000)
        .OrderBy(p => p.Price)
        .Take(10)
        .Select(p => new ExportProductDto
        {
            Name = p.Name,
            Price = p.Price,
            BuyerName = p.Buyer.FirstName + " " + p.Buyer.LastName
        })
        .ToArray();

        return xmlParser.Serialize<ExportProductDto[]>(products, "Products");
    }

    // 06. Export Sold Products
    public static string GetSoldProducts(ProductShopContext context)
    {
        var xmlParser = new XmlParser();

        var products = context.Users
            .Where(u => u.ProductsSold.Count > 0)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Take(5)
            .Select(u => new ExportSoldProductsDto
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                SoldProducts = u.ProductsSold.Select(p => new ProductDto()
                {
                    Name = p.Name,
                    Price = p.Price,
                })
                .ToArray()
            })
            .ToArray();

        return xmlParser.Serialize<ExportSoldProductsDto[]>(products, "Users");
    }

    // 07. Export Categories By Products Count
    public static string GetCategoriesByProductsCount(ProductShopContext context)
    {
        var xmlParser = new XmlParser();

        var categories = context.Categories
       .AsNoTracking()
       .Select(c => new ExportCategoryDto
       {
           Name = c.Name,
           Count = c.CategoryProducts.Count,
           AveragePrice = c.CategoryProducts.Average(cp => cp.Product.Price),
           TotalRevenue = c.CategoryProducts.Sum(cp => cp.Product.Price)
       })
       .OrderByDescending(x => x.Count)
       .ThenBy(x => x.TotalRevenue)
       .ToArray();

        return xmlParser.Serialize<ExportCategoryDto[]>(categories, "Categories");
    }

    // 08. Export Users and Products
    public static string GetUsersWithProducts(ProductShopContext context)
    {
        var xmlParser = new XmlParser();

        var users = context.Users
            .Where(u => u.ProductsSold.Count > 0)
            .OrderByDescending(u => u.ProductsSold.Count)
            .Take(10)
             .Select(u => new UserInfo()
             {
                 FirstName = u.FirstName,
                 LastName = u.LastName,
                 Age = u.Age,
                 SoldProducts = new SoldProductsCount()
                 {
                     Count = u.ProductsSold.Count,
                     Products = u.ProductsSold.Select(p => new SoldProduct()
                     {
                         Name = p.Name,
                         Price = p.Price
                     })
                        .OrderByDescending(p => p.Price)
                        .ToArray()
                 }
             })
                .Take(10)
                .ToArray();

        ExportUsersDto exportUserCountDto = new ExportUsersDto()
        {
            Count = context.Users.Count(u => u.ProductsSold.Any()),
            Users = users
        };

        return xmlParser.Serialize<ExportUsersDto>(exportUserCountDto, "Users");
    }
}