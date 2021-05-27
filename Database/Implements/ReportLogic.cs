using BusinessLogic.BindingModel;
using BusinessLogic.HelperModels;
using BusinessLogic.Interfaces;
using BusinessLogic.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Database.Implements
{
    public class ReportLogic
    {
        private readonly IVisit visit;
        private readonly IInspections ins;
        private readonly IPayment payment;
        public ReportLogic(IVisit visit, IInspections ins, IPayment payment)
        {
            this.ins = ins;
            this.visit = visit;
            this.payment = payment;
        }
        public List<ReportDetailViewModel> GetDetail(int Id, ReportBindingModel model)
        {
            var engines = visit.GetFilteredList(new VisitBindingModel
            { ClientId = Id, Selected = model.Selected }, model.dateof, model.dateto);
            var list = new List<ReportDetailViewModel>();
            foreach (var engine in engines)
            {

                var record = new ReportDetailViewModel
                {
                    Id = (int)engine.Id,
                    Status = engine.Status,
                    Summ = engine.Summ,
                    Date = engine.Date,
                    DetailsPay = new List<Tuple<DateTime, decimal>>(),

                };
                foreach (var detail in engine.visitPayments)
                {
                    record.DetailsPay.Add(new Tuple<DateTime, decimal>(payment.GetElement(new PaymentsBindingModel { Id = detail.Key }).DatePayment, detail.Value));
                }
                list.Add(record);
            }
            return list;
        }
        public void SaveDetailsToWordFile(ReportBindingModel model)
        {
            SaveToWord.CreateDoc(new Info
            {
                FileName = model.FileName,
                Title = model.Title,

                Visit = GetDetail(
                    (int)model.User.Id,
                model)
            });
        }
        public void SaveDetailToExcelFile(ReportBindingModel model)
        {
            SaveToExel.CreateDoc(new Info
            {
                FileName = model.FileName,
                Title = model.Title,
                Visit = GetDetail((int)model.User.Id, model)
            });
        }

        public void SaveToPdfFile(ReportBindingModel model)
        {
            SaveToPdf.CreateDoc(new Info
            {
                FileName = model.FileName,
                Title = model.Title,
                Visit = GetDetail((int)model.User.Id, model)
            });
            Mail(model);
        }

        public void Mail(ReportBindingModel model)
        {
            MailAddress from = new MailAddress("yudenichevaforlab@gmail.com", "Отчет!");
            MailAddress to = new MailAddress(model.User.Email);
            MailMessage m = new MailMessage(from, to);
            m.Subject = model.Title;
            m.Attachments.Add(new Attachment($"{model.FileName}"));
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential("yudenichevaforlab@gmail.com", "passwd2001");
            smtp.EnableSsl = true;
            smtp.Send(m);
        }
    }
}

