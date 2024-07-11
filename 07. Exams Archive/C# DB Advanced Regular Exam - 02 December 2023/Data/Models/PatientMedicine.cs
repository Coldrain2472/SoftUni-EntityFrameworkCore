namespace Medicines.Data.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PatientMedicine
{
    [Required]
    public int PatientId { get; set; }

    public virtual Patient? Patient { get; set; }

    [Required]
    public int MedicineId {  get; set; }

    public virtual Medicine? Medicine { get; set; }
}
