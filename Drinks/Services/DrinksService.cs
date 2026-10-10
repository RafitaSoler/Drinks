using Drinks.Models;
using System.Net.Http.Json;

namespace Drinks.Services
{
    internal static class DrinksService
    {
        private static HttpClient _client = null!;

        internal static void SetClient(HttpClient client) => _client = client;

        internal static async Task<List<Category>> GetCategoryList()
        {
            Categories? result = await _client.GetFromJsonAsync<Categories>("list.php?c=list");
            return result?.CategoryList ?? new List<Category>();
        }
    }
}
