using Database.Models;
using System;
using System.Collections.Generic;
using System.Text;
using BusinessLogic.Interfaces;
using BusinessLogic.BindingModel;
using BusinessLogic.ViewModels;
using System.Linq;

namespace Database.Implements
{
    public class VisitLogic : IVisit
    {
        public void CreateOrUpdate(VisitBindingModel model, List<InspectionsDoctorsViewModels> visitDoctors)
        {
            using (var context = new Database())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        Visit element = model.Id.HasValue ? null : new Visit();
                        if (model.Id.HasValue)
                        {
                            element = context.Visis.FirstOrDefault(rec => rec.Id ==
                           model.Id);
                            if (element == null)
                            {
                                throw new Exception("Элемент не найден");
                            }
                            element.ClientId = model.ClientId;
                            element.Date = model.Date;
                            element.Status = model.Status;
                            element.Summ = model.Summ;

                            context.SaveChanges();
                        }
                        else
                        {
                            element.ClientId = model.ClientId;
                            element.Date = model.Date;
                            element.Status = model.Status;
                            element.Summ = model.Summ;
                            context.Visis.Add(element);
                            context.SaveChanges();
                            var groupDoctors = visitDoctors
                               .GroupBy(rec => rec.InspectionId)
                               .Select(rec => new
                               {
                                   InspectionId = rec.Key,
                                   Count = rec.Sum(r => r.Count)
                               });

                            foreach (var groupDoctor in groupDoctors)
                            {
                                context.InspectionDoctors.Add(new InspectionDoctor
                                {
                                    VisitId = (int)element.Id,
                                    InspectionId = groupDoctor.InspectionId,
                                    Count = groupDoctor.Count
                                });
                                context.SaveChanges();
                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<VisitViewModels> Read(VisitBindingModel model, DateTime DateFrom, DateTime DateTo)
        {
            using (var context = new Database())
            {
                return context.Visis
                 .Where(rec => model == null
                   || rec.Id == model.Id
                   || (rec.Date <= DateTo && rec.Date >= DateFrom && rec.ClientId == model.ClientId))
               .Select(rec => new VisitViewModels
               {
                   Id = rec.Id,
                   ClientId = rec.ClientId,
                   Date = rec.Date,
                   Status = rec.Status,
                   Summ = rec.Summ
               })
                .ToList();
            }
        }

        public List<InspectionsDoctorsViewModels> ReadD(InspectionsDoctorsBindingModel model)
        {
            using (var context = new Database())
            {
                return context.InspectionDoctors
                 .Where(rec => model == null
                   || rec.Id == model.Id
                   || rec.InspectionId == model.InspectionId || rec.VisitId == model.VisitId)
               .Select(rec => new InspectionsDoctorsViewModels
               {
                   Id = rec.Id,
                   Count = rec.Count,
                   InspectionId = rec.InspectionId,
                   VisitId = rec.VisitId
               })
                .ToList();
            }
        }
    }
}
