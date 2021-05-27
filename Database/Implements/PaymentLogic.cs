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
    public class PaymentLogic : IPayment
    {
        public List<PaymentsViewModels> GetFilteredList(PaymentsBindingModel model)
        {
            if (model == null)
            {
                return null;
            }
            using (var context = new Database())
            {
                return context.Payments
               .Where(rec => rec.VisitId == model.VisitId
               )
               .ToList()
               .Select(rec => new PaymentsViewModels
               {
                   Id = rec.Id,
                   VisitId = rec.VisitId,
                   ClientId = rec.ClientId,
                   DatePayment = rec.DatePayment,
                   SumPaument = rec.SumPaument
               })
               .ToList();
            }
        }
        private Payment CreateModel(PaymentsBindingModel model, Payment payment, Database context)
        {
            payment.VisitId = model.VisitId;
            payment.ClientId = model.ClientId;
            payment.DatePayment = model.DatePayment;
            payment.SumPaument = model.SumPaument;
            return payment;
        }
        public void Update(PaymentsBindingModel model)
        {
            using (var context = new Database())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var payments = context.Payments.FirstOrDefault(rec => rec.Id == model.Id);
                        if (payments == null)
                        {
                            throw new Exception("Заказ не найдена");
                        }
                        CreateModel(model, payments, context);
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
        public void Insert(PaymentsBindingModel model)
        {
            using (var context = new Database())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        Payment cost = new Payment();
                        context.Payments.Add(cost);
                        CreateModel(model, cost, context);
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
        public PaymentsViewModels GetElement(PaymentsBindingModel model)
        {
            if (model == null)
            {
                return null;
            }
            using (var context = new Database())
            {
                var component = context.Payments
                .FirstOrDefault(rec => rec.Id == model.Id);
                return component != null ?
                new PaymentsViewModels
                {
                    Id = component.Id,
                    VisitId = component.VisitId,
                    ClientId = component.ClientId,
                    DatePayment = component.DatePayment,
                    SumPaument = component.SumPaument
                } :
               null;
            }
        }
        public List<PaymentsViewModels> GetFullList()
        {
            using (var context = new Database())
            {
                return context.Payments
                .Select(rec => new PaymentsViewModels
                {
                    Id = rec.Id,
                    VisitId = rec.VisitId,
                    ClientId = rec.ClientId,
                    DatePayment = rec.DatePayment,
                    SumPaument = rec.SumPaument
                })
               .ToList();
            }
        }
    }
}
