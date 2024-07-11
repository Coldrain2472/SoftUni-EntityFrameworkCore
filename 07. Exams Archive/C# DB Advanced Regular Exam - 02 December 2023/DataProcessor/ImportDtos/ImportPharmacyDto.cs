namespace Medicines.DataProcessor.ImportDtos;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

[XmlType("Pharmacy")]
public class ImportPharmacyDto
{
    [XmlAttribute("non-stop")]
    [Required]
    public string IsNonStop { get; set; }

    [XmlElement("Name")]
    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    public string Name { get; set; }

    [XmlElement("PhoneNumber")]
    [Required]
    [RegularExpression(@"^\(\d{3}\) \d{3}-\d{4}$")]
    public string PhoneNumber { get; set; }

    [XmlArray("Medicines")]
    public ImportMedicineDto[] Medicines { get; set; }
}

[XmlType("Medicine")]
public class ImportMedicineDto
{
    [XmlAttribute("category")]
    [Required]
    [Range(0, 4)]
    public int Category { get; set; }

    [XmlElement("Name")]
    [Required]
    [MinLength(3)]
    [MaxLength(150)]
    public string Name { get; set; }

    [XmlElement("Price")]
    [Required]
    [Range(0.01, 1000.0)]
    public decimal Price { get; set; }

    [XmlElement("ProductionDate")]
    [Required]
    public string ProductionDate { get; set; }

    [XmlElement("ExpiryDate")]
    [Required]
    public string ExpiryDate { get; set; }

    [XmlElement("Producer")]
    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public string Producer { get; set; }
}