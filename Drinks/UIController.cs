using Drinks.Models;
using Drinks.Services;
using Spectre.Console;

namespace Drinks
{
    internal static class UIController
    {
        private static readonly Category CancelOption = new() { CategoryName = "Cancel" };

        internal static async Task MainMenu()
        {
            List<Category> categories = await AnsiConsole.Status().StartAsync("Fetching the categories...", _ => DrinksService.GetCategoryList());
            Category? category = PromptCategorySelection(categories);
        }

        private static Category? PromptCategorySelection(List<Category> categories)
        {
            Category category = AnsiConsole.Prompt(
                new SelectionPrompt<Category>()
                .Title("Choose a Drink Category")
                .HighlightStyle(Style.Parse("cyan"))
                .UseConverter(c => c == CancelOption ? "[grey]Cancel[/]" : Markup.Escape($"{c.CategoryName}"))
                .AddChoices(categories)
                .AddChoices(CancelOption)
                .PageSize(Console.WindowHeight - 4)
                .WrapAround()
            );
            return category == CancelOption ? null : category;
        }
    }
}
