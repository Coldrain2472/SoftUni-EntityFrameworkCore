namespace Cadastre.DataProcessor
{
    using Cadastre.Data;
    using Cadastre.Data.Enumerations;
    using Cadastre.Data.Models;
    using Cadastre.DataProcessor.ImportDtos;
    using Cadastre.Utilities;
    using Newtonsoft.Json;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Text;

    public class Deserializer
    {
        private const string ErrorMessage =
            "Invalid Data!";
        private const string SuccessfullyImportedDistrict =
            "Successfully imported district - {0} with {1} properties.";
        private const string SuccessfullyImportedCitizen =
            "Succefully imported citizen - {0} {1} with {2} properties.";

        public static string ImportDistricts(CadastreContext dbContext, string xmlDocument)
        {
            XmlHelper xmlHelper = new XmlHelper();

            ImportDistrictDto[] districtDtos = xmlHelper.Deserialize<ImportDistrictDto[]>(xmlDocument, "Districts");

            var allDistricts = dbContext.Districts.ToArray();
            var allProperties = dbContext.Properties.ToArray();

            HashSet<District> validDistricts = new HashSet<District>();

            StringBuilder sb = new StringBuilder();

            foreach (var districtDto in districtDtos)
            {
                if (!IsValid(districtDto)
                    || allDistricts.Any(d => d.Name == districtDto.Name)
                    || !Enum.TryParse(districtDto.Region, out Region validRegion))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
               
                District validDistrict = new District()
                {
                    Region = validRegion,
                    Name = districtDto.Name,
                    PostalCode = districtDto.PostalCode
                };

                foreach (var propertyDto in districtDto.Properties)
                {
                    if (!IsValid(propertyDto)
                        || !DateTime.TryParseExact(propertyDto.DateOfAcquisition, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime validDateOfAquistion)
                        || allProperties.Any(p => p.PropertyIdentifier == propertyDto.PropertyIdentifier || p.Address == propertyDto.Address)
                        || validDistrict.Properties.Any(p => p.PropertyIdentifier == propertyDto.PropertyIdentifier || p.Address == propertyDto.Address))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }
                    
                    validDistrict.Properties.Add(new Property()
                    {
                        PropertyIdentifier = propertyDto.PropertyIdentifier,
                        Area = propertyDto.Area,
                        Details = propertyDto.Details,
                        Address = propertyDto.Address,
                        DateOfAcquisition = validDateOfAquistion
                    });
                }

                validDistricts.Add(validDistrict);
                sb.AppendLine(String.Format(SuccessfullyImportedDistrict, validDistrict.Name, validDistrict.Properties.Count));
            }

            dbContext.Districts.AddRange(validDistricts);
            dbContext.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportCitizens(CadastreContext dbContext, string jsonDocument)
        {
            ImportCitizenDto[] citizenDtos = JsonConvert.DeserializeObject<ImportCitizenDto[]>(jsonDocument);

            StringBuilder sb = new StringBuilder();

            HashSet<Citizen> citizens = new HashSet<Citizen>();

            foreach (var citizenDto in citizenDtos)
            {
                if (!IsValid(citizenDto) 
                    || !Enum.TryParse(citizenDto.MaritalStatus, out MaritalStatus validMaritalStatus)
                    || !DateTime.TryParseExact(citizenDto.BirthDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime validBirthDate))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Citizen citizen = new Citizen()
                {
                    FirstName = citizenDto.FirstName,
                    LastName = citizenDto.LastName,
                    BirthDate = validBirthDate,
                    MaritalStatus = validMaritalStatus
                };

                foreach (var propertyId in citizenDto.PropertiesIds)
                {
                    citizen.PropertiesCitizens.Add(new PropertyCitizen()
                    {
                        PropertyId = propertyId
                    });
                }
                
                citizens.Add(citizen);
                sb.AppendLine(String.Format(SuccessfullyImportedCitizen, citizen.FirstName, citizen.LastName, citizen.PropertiesCitizens.Count()));
            }

            dbContext.Citizens.AddRange(citizens);
            dbContext.SaveChanges();

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
