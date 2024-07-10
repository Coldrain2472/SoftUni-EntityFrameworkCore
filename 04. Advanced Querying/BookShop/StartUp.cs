namespace BookShop;

using BookShop.Models.Enums;
using Data;
using Initializer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text;

public class StartUp
{
    public static void Main()
    {
        using var db = new BookShopContext();
        // DbInitializer.ResetDatabase(db);

        //string input = Console.ReadLine();
        //string result = GetBooksByAgeRestriction(db, input);
        //Console.WriteLine(result);

        //int input = int.Parse(Console.ReadLine());
        //Console.WriteLine(IncreasePrices(db));
    }

    // 02. Age Restriction
    public static string GetBooksByAgeRestriction(BookShopContext context, string command)
    {
        var enumValue = Enum.Parse<AgeRestriction>(command, true);

        var books = context.Books
             .Where(b => b.AgeRestriction == enumValue)
             .Select(b => b.Title)
             .OrderBy(t => t)
             .ToArray();

        return string.Join(Environment.NewLine, books);
    }

    // 03. Golden Books
    public static string GetGoldenBooks(BookShopContext context)
    {
        var editionType = Enum.Parse<EditionType>("Gold", false);

        var goldenBooks = context.Books
            .Where(gb => gb.EditionType == editionType && gb.Copies < 5000)
            .Select(gb => new { gb.BookId, gb.Title })
            .OrderBy(gb => gb.BookId)
            .ToArray();

        return string.Join(Environment.NewLine, goldenBooks.Select(gb => gb.Title));
    }

    // 04. Books by Price
    public static string GetBooksByPrice(BookShopContext context)
    {
        var books = context.Books
            .Where(b => b.Price > 40)
            .Select(b => new { b.Title, b.Price })
            .OrderByDescending(b => b.Price)
            .ToArray();

        StringBuilder sb = new StringBuilder();

        foreach (var book in books)
        {
            sb.AppendLine($"{book.Title} - ${book.Price:f2}");
        }

        return sb.ToString().TrimEnd();
    }

    // 05. Not Released In
    public static string GetBooksNotReleasedIn(BookShopContext context, int year)
    {
        var books = context.Books
            .Where(b => b.ReleaseDate!.Value.Year != year)
            .Select(b => new { b.BookId, b.Title })
            .OrderBy(b => b.BookId)
            .ToArray();

        return string.Join(Environment.NewLine, books.Select(b => b.Title));
    }

    // 06. Book Titles by Category
    public static string GetBooksByCategory(BookShopContext context, string input)
    {
        string[] categories = input.ToLower().Split().ToArray();

        var books = context.BooksCategories
            .Where(bc => categories.Contains(bc.Category.Name.ToLower()))
            .Select(bc => bc.Book.Title)
            .OrderBy(t => t)
            .ToArray();

        return string.Join(Environment.NewLine, books);
    }

    // 07. Released Before Date
    public static string GetBooksReleasedBefore(BookShopContext context, string date)
    {
        DateTime parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", null);

        var books = context.Books
            .Where(b => b.ReleaseDate < parsedDate)
            .Select(b => new { b.Title, b.EditionType, b.Price, b.ReleaseDate })
            .OrderByDescending(b => b.ReleaseDate);

        StringBuilder sb = new StringBuilder();

        foreach (var book in books)
        {
            sb.AppendLine($"{book.Title} - {book.EditionType} - ${book.Price:f2}");
        }

        return sb.ToString().TrimEnd();
    }

    // 08. Author Search
    public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
    {
        var authors = context.Authors
           .Where(a => a.FirstName.EndsWith(input))
           .Select(a => new { FullName = a.FirstName + " " + a.LastName })
           .OrderBy(a => a.FullName)
           .ToArray();

        return string.Join(Environment.NewLine, authors.Select(a => a.FullName));
    }

    // 09. Book Search
    public static string GetBookTitlesContaining(BookShopContext context, string input)
    {
        var books = context.Books
            .Where(b => b.Title.ToLower().Contains(input.ToLower()))
            .Select(b => b.Title)
            .OrderBy(b => b)
            .ToArray();

        return string.Join(Environment.NewLine, books);
    }

