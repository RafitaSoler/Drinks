using System.Text.Json.Serialization;

namespace Drinks.Models
{
    internal class Categories
    {
        [JsonPropertyName("drinks")]
        public List<Category>? CategoryList { get; set; }
    }

    internal class Category
    {
        [JsonPropertyName("strCategory")] public string CategoryName { get; set; } = "";
    }
}