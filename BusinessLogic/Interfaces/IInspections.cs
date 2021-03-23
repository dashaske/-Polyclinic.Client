using System;
using System.Collections.Generic;
using BusinessLogic.BindingModel;
using BusinessLogic.ViewModels;
using System.Text;

namespace BusinessLogic.Interfaces
{
    public interface IInspections
    {
        List<InspectionsViewModels> Read(InspectionsBindingModel model);
        List<CostInspectionsViewModels> ReadCI(CostInspectionsBindingModel model);
    }
}
