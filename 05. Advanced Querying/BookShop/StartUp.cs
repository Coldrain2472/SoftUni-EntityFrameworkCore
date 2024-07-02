namespace BookShop;

using BookShop.Models.Enums;
using Data;
using Initializer;
using Microsoft.EntityFrameworkCore;
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

        string input = Console.ReadLine();
        Console.WriteLine(GetBooksReleasedBefore(db, input));
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
            .Where(b=>b.ReleaseDate!.Value.Year != year)
            .Select(b => new {b.BookId, b.Title})
            .OrderBy(b=>b.BookId)
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

}


