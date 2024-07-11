namespace Medicines.Data.Models;

using Medicines.Data.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Medicine
{
    public Medicine()
    {
        this.PatientsMedicines = new HashSet<PatientMedicine>();
    }

    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string Name { get; set; } = null!;

    [Required]
    public decimal Price { get; set; }

    [Required]
    public Category Category { get; set; }

    [Required]
    public DateTime ProductionDate { get; set; }

    [Required]
    public DateTime ExpiryDate { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Producer { get; set; } = null!;

    [ForeignKey("Pharmacy")]
    public int PharmacyId { get; set; }

    public virtual Pharmacy Pharmacy { get; set; }

    public virtual ICollection<PatientMedicine> PatientsMedicines {  get; set; }
}
