namespace Medicines.Data.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Pharmacy
{
    public Pharmacy()
    {
        this.Medicines = new HashSet<Medicine>();
    }

    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(14)]
    [RegularExpression(@"\(\d{3}\) \d{3}-\d{4}")]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    public bool IsNonStop { get; set; }

    public virtual ICollection<Medicine> Medicines { get; set; }
}
