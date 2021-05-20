using System;
using System.Collections.Generic;
using BusinessLogic.BindingModel;
using BusinessLogic.ViewModels;
using System.Text;

namespace BusinessLogic.Interfaces
{
    public interface IVisit
    {
        void DeleteInspection(InspectionsDoctorsBindingModel model);
        List<VisitViewModels> GetFullList();
        VisitViewModels GetElement(VisitBindingModel model);
        InspectionsDoctorsViewModels GetElement(InspectionsDoctorsBindingModel model);
        void Insert(VisitBindingModel model);
        void Update(VisitBindingModel model);
        void Delete(VisitBindingModel model);
        List<VisitViewModels> GetFilteredList(VisitBindingModel model, DateTime DateFrom, DateTime DateTo);
    }
}
