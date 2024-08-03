using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using TravelAgency.Data;
using TravelAgency.Data.Models;
using TravelAgency.DataProcessor.ImportDtos;
using TravelAgency.Utilities;

namespace TravelAgency.DataProcessor
{
    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data format!";
        private const string DuplicationDataMessage = "Error! Data duplicated.";
        private const string SuccessfullyImportedCustomer = "Successfully imported customer - {0}";
        private const string SuccessfullyImportedBooking = "Successfully imported booking. TourPackage: {0}, Date: {1}";

        public static string ImportCustomers(TravelAgencyContext context, string xmlString)
        {
            XmlHelper xmlHelper = new XmlHelper();

            ImportCustomerDto[] customerDtos = xmlHelper.Deserialize<ImportCustomerDto[]>(xmlString, "Customers");

            HashSet<Customer> validCustomers = new HashSet<Customer>();

            StringBuilder sb = new StringBuilder();

            foreach (var customerDto in customerDtos)
            {
                if (!IsValid(customerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (context.Customers.Any(c => c.FullName == customerDto.FullName || c.Email == customerDto.Email || c.PhoneNumber == customerDto.PhoneNumber) || validCustomers.Any(c=>c.FullName == customerDto.FullName || c.PhoneNumber == customerDto.PhoneNumber || c.Email == customerDto.Email))
                {
                    sb.AppendLine(DuplicationDataMessage);
                    continue;
                }

                var customer = new Customer
                {
                    FullName = customerDto.FullName,
                    Email = customerDto.Email,
                    PhoneNumber = customerDto.PhoneNumber
                };

                validCustomers.Add(customer);
                sb.AppendLine(string.Format(SuccessfullyImportedCustomer, customerDto.FullName));
            }

            context.Customers.AddRange(validCustomers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }


        public static string ImportBookings(TravelAgencyContext context, string jsonString)
        {
            ImportBookingDto[] bookingDtos = JsonConvert.DeserializeObject<ImportBookingDto[]>(jsonString);

            HashSet<Booking> validBookings = new HashSet<Booking>();

            StringBuilder sb = new StringBuilder();

            foreach (var bookingDto in bookingDtos)
            {
                if (!DateTime.TryParseExact(bookingDto.BookingDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime bookingDate))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var customer = context.Customers.FirstOrDefault(c => c.FullName == bookingDto.CustomerName);
                var tourPackage = context.TourPackages.FirstOrDefault(tp => tp.PackageName == bookingDto.TourPackageName);

                if (customer == null || tourPackage == null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var booking = new Booking
                {
                    BookingDate = bookingDate,
                    CustomerId = customer.Id,
                    TourPackageId = tourPackage.Id
                };

                validBookings.Add(booking);
                sb.AppendLine(string.Format(SuccessfullyImportedBooking, bookingDto.TourPackageName, bookingDate.ToString("yyyy-MM-dd")));
            }

            context.Bookings.AddRange(validBookings);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }


        public static bool IsValid(object dto)
        {
            var validateContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(dto, validateContext, validationResults, true);

            foreach (var validationResult in validationResults)
            {
                string currValidationMessage = validationResult.ErrorMessage;
            }

            return isValid;
        }
    }
}
