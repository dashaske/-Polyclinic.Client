using System;
using System.Collections.Generic;
using BusinessLogic.BindingModel;
using BusinessLogic.ViewModels;
using System.Text;

namespace BusinessLogic.Interfaces
{
    public interface IVisit
    {
        List<VisitViewModels> Read(VisitBindingModel model, DateTime DateTo, DateTime DateFrom);
        void CreateOrUpdate(VisitBindingModel model, List<InspectionsDoctorsViewModels> visitDoctors);
        List<InspectionsDoctorsViewModels> ReadD(InspectionsDoctorsBindingModel model);
    }
}
