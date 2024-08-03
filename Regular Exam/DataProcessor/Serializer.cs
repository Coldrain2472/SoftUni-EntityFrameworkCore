using Newtonsoft.Json;
using TravelAgency.Data;
using TravelAgency.Data.Models.Enums;
using TravelAgency.DataProcessor.ExportDtos;
using TravelAgency.Utilities;

namespace TravelAgency.DataProcessor
{
    public class Serializer
    {
        public static string ExportGuidesWithSpanishLanguageWithAllTheirTourPackages(TravelAgencyContext context)
        {
            var guides = context.Guides
                .Where(g => g.Language == Language.Spanish)
                .Select(g => new ExportGuideDto
                {
                    FullName = g.FullName,
                    TourPackages = g.TourPackagesGuides
                        .Select(tpg => tpg.TourPackage)
                        .OrderByDescending(tp => tp.Price)
                        .ThenBy(tp => tp.PackageName)
                        .Select(tp => new ExportTourPackageDto
                        {
                            Name = tp.PackageName,
                            Description = tp.Description,
                            Price = tp.Price
                        })
                        .ToList()
                })
                .OrderByDescending(g => g.TourPackages.Count)
                .ThenBy(g => g.FullName)
                .ToList();

            XmlHelper xmlHelper = new XmlHelper();

            return xmlHelper.Serialize<List<ExportGuideDto>>(guides, "Guides");
        }

        public static string ExportCustomersThatHaveBookedHorseRidingTourPackage(TravelAgencyContext context)
        {
            var customers = context.Customers
                .Where(c => c.Bookings.Any(b => b.TourPackage.PackageName == "Horse Riding Tour"))
                .Select(c => new ExportCustomerDto
                {
                    FullName = c.FullName,
                    PhoneNumber = c.PhoneNumber,
                    Bookings = c.Bookings
                        .Where(b => b.TourPackage.PackageName == "Horse Riding Tour")
                        .OrderBy(b => b.BookingDate)
                        .Select(b => new ExportBookingDto
                        {
                            TourPackageName = b.TourPackage.PackageName,
                            Date = b.BookingDate.ToString("yyyy-MM-dd")
                        })
                        .ToList()
                })
                .OrderByDescending(c => c.Bookings.Count)
                .ThenBy(c => c.FullName)
                .ToList();

            return JsonConvert.SerializeObject(customers, Formatting.Indented);
        }
    }
}
