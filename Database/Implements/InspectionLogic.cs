using System;
using System.Collections.Generic;
using BusinessLogic.BindingModel;
using BusinessLogic.Interfaces;
using BusinessLogic.ViewModels;
using Database.Models;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Database.Implements
{
    public class InspectionLogic : IInspections
    {
        public List<InspectionsViewModels> GetFullList()
        {
            using (var context = new Database())
            {
                return context.Inspections
                .Include(rec => rec.CostInspection)
               .ThenInclude(rec => rec.Cost)
               .ToList()
               .Select(rec => new InspectionsViewModels
               {
                   Id = rec.Id,
                   Name = rec.Name,
                   UserId = rec.UserId,
                   costInspections = rec.CostInspection
                .ToDictionary(recPC => (int)recPC.CostId, recPC =>
               (recPC.Cena))
               })
               .ToList();
            }
        }
        public List<InspectionsViewModels> GetFilteredList(InspectionsBindingModel model)
        {
            if (model == null)
            {
                return null;
            }
            using (var context = new Database())
            {
                return context.Inspections
                .Include(costi => costi.CostInspection)
                .ThenInclude(cost => cost.Cost)
               .Where(ins => ins.UserId == model.UserId
               )
               .ToList()
               .Select(rec => new InspectionsViewModels
               {
                   Id = rec.Id,
                   Name = rec.Name,
                   UserId = rec.UserId,
                   costInspections = rec.CostInspection
                 .ToDictionary(recPC => (int)recPC.CostId, recPC =>
               (recPC.Cena))
               })
               .ToList();
            }
        }
        public InspectionsViewModels GetElement(InspectionsBindingModel model)
        {
            if (model == null)
            {
                return null;
            }
            using (var context = new Database())
            {
                var product = context.Inspections
                .Include(rec => rec.CostInspection)
               .ThenInclude(rec => rec.Cost)
               .FirstOrDefault(rec => rec.Id == model.Id ||
                (rec.Name.Contains(model.Name) && rec.UserId == model.UserId)
               );
                return product != null ?
                new InspectionsViewModels
                {
                    Id = product.Id,
                    Name = product.Name,
                    UserId = product.UserId,
                    costInspections = product.CostInspection
                 .ToDictionary(recPC => (int)recPC.CostId, recPC =>
               (recPC.Cena))
                } :
               null;
            }
        }
        public CostInspectionsViewModels GetElement(CostInspectionsBindingModel model)
        {
            if (model == null)
            {
                return null;
            }
            using (var context = new Database())
            {
                var product = context.CostInspections
               .FirstOrDefault(rec => rec.Id == model.Id || (rec.CostId == model.CostId && rec.InspectionId == model.InspectionId));
                return product != null ?
                new CostInspectionsViewModels
                {
                    Id = product.Id,
                    Cena = product.Cena,
                    CostId = product.CostId,
                    InspectionId = product.InspectionId
                } :
               null;
            }
        }
    }
}
