namespace CarDealer.DTOs.Export;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

[XmlType("car")]
public class ExportCarPartsDto
{
    [XmlAttribute("make")]
    public string Make { get; set; } = null!;

    [XmlAttribute("model")]
    public string Model { get; set; } = null!;

    [XmlAttribute("traveled-distance")]
    public long TraveledDistance { get; set; }

    [XmlArray("parts")]
    public PartsDto[] Parts { get; set; }
}

[XmlType("part")]
public class PartsDto
{
    [XmlAttribute("name")]
    public string Name { get; set; } = null!;

    [XmlAttribute("price")]
    public decimal Price { get; set; }
}
