using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.EF.CustomModel
{
    public class Report
    {
        public int Quantity { get; set; }
        public decimal ProductPrice { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
