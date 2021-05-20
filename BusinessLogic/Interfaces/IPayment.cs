using System;
using System.Collections.Generic;
using BusinessLogic.BindingModel;
using BusinessLogic.ViewModels;
using System.Text;

namespace BusinessLogic.Interfaces
{
    public interface IPayment
    {
        List<PaymentsViewModels> GetFullList();
        PaymentsViewModels GetElement(PaymentsBindingModel model);
        void Insert(PaymentsBindingModel model);
        void Update(PaymentsBindingModel model);
        List<PaymentsViewModels> GetFilteredList(PaymentsBindingModel model);
    }
}
