using Drinks.Models;
using Drinks.Services;
using Spectre.Console;
using System.Text.Json;

namespace Drinks
{
    internal static class UIController
    {
        private static readonly Category CancelCategory = new() { Name = "Cancel" };
        private static readonly Drink CancelDrink = new() { Name = "Cancel" };

        internal static async Task MainMenu()
        {
            try
            {
                bool exitApp = false;
                List<Category> categories = await AnsiConsole.Status().StartAsync("Fetching the categories...", _ => DrinksService.GetCategories());
                while (!exitApp)
                {
                    Category? category = PromptSelection("Choose a Drink Category", categories, c => c.Name, CancelCategory, "Exit");
                    if (category != null)
                    {
                        List<Drink> drinks = await AnsiConsole.Status().Start("Fetching the drinks...", _ => DrinksService.GetDrinksByCategory(category.Name));
                        Drink? drink = PromptSelection("Choose a Drink", drinks, d => d.Name, CancelDrink, "Go Back");
                    }
                    else
                    {
                        exitApp = true;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                AnsiConsole.MarkupLine($"[red]Could not reach the API:[/] {Markup.Escape(ex.Message)}");
                return;
            }
            catch (OperationCanceledException)
            {
                AnsiConsole.MarkupLine("[red]The request timed out.[/]");
                return;
            }
            catch (JsonException)
            {
                AnsiConsole.MarkupLine("[red]The API sent data in an unexpected format.[/]");
                return;
            }
        }

        private static T? PromptSelection<T>(string title, List<T> items, Func<T, string> label, T cancel, string exitMessage) where T : class
        {
            T selected = AnsiConsole.Prompt(
                new SelectionPrompt<T>()
                .Title(title)
                .HighlightStyle(Style.Parse("cyan"))
                .UseConverter(item => ReferenceEquals(item, cancel) ? $"[grey]{exitMessage}[/]" : Markup.Escape(label(item)))
                .AddChoices(items)
                .AddChoices(cancel)
                .PageSize(Math.Max(3, Console.WindowHeight - 4))
                .EnableSearch()
                .WrapAround());

            return ReferenceEquals(selected, cancel) ? null : selected;
        }
    }
}
