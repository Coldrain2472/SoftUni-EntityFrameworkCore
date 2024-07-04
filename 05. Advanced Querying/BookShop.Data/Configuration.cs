namespace BookShop.Data;

internal class Configuration
{
    internal static string ConnectionString
        => @".\SQLEXPRESS;Database=BookShop;Integrated Security=True;";
}
