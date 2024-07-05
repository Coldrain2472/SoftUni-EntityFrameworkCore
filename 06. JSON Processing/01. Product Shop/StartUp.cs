namespace ProductShop;

using ProductShop.Data;
using Newtonsoft.Json;
using ProductShop.DTOs.Import;
using ProductShop.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

public class StartUp
{
    public static void Main()
    {
        ProductShopContext context = new ProductShopContext();

       // string inputJson = File.ReadAllText(@"../../../Datasets/products.json");
        string result = GetUsersWithProducts(context);
        Console.WriteLine(result);
    }

    // 01. Import Users
    public static string ImportUsers(ProductShopContext context, string inputJson)
    {
        IMapper mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProductShopProfile>();
        }));

        ImportUserDto[] userDtos = JsonConvert.DeserializeObject<ImportUserDto[]>(inputJson);

        ICollection<User> validUsers = new HashSet<User>();

        foreach (ImportUserDto userDto in userDtos)
        {
            User user = mapper.Map<User>(userDto);
            validUsers.Add(user);
        }

        context.Users.AddRange(validUsers);
        context.SaveChanges();

        return $"Successfully imported {validUsers.Count}";
    }

    // 02. Import Products
    public static string ImportProducts(ProductShopContext context, string inputJson)
    {
        IMapper mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProductShopProfile>();
        }));

        ImportProductDto[] productDtos = JsonConvert.DeserializeObject<ImportProductDto[]>(inputJson);

        Product[] products = mapper.Map<Product[]>(productDtos);

        context.Products.AddRange(products);
        context.SaveChanges();

        return $"Successfully imported {products.Length}";
    }

    // 03. Import Categories
    public static string ImportCategories(ProductShopContext context, string inputJson)
    {
        IMapper mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProductShopProfile>();
        }));

        ImportCategoryDto[] allCategoryDtos = JsonConvert.DeserializeObject<ImportCategoryDto[]>(inputJson);

        var categoriesDto = allCategoryDtos
           .Where(c => c.Name != null)
           .ToArray();

        Category[] categories = mapper.Map<Category[]>(categoriesDto);

        context.Categories.AddRange(categories);
        context.SaveChanges();

        return $"Successfully imported {categories.Length}";
    }

    // 04. Import Categories and Products
    public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
    {
        IMapper mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProductShopProfile>();
        }));

        ImportCategoryProductDto[] categoriesAndProductsDto = JsonConvert.DeserializeObject<ImportCategoryProductDto[]>(inputJson);

        var categoriesAndProducts = mapper.Map<CategoryProduct[]>(categoriesAndProductsDto);

        context.CategoriesProducts.AddRangeAsync(categoriesAndProducts);
        context.SaveChanges();

        return $"Successfully imported {categoriesAndProducts.Length}";
    }

    // 05. Export Products In Range
    public static string GetProductsInRange(ProductShopContext context)
    {
        var products = context.Products
           .AsNoTracking()
           .Where(p => p.Price >= 500 && p.Price <= 1000)
           .OrderBy(p => p.Price)
           .Select(p => new
           {
               name = p.Name,
               price = p.Price,
               seller = p.Seller.FirstName + " " + p.Seller.LastName,
           })
           .AsNoTracking()
           .ToArray();

        return JsonConvert.SerializeObject(products, Formatting.Indented);
    }

    // 06. Export Sold Products
    public static string GetSoldProducts(ProductShopContext context)
    {
        var usersProducts = context.Users
            .Where(u => u.ProductsSold.Any(ps => ps.BuyerId != null))
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .AsNoTracking()
            .Select(u => new
            {
                firstName = u.FirstName,
                lastName = u.LastName,
                soldProducts = u.ProductsSold
                    .Select(p => new
                    {
                        name = p.Name,
                        price = p.Price,
                        buyerFirstName = p.Buyer.FirstName,
                        buyerLastName = p.Buyer.LastName

                    })
                    .ToArray()

            })
            .ToArray();

        return JsonConvert.SerializeObject(usersProducts, Formatting.Indented);
    }

    // 07. Export Categories By Products Count
    public static string GetCategoriesByProductsCount(ProductShopContext context)
    {
        var categories = context.Categories
           .AsNoTracking()
           .OrderByDescending(c => c.CategoriesProducts.Count)
           .Select(c => new
           {
               category = c.Name,
               productsCount = c.CategoriesProducts.Count,
               averagePrice = c.CategoriesProducts.Average(cp => cp.Product.Price).ToString("f2"),
               totalRevenue = c.CategoriesProducts.Sum(cp => cp.Product.Price).ToString("f2")
           })
           .ToArray();

        return JsonConvert.SerializeObject(categories, Formatting.Indented);
    }

    // 08. Export Users and Products
    public static string GetUsersWithProducts(ProductShopContext context)
    {
        var users = context.Users
                .Where(u => u.ProductsSold.Any(p => p.BuyerId != null))
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    age = u.Age,
                    soldProducts = new
                    {
                        count = u.ProductsSold.Count(p => p.BuyerId != null),
                        products = u.ProductsSold
                            .Where(p => p.BuyerId != null)
                            .Select(p => new
                            {
                                name = p.Name,
                                price = p.Price
                            })
                            .ToArray()
                    }
                })
                .AsNoTracking()
                .OrderByDescending(x => x.soldProducts.count)
                .ToArray();

        var resultUsers = new
        {
            usersCount = users.Length,
            users = users
        };

        return JsonConvert.SerializeObject(resultUsers, Formatting.Indented, new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
        });
    }
}