using Invoices.Data.Models.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoices.DataProcessor.ImportDto
{
    public class ImportInvoiceDto
    {
        [JsonProperty("Number")]
        [Required]
        [Range(1000000000, 1500000000)]
        public int Number {  get; set; }

        [JsonProperty("IssueDate")]
        [Required]
        public string IssueDate {  get; set; } = null!;

        [JsonProperty("DueDate")]
        [Required]
        public string DueDate { get; set; } = null!;

        [JsonProperty("Amount")]
        [Required]
        public decimal Amount { get; set; }

        [JsonProperty("CurrencyType")]
        [Required]
        [Range(0, 2)]
        public int CurrencyType { get; set; }

        [JsonProperty("ClientId")]
        [Required]
        public int ClientId { get; set; }
    }
}
