using Database.Models;
using System;
using System.Collections.Generic;
using System.Text;
using BusinessLogic.Interfaces;
using BusinessLogic.BindingModel;
using BusinessLogic.ViewModels;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Database.Implements
{
    public class VisitLogic : IVisit
    {
        public void Insert(VisitBindingModel model)
        {
            using (var context = new Database())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        Visit ins = new Visit();
                        context.Visis.Add(ins);
                        CreateModel(model, ins, context);
                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public void Update(VisitBindingModel model)
        {
            using (var context = new Database())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var element = context.Visis.FirstOrDefault(rec => rec.Id ==
                       model.Id);
                        if (element == null)
                        {
                            throw new Exception("Элемент не найден");
                        }
                        CreateModel(model, element, context);
                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        private Visit CreateModel(VisitBindingModel model, Visit product,
       Database context)
        {
            product.ClientId = model.ClientId;
            product.Date = model.Date;
            product.Status = model.Status;
            product.Summ = model.Summ;
            if (model.Id.HasValue && model.visitInspections != null)
            {
                var productComponents = context.InspectionDoctors.Where(rec =>
               rec.VisitId == model.Id.Value).ToList();
                // удалили те, которых нет в модели
                context.InspectionDoctors.RemoveRange(productComponents.Where(rec =>
               !model.visitInspections.ContainsKey(rec.InspectionId)).ToList());
                context.SaveChanges();
                // обновили количество у существующих записей
                foreach (var updateComponent in productComponents)
                {
                    updateComponent.Count =
                   model.visitInspections[updateComponent.InspectionId] + updateComponent.Count;
                    model.visitInspections.Remove(updateComponent.InspectionId);
                }

            }
            context.SaveChanges();
            // добавили новые
            if (model.visitInspections != null)
            {
                foreach (var pc in model.visitInspections)
                {
                    if (pc.Value > 0)
                    {
                        context.InspectionDoctors.Add(new InspectionDoctor
                        {
                            Count = pc.Value,
                            VisitId = (int)product.Id,
                            InspectionId = pc.Key
                        });
                        context.SaveChanges();
                    }
                }
            }

            return product;
        }
        public List<VisitViewModels> GetFullList()
        {
            using (var context = new Database())
            {
                return context.Visis
                .Include(rec => rec.InspectionDoctor)
               .ThenInclude(rec => rec.Inspection)
                .Include(rec => rec.Payment)
               .ThenInclude(rec => rec.User)
               .ToList()
               .Select(rec => new VisitViewModels
               {
                   Id = rec.Id,
                   ClientId = rec.ClientId,
                   Date = rec.Date,
                   Status = rec.Status,
                   Summ = rec.Summ,
                   visitInspections = rec.InspectionDoctor
                .ToDictionary(recPC => (int)recPC.InspectionId, recPC => (recPC.Count)),
                   visitPayments = rec.Payment
                   .ToDictionary(recPC => (int)recPC.Id, (recPC => (recPC.SumPaument)))
               })
               .ToList();
            }
        }
        public List<VisitViewModels> GetFilteredList(VisitBindingModel model, DateTime DateFrom, DateTime DateTo)
        {
            if (model == null)
            {
                return null;
            }
            using (var context = new Database())
            {
                var t = context.Visis
                 .Include(rec => rec.InspectionDoctor)
                .ThenInclude(rec => rec.Inspection)
                 .Include(rec => rec.Payment)
                .ThenInclude(rec => rec.User)
               .Where(rec =>
                   (rec.Date <= DateTo && rec.Date >= DateFrom && rec.ClientId == model.ClientId)
                   &&
                     (((model.Selected == null) || model.Selected.Contains((int)rec.Id)))
               )
               .ToList()
               .Select(rec => new VisitViewModels
               {
                   Id = rec.Id,
                   ClientId = rec.ClientId,
                   Date = rec.Date,
                   Status = rec.Status,
                   Summ = rec.Summ,
                   visitInspections = rec.InspectionDoctor
                .ToDictionary(recPC => (int)recPC.InspectionId, recPC => (recPC.Count)),
                   visitPayments = rec.Payment
                   .ToDictionary(recPC => (int)recPC.Id, recPC => (recPC.SumPaument))
               })
               .ToList();
                return t;
            }
        }
        public VisitViewModels GetElement(VisitBindingModel model)
        {
            if (model == null)
            {
                return null;
            }
            using (var context = new Database())
            {
                var product = context.Visis
                .Include(rec => rec.InspectionDoctor)
               .ThenInclude(rec => rec.Inspection)
                .Include(rec => rec.Payment)
               .ThenInclude(rec => rec.User)
               .FirstOrDefault(rec => rec.Id == model.Id);
                return product != null ?
                new VisitViewModels
                {
                    Id = product.Id,
                    ClientId = product.ClientId,
                    Date = product.Date,
                    Status = product.Status,
                    Summ = product.Summ,
                    visitInspections = product.InspectionDoctor
                .ToDictionary(recPC => (int)recPC.InspectionId, recPC => (recPC.Count)),
                    visitPayments = product.Payment
                   .ToDictionary(recPC => (int)recPC.Id, recPC => (recPC.SumPaument))
                } :
               null;
            }
        }
        public InspectionsDoctorsViewModels GetElement(InspectionsDoctorsBindingModel model)
        {
            if (model == null)
            {
                return null;
            }
            using (var context = new Database())
            {
                var product = context.InspectionDoctors
               .FirstOrDefault(rec => rec.Id == model.Id
                   || rec.InspectionId == model.InspectionId && rec.VisitId == model.VisitId);
                return product != null ?
                new InspectionsDoctorsViewModels
                {
                    Id = product.Id,
                    Count = product.Count,
                    InspectionId = product.InspectionId,
                    VisitId = product.VisitId
                } :
               null;
            }
        }

        public void DeleteInspection(InspectionsDoctorsBindingModel model)
        {
            using (var context = new Database())
            {
                InspectionDoctor element = context.InspectionDoctors.FirstOrDefault(rec => rec.Id == model.Id);

                if (element != null)
                {
                    context.InspectionDoctors.Remove(element);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Элемент не найден");
                }
            }
        }

        public void Delete(VisitBindingModel model)
        {
            using (var context = new Database())
            {
                Visit element = context.Visis.FirstOrDefault(rec => rec.Id ==
               model.Id);
                if (element != null)
                {
                    context.Visis.Remove(element);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Элемент не найден");
                }
            }
        }
    }
}
