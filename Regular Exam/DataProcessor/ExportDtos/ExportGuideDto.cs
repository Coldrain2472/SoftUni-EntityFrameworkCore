using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TravelAgency.DataProcessor.ExportDtos
{
    [XmlType("Guide")]
    public class ExportGuideDto
    {
        public string FullName { get; set; }

        public List<ExportTourPackageDto> TourPackages {  get; set; }
    }

    [XmlType("TourPackage")]
    public class ExportTourPackageDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }
    }
}
