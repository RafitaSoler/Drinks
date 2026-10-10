using Drinks.Models;
using Drinks.Services;
using Spectre.Console;
using System.Reflection;
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
                    AnsiConsole.Clear();
                    Category? category = PromptSelection("Choose a Drink Category", categories, c => c.Name, CancelCategory, "Exit");
                    if (category != null)
                    {
                        List<Drink> drinks = await AnsiConsole.Status().StartAsync("Fetching the drinks...", _ => DrinksService.GetDrinksByCategory(category.Name));
                        Drink? drink = PromptSelection("Choose a Drink", drinks, d => d.Name, CancelDrink, "Go Back");
                        if (drink != null)
                        {
                            DrinkDetail? drinkDetail = await AnsiConsole.Status().StartAsync("Fetching the drink details...", _ => DrinksService.GetDrinkById(drink.Id));
                            if (drinkDetail != null)
                            {
                                DrawDrinkDetailTable(drinkDetail);
                            }
                        }
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

        private static void DrawDrinkDetailTable(DrinkDetail drinkDetail)
        {
            Table table = new Table().AddColumns("Field", "Value");
            foreach (PropertyInfo prop in typeof(DrinkDetail).GetProperties())
            {
                string? value = prop.GetValue(drinkDetail)?.ToString();
                if (string.IsNullOrWhiteSpace(value)) continue;
                table.AddRow(Markup.Escape(prop.Name), Markup.Escape(value));
            }
            AnsiConsole.Clear();
            AnsiConsole.Write(table);
            PromptGoBack();
        }

        private static void PromptGoBack()
        {
            AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .HighlightStyle(Style.Parse("cyan"))
                .AddChoices("Go Back")
            );
        }
    }
}
