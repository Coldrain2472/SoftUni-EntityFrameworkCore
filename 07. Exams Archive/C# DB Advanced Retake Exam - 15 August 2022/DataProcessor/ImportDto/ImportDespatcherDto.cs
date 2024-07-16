using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Trucks.DataProcessor.ImportDto;

[XmlType("Despatcher")]
public class ImportDespatcherDto
{
    [XmlElement("Name")]
    [Required]
    [MinLength(2)]
    [MaxLength(40)]
    public string Name { get; set; }

    [XmlElement("Position")]
    public string Position { get; set; }

    [XmlArray("Trucks")]
    public ImportTruckDto[] Trucks { get; set; }
}

[XmlType("Truck")]
public class ImportTruckDto
{
    [XmlElement("RegistrationNumber")]
    [RegularExpression(@"^[A-Z]{2}\d{4}[A-Z]{2}$")]
    public string RegistrationNumber { get; set; }

    [XmlElement("VinNumber")]
    [Required]
    [MinLength(17)]
    [MaxLength(17)]
    public string VinNumber { get; set; }

    [XmlElement("TankCapacity")]
    [Range(950, 1420)]
    public int TankCapacity { get; set; }

    [XmlElement("CargoCapacity")]
    [Range(5000, 29000)]
    public int CargoCapacity { get; set; }

    [XmlElement("CategoryType")]
    [Required]
    [Range(0, 3)]
    public int CategoryType { get; set; }

    [XmlElement("MakeType")]
    [Required]
    [Range(0, 4)]
    public int MakeType { get; set; }
}