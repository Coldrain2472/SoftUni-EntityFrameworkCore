using Cadastre.Data;
using Cadastre.DataProcessor.ExportDtos;
using Cadastre.Utilities;
using Newtonsoft.Json;
using System.Globalization;

namespace Cadastre.DataProcessor
{
    public class Serializer
    {
        public static string ExportPropertiesWithOwners(CadastreContext dbContext)
        {
            DateTime dateOfAcquisition = DateTime.ParseExact("01/01/2000", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);

            var properties = dbContext.Properties
                .AsEnumerable()
                .Where(p=>p.DateOfAcquisition >= dateOfAcquisition)
                .OrderByDescending(p=>p.DateOfAcquisition)
                .ThenBy(p=>p.PropertyIdentifier)
                .Select(p=> new
                {
                    PropertyIdentifier = p.PropertyIdentifier,
                    Area = p.Area,
                    Address = p.Address,
                    DateOfAcquisition = p.DateOfAcquisition.ToString("dd/MM/yyyy"),
                    Owners = p.PropertiesCitizens
                    .Select(pc => new
                    {
                        LastName = pc.Citizen.LastName,
                        MaritalStatus = pc.Citizen.MaritalStatus.ToString()
                    })
                    .OrderBy(c => c.LastName)
                    .ToArray()
                })
                .ToArray();

            return JsonConvert.SerializeObject(properties, Formatting.Indented);
        }

        public static string ExportFilteredPropertiesWithDistrict(CadastreContext dbContext)
        {
            var properties = dbContext.Properties 
                .AsEnumerable()
                .Where(p=>p.Area >= 100)
                .OrderByDescending(p=>p.Area)
                .ThenBy(p=>p.DateOfAcquisition)
                .Select(p => new ExportPropertyDto()
                {
                    PostalCode = p.District.PostalCode,
                    PropertyIdentifier = p.PropertyIdentifier,
                    Area = p.Area,
                    DateOfAcquisition = p.DateOfAcquisition.ToString("dd/MM/yyyy")
                })
                .ToArray();

            XmlHelper xmlHelper = new XmlHelper();

            return xmlHelper.Serialize<ExportPropertyDto[]>(properties, "Properties");
        }
    }
}
