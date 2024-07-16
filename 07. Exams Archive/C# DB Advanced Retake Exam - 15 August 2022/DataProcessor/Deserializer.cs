namespace Trucks.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.SqlTypes;
    using System.Text;
    using Castle.Core.Internal;
    using Data;
    using Newtonsoft.Json;
    using Trucks.Data.Models;
    using Trucks.Data.Models.Enums;
    using Trucks.DataProcessor.ImportDto;
    using Trucks.Utilities;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedDespatcher
            = "Successfully imported despatcher - {0} with {1} trucks.";

        private const string SuccessfullyImportedClient
            = "Successfully imported client - {0} with {1} trucks.";

        public static string ImportDespatcher(TrucksContext context, string xmlString)
        {
            XmlHelper xmlHelper = new XmlHelper();
            ImportDespatcherDto[] despatcherDtos = xmlHelper.Deserialize<ImportDespatcherDto[]>(xmlString, "Despatchers");

            HashSet<Despatcher> validDespatchers = new HashSet<Despatcher>();
            StringBuilder sb = new StringBuilder();

            foreach (var despatcherDto in despatcherDtos)
            {
                if (!IsValid(despatcherDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                if (String.IsNullOrEmpty(despatcherDto.Position))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Despatcher validDespatcher = new Despatcher()
                {
                    Name = despatcherDto.Name,
                    Position = despatcherDto.Position
                };

                foreach (var truckDto in despatcherDto.Trucks)
                {
                    if (!IsValid(truckDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }
                    if (String.IsNullOrEmpty(truckDto.RegistrationNumber))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Truck validTruck = new Truck()
                    {
                        RegistrationNumber = truckDto.RegistrationNumber,
                        VinNumber = truckDto.VinNumber,
                        TankCapacity = truckDto.TankCapacity,
                        CargoCapacity = truckDto.CargoCapacity,
                        CategoryType = (CategoryType)truckDto.CategoryType,
                        MakeType = (MakeType)truckDto.MakeType,
                    };

                    validDespatcher.Trucks.Add(validTruck);
                }

                validDespatchers.Add(validDespatcher);

                sb.AppendLine(String.Format(SuccessfullyImportedDespatcher, validDespatcher.Name, validDespatcher.Trucks.Count));
            }

            context.Despatchers.AddRange(validDespatchers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportClient(TrucksContext context, string jsonString)
        {
            ImportClientDto[] clientDtos = JsonConvert.DeserializeObject<ImportClientDto[]>(jsonString);

            HashSet<Client> validClients = new HashSet<Client>();
            int[] trucksIds = context.Trucks.Select(t => t.Id).ToArray();
            StringBuilder sb = new StringBuilder();

            foreach (var clientDto in clientDtos)
            {
                if (!IsValid(clientDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                if (clientDto.Type.ToLower() == "usual")
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Client validClient = new Client()
                {
                    Name = clientDto.Name,
                    Nationality = clientDto.Nationality,
                    Type = clientDto.Type
                };

                foreach (var truckId in clientDto.TrucksIds.Distinct())
                {
                    if (!trucksIds.Contains(truckId))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    validClient.ClientsTrucks.Add(new ClientTruck()
                    {
                        TruckId = truckId
                    });
                }

                validClients.Add(validClient);

                sb.AppendLine(String.Format(SuccessfullyImportedClient, validClient.Name, validClient.ClientsTrucks.Count()));
            }

            context.Clients.AddRange(validClients);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}