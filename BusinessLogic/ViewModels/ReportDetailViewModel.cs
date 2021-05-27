using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogic.ViewModels
{
    public class ReportDetailViewModel
    {
        public int Id { get; set; }
        public Enum.StatusVisit Status { get; set; }

        public decimal Summ { get; set; }
        public List<Tuple<DateTime, decimal>> DetailsPay { get; set; }
        public DateTime Date { get; set; }
    }
}
