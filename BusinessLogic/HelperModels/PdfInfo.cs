using System;
using System.Collections.Generic;
using BusinessLogic.ViewModels;
using System.Text;

namespace BusinessLogic.HelperModels
{
    public class PdfInfo
    {
        public string FileName { get; set; }
        public string Title { get; set; }
        public List<VisitViewModels> Visit { get; set; }
        public List<PaymentsViewModels> Payment { get; set; }
    }
}
