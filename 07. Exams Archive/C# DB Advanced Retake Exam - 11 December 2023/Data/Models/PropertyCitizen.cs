using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cadastre.Data.Models
{
    public class PropertyCitizen
    {
        [ForeignKey(nameof(Property))]
        public int PropertyId { get; set; }

        public virtual Property? Property { get; set; }

        [ForeignKey(nameof(Citizen))]
        public int CitizenId { get; set; }

        public virtual Citizen? Citizen { get; set; }
    }
}
