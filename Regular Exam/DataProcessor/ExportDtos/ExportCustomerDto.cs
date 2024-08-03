using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelAgency.DataProcessor.ExportDtos
{
    public class ExportCustomerDto
    {
        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public List<ExportBookingDto> Bookings {  get; set; }
    }

    public class ExportBookingDto
    {
        public string TourPackageName { get; set; }

        public string Date {  get; set; }
    }
}
