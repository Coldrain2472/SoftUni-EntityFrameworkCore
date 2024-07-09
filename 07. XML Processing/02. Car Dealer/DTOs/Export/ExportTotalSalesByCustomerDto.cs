namespace CarDealer.DTOs.Export;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

[XmlType("customer")]
public class ExportTotalSalesByCustomerDto
{
    [XmlAttribute("full-name")]
    public string FullName { get; set; }

    [XmlAttribute("bought-cars")]
    public int BoughtCars { get; set; }

    [XmlAttribute("spent-money")]
    public string SpentMoney { get; set; }
}