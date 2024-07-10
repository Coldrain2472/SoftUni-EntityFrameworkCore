namespace ProductShop.DTOs.Import;

using Newtonsoft.Json;

public class ImportCategoryProductDto
{
    [JsonProperty("CategoryId")]
    public int CategoryId {  get; set; }

    [JsonProperty("ProductId")]
    public int ProductId { get; set; }
}
