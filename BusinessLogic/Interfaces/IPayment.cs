using System;
using System.Collections.Generic;
using BusinessLogic.BindingModel;
using BusinessLogic.ViewModels;
using System.Text;

namespace BusinessLogic.Interfaces
{
    public interface IPayment
    {
        List<PaymentsViewModels> Read(PaymentsBindingModel model);
        void CreateOrUpdate(PaymentsBindingModel model);
    }
}
