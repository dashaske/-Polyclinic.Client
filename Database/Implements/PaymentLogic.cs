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
        public void CreateOrUpdate(PaymentsBindingModel model)
        {
            using (var context = new Database())
            {
                Payment element = model.Id.HasValue ? null : new Payment();
                if (model.Id.HasValue)
                {
                    element = context.Payments.FirstOrDefault(rec => rec.Id == model.Id);
                    if (element == null)
                    {
                        throw new Exception("Элемент не найден");
                    }
                }
                else
                {
                    element = new Payment();
                    context.Payments.Add(element);
                }
                element.VisitId = model.VisitId;
                element.DatePayment = model.DatePayment;
                element.SumPaument = model.SumPaument;
                element.ClientId = model.ClientId;
                context.SaveChanges();
            }
        }

        public List<PaymentsViewModels> Read(PaymentsBindingModel model)
        {
            using (var context = new Database())
            {
                return context.Payments
                 .Where(rec => model == null
                   || rec.Id == model.Id
                   || rec.VisitId == model.VisitId)
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
