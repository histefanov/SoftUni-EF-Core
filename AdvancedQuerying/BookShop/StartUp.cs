namespace BookShop
{
    using BookShop.Models.Enums;
    using Data;
    using Initializer;
    using System;
    using System.Linq;
    using System.Text;

    public class StartUp
    {
        public static void Main()
        {
            var db = new BookShopContext();
            //DbInitializer.ResetDatabase(db);

            //var input = Console.ReadLine();
            var result = GetBooksByCategory(db, "horror mystery drama");

            Console.WriteLine(result);
        }

        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            var ageRestriction = Enum.Parse<AgeRestriction>(command, true);

            var bookTitles = context.Books
                .Where(x => x.AgeRestriction == ageRestriction)
                .Select(x => x.Title)
                .OrderBy(x => x)
                .ToList();

            var sb = new StringBuilder();

            foreach (var title in bookTitles)
            {
                sb.AppendLine(title);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetGoldenBooks(BookShopContext context)
        {
            var goldenBooks = context.Books
                .Where(x => x.EditionType == EditionType.Gold && x.Copies < 5000)
                .Select(x => new { x.Title, x.BookId })
                .OrderBy(x => x.BookId)
                .ToList();

            var sb = new StringBuilder();

            foreach (var book in goldenBooks)
            {
                sb.AppendLine(book.Title);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksByPrice(BookShopContext context)
        {
            var booksByPrice = context.Books
                .Where(x => x.Price > 40)
                .Select(x => new { x.Title, x.Price })
                .OrderByDescending(x => x.Price)
                .ToList();

            var sb = new StringBuilder();

            foreach (var book in booksByPrice)
            {
                sb.AppendLine($"{book.Title} - ${book.Price:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var books = context.Books
                .Where(x => x.ReleaseDate.Value.Year != year)
                .Select(x => new { x.BookId, x.Title })
                .OrderBy(x => x.BookId)
                .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine(book.Title);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            var categories = input.ToLower();

            var books = context.Books
                .Where(x => x.BookCategories.Any(c => categories.Contains(c.Category.Name.ToLower())))
                .Select(x => x.Title)
                .OrderBy(x => x)
                .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine(book);
            }

            return sb.ToString().TrimEnd();
        }
    }
}
