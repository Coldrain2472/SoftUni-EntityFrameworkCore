namespace Medicines.Data.Models;

using Medicines.Data.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Patient
{
    public Patient()
    {
        this.PatientsMedicines = new HashSet<PatientMedicine>();    
    }

    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 5)]
    public string FullName { get; set; } = null!;

    [Required]
    public AgeGroup AgeGroup { get; set; }

    [Required]
    public Gender Gender { get; set; }

    public virtual ICollection<PatientMedicine> PatientsMedicines {get;set;}
}
