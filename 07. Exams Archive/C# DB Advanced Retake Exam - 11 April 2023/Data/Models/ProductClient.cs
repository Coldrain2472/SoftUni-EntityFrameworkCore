using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoices.Data.Models
{
    public class ProductClient
    {
        public int ProductId { get; set; }

        public virtual Product? Product { get; set; }

        public int ClientId { get; set; }

        public virtual Client? Client { get; set; }
    }
}
