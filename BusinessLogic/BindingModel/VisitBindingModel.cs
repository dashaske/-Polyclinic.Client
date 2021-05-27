using System;
using System.Collections.Generic;
using BusinessLogic.Enum;
using System.Text;

namespace BusinessLogic.BindingModel
{
    public class VisitBindingModel
    {
        public int? Id { get; set; }
        public int ClientId { get; set; }
        public decimal Summ { get; set; }
        public DateTime Date { get; set; }
        public StatusVisit Status { get; set; }
        public Dictionary<int, int> visitInspections { get; set; }// costid, cena
        public Dictionary<int, decimal> visitPayments { get; set; }// costid, cena
        public List<int> Selected { get; set; }
    }
}
