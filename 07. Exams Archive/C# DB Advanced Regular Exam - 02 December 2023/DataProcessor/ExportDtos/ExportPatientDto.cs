namespace Medicines.DataProcessor.ExportDtos;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

[XmlType("Patient")]
public class ExportPatientDto
{
    [XmlAttribute("Gender")]
    public string Gender { get; set; }

    [XmlElement("Name")]
    public string FullName { get; set; }

    [XmlElement("AgeGroup")]
    public string AgeGroup { get; set; }

    [XmlArray("Medicines")]
    public ExportMedicineDto[] Medicines { get; set; }
}

[XmlType("Medicine")]
public class ExportMedicineDto
{
    [XmlAttribute("Category")]
    public string Category { get; set; }

    [XmlElement("Name")]
    public string Name { get; set; }

    [XmlElement("Price")]
    public string Price { get; set; }

    [XmlElement("Producer")]
    public string Producer { get; set; }

    [XmlElement("BestBefore")]
    public string BestBefore { get; set; }
}
