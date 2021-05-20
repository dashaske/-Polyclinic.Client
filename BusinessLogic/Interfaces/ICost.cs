using System;
using System.Collections.Generic;
using BusinessLogic.BindingModel;
using BusinessLogic.ViewModels;
using System.Text;

namespace BusinessLogic.Interfaces
{
    public interface ICost
    {
        List<CostViewModels> GetFullList();
        CostViewModels GetElement(CostBindingModel model);
    }
}
