namespace Invoices.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Text;
    using Invoices.Data;
    using Invoices.Data.Models;
    using Invoices.Data.Models.Enums;
    using Invoices.DataProcessor.ImportDto;
    using Invoices.Utilities;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedClients
            = "Successfully imported client {0}.";

        private const string SuccessfullyImportedInvoices
            = "Successfully imported invoice with number {0}.";

        private const string SuccessfullyImportedProducts
            = "Successfully imported product - {0} with {1} clients.";


        public static string ImportClients(InvoicesContext context, string xmlString)
        {
            XmlHelper xmlHelper = new XmlHelper();

            ImportClientDto[] clientDtos = xmlHelper.Deserialize<ImportClientDto[]>(xmlString, "Clients");

            StringBuilder sb = new StringBuilder();

            HashSet<Client> clients = new HashSet<Client>();

            foreach (var clientDto in clientDtos)
            {
                if (!IsValid(clientDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                
                Client client = new Client()
                {
                    Name = clientDto.Name,
                    NumberVat = clientDto.NumberVat
                };

                foreach (var addressDto in clientDto.Addresses)
                {
                    if (!IsValid(addressDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Address address = new Address()
                    {
                        StreetName = addressDto.StreetName,
                        StreetNumber = addressDto.StreetNumber,
                        PostCode = addressDto.PostCode,
                        City = addressDto.City,
                        Country = addressDto.Country,
                    };

                    client.Addresses.Add(address);
                }

                clients.Add(client);
                sb.AppendLine(String.Format(SuccessfullyImportedClients, client.Name));
            }

            context.Clients.AddRange(clients);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }


        public static string ImportInvoices(InvoicesContext context, string jsonString)
        {
            ImportInvoiceDto[] invoiceDtos = JsonConvert.DeserializeObject<ImportInvoiceDto[]>(jsonString);

            StringBuilder sb = new StringBuilder();

            HashSet<Invoice> invoices = new HashSet<Invoice>();

            int[] clientsIds = context.Clients
                .Select(c => c.Id)
                .ToArray();

            foreach (var invoiceDto in invoiceDtos)
            {
                if (!IsValid(invoiceDto) || !clientsIds.Contains(invoiceDto.ClientId))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime issueDate = DateTime.ParseExact(invoiceDto.IssueDate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                DateTime dueDate = DateTime.ParseExact(invoiceDto.DueDate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);

                if (issueDate > dueDate)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Invoice invoice = new Invoice()
                {
                    Number = invoiceDto.Number,
                    IssueDate = issueDate,
                    DueDate = dueDate,
                    Amount = invoiceDto.Amount,
                    CurrencyType = (CurrencyType)invoiceDto.CurrencyType,
                    ClientId = invoiceDto.ClientId
                };

                invoices.Add(invoice);
                sb.AppendLine(String.Format(SuccessfullyImportedInvoices, invoice.Number));
            }

            context.Invoices.AddRange(invoices);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportProducts(InvoicesContext context, string jsonString)
        {
            ImportProductDto[] productDtos = JsonConvert.DeserializeObject<ImportProductDto[]>(jsonString);

            StringBuilder sb = new StringBuilder();

            HashSet<Product> products = new HashSet<Product>();

            int[] clientsIds = context.Clients
                .Select(c=>c.Id)
                .ToArray();

            foreach (var productDto in productDtos) 
            {
                if (!IsValid(productDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Product product = new Product()
                {
                    Name = productDto.Name,
                    Price = productDto.Price,
                    CategoryType = (CategoryType)productDto.CategoryType
                };

                foreach (var clientId in productDto.Clients.Distinct())
                {
                    if (!clientsIds.Contains(clientId))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    product.ProductsClients.Add(new ProductClient()
                    {
                        ClientId = clientId
                    });
                }

                products.Add(product);

                sb.AppendLine(String.Format(SuccessfullyImportedProducts, product.Name, product.ProductsClients.Count));
            }

            context.Products.AddRange(products);
            context.SaveChanges();

            return sb.ToString().TrimEnd();

        }

        public static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    } 
}
