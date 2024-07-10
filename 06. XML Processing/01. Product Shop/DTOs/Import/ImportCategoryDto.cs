namespace ProductShop.DTOs.Import;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

[XmlType("Category")]
public class ImportCategoryDto
{
    [XmlElement("name")]
    public string Name { get; set; } = null!;
}
