using System;
using System.Collections.Generic;
using BusinessLogic.BindingModel;
using BusinessLogic.Interfaces;
using BusinessLogic.ViewModels;
using Database.Models;
using System.Linq;
using System.Text;

namespace Database.Implements
{
    public class InspectionLogic : IInspections
    {
        public List<InspectionsViewModels> Read(InspectionsBindingModel model)
        {
            using (var context = new Database())
            {
                return context.Inspections
                 .Where(rec => model == null
                   || rec.Id == model.Id
                   || ((rec.Name == model.Name || model.Name == null) && rec.UserId == model.UserId))
               .Select(rec => new InspectionsViewModels
               {
                   Id = rec.Id,
                   Name = rec.Name,
                   UserId = rec.UserId
               })
                .ToList();
            }
        }

        public List<CostInspectionsViewModels> ReadCI(CostInspectionsBindingModel model)
        {
            using (var context = new Database())
            {
                return context.CostInspections
                 .Where(rec => model == null
                   || rec.Id == model.Id
                   || rec.InspectionId == model.InspectionId)
               .Select(rec => new CostInspectionsViewModels
               {
                   Id = rec.Id,
                   Cena = rec.Cena,
                   InspectionId = rec.InspectionId,
                   CostId = rec.CostId
               })
                .ToList();
            }
        }
    }
}