    // 10. Book Search by Author
    public static string GetBooksByAuthor(BookShopContext context, string input)
    {
        var books = context.Books
            .Where(b => b.Author.LastName.ToLower().StartsWith(input.ToLower()))
            .OrderBy(b => b.BookId)
            .Select(b => new
            {
                b.Title,
                FullName = b.Author.FirstName + " " + b.Author.LastName
            })
            .ToArray();

        StringBuilder sb = new StringBuilder();

        foreach (var book in books)
        {
            sb.AppendLine($"{book.Title} ({book.FullName})");
        }

        return sb.ToString().TrimEnd();
    }

    // 11. Count Books
    public static int CountBooks(BookShopContext context, int lengthCheck)
    {
        var books = context.Books
            .Where(b => b.Title.Length > lengthCheck)
            .Select(b => new { b.Title.Length })
            .ToArray();

        return books.Count();
    }

    // 12. Total Book Copies
    public static string CountCopiesByAuthor(BookShopContext context)
    {
        var authorsInfo = context.Authors
       .AsNoTracking()
       .Select(a => new
       {
           FullName = a.FirstName + " " + a.LastName,
           BooksCopiesCount = a.Books.Sum(b => b.Copies)
       })
       .OrderByDescending(a => a.BooksCopiesCount)
       .ToList();

        StringBuilder sb = new StringBuilder();

        foreach (var author in authorsInfo)
        {
            sb.AppendLine($"{author.FullName} - {author.BooksCopiesCount}");
        }

        return sb.ToString().TrimEnd();
    }

    // 13. Profit by Category
    public static string GetTotalProfitByCategory(BookShopContext context)
    {
        var info = context.Categories
           .AsNoTracking()
           .Select(c => new
           {
               CategoryName = c.Name,
               Profit = c.CategoryBooks.Sum(bc => bc.Book.Copies * bc.Book.Price)
           })
           .OrderByDescending(c => c.Profit)
           .ThenBy(c => c.CategoryName);

        StringBuilder sb = new StringBuilder();

        foreach (var category in info)
        {
            sb.AppendLine($"{category.CategoryName} ${category.Profit:f2}");
        }

        return sb.ToString().TrimEnd();
    }

    // 14. Most Recent Books
    public static string GetMostRecentBooks(BookShopContext context)
    {
        var categoriesInfo = context.Categories
            .AsNoTracking()
            .Select(c => new
            {
                CategoryName = c.Name,
                Books = c.CategoryBooks.Select(cb => new
                {
                    BookName = cb.Book.Title,
                    ReleaseDate = cb.Book.ReleaseDate
                })
                .OrderByDescending(b => b.ReleaseDate)
                .Take(3)
                .ToArray()
            })
            .OrderBy(c => c.CategoryName)
            .ToArray();

        StringBuilder sb = new StringBuilder();

        foreach (var category in categoriesInfo)
        {
            sb.AppendLine($"--{category.CategoryName}");

            foreach (var book in category.Books)
            {
                sb.AppendLine($"{book.BookName} ({book.ReleaseDate!.Value.Year})");
            }
        }

        return sb.ToString().TrimEnd();
    }

    // 15. Increase Prices
    public static void IncreasePrices(BookShopContext context)
    {
        var books = context.Books
           .Where(b => b.ReleaseDate!.Value.Year < 2010)
           .ToArray();

        foreach (var book in books)
        {
            book.Price += 5;
        }

        context.SaveChanges();
    }

    // 16. Remove Books
    public static int RemoveBooks(BookShopContext context)
    {
        var booksCategoriesToRemove = context.BooksCategories
            .Where(bc => bc.Book.Copies < 4200)
            .ToArray();

        var booksToRemove = context.Books
            .Where(b => b.Copies < 4200)
            .ToArray();

        int removedBooks = booksToRemove.Count();

        context.BooksCategories.RemoveRange(booksCategoriesToRemove);
        context.Books.RemoveRange(booksToRemove);
        context.SaveChanges();

        return removedBooks;
    }
}


