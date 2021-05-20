using System;
using System.Collections.Generic;
using BusinessLogic.BindingModel;
using BusinessLogic.ViewModels;
using System.Text;

namespace BusinessLogic.Interfaces
{
    public interface IInspections
    {
        List<InspectionsViewModels> GetFullList();
        InspectionsViewModels GetElement(InspectionsBindingModel model);
        CostInspectionsViewModels GetElement(CostInspectionsBindingModel model);
        List<InspectionsViewModels> GetFilteredList(InspectionsBindingModel model);
    }
}
