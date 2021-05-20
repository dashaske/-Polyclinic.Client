using BusinessLogic.BindingModel;
using BusinessLogic.HelperModels;
using BusinessLogic.Interfaces;
using BusinessLogic.ViewModels;
using Database.Implements;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class ClientController : Controller
    {
        private readonly IInspections inspections;
        private readonly IVisit visit;
        private readonly IUsers user;
        private readonly IPayment payment;
        public ClientController(IUsers user, IPayment payment, IInspections inspections, IVisit visit)
        {
            this.payment = payment;
            this.user = user;
            this.inspections = inspections;
            this.visit = visit;

        }
        public IActionResult Inspections()
        {
            ViewBag.Doctor = user.GetFullList();
            var inspection = inspections.GetFullList();
            Dictionary<int, decimal> cena = new Dictionary<int, decimal>();
            foreach (var i in inspection)
            {
                cena.Add((int)i.Id,
                   i.costInspections.Sum(x => x.Value));
            }
            ViewBag.Cena = cena;
            ViewBag.In = inspection;
            return View();
        }
        public IActionResult CreateVisit()
        {
            if (TempData["ErrorLack"] != null)
            {
                ModelState.AddModelError("", TempData["ErrorLack"].ToString());
            }
            ViewBag.Doctor = user.GetFullList();
            var inspection = inspections.GetFullList();
            Dictionary<int, decimal> cena = new Dictionary<int, decimal>();
            foreach (var i in inspection)
            {
                cena.Add((int)i.Id,
                    i.costInspections.Sum(x => x.Value));
            }
            ViewBag.Cena = cena;
            ViewBag.Inspections = inspection;
            return View();
        }
        public IActionResult Visit(ReportModel model)
        {
            if (model.DateFrom > model.DateTo)
            {
                ModelState.AddModelError("", "Даты должны быть разными");
            }
            var Visits = visit.GetFilteredList(new VisitBindingModel { ClientId = (int)Program.User.Id }, new DateTime(), DateTime.Now);
            if (model.DateTo != new DateTime())
                Visits = visit.GetFilteredList(new VisitBindingModel { ClientId = (int)Program.User.Id }, model.DateFrom, model.DateTo);
            Dictionary<int, decimal> Cena = new Dictionary<int, decimal>();
            Dictionary<int, decimal> Pay = new Dictionary<int, decimal>();
            foreach (var visits in Visits)
            {
                foreach (var ins in visits.visitInspections)
                {
                    var insp = inspections.GetElement(new InspectionsBindingModel { Id = ins.Key });
                    Cena.Add((int)ins.Key,
                  insp.costInspections.Sum(x => x.Value));
                }

                Pay.Add((int)visits.Id,
                  visits.visitPayments.Sum(x => x.Value));

            }
            ViewBag.Cena = Cena;
            ViewBag.Pay = Pay;
            ViewBag.Doctor = user.GetFullList();
            ViewBag.Payments = payment.GetFullList();
            ViewBag.InspectionsDoctors = visit.GetFullList();
            ViewBag.Inspections = inspections.GetFullList();
            ViewBag.Visits = Visits;
            return View();
        }
        public IActionResult Payment(int id)
        {
            if (TempData["ErrorLack"] != null)
            {
                ModelState.AddModelError("", TempData["ErrorLack"].ToString());
            }
            var Visit = visit.GetElement(new VisitBindingModel
            {
                Id = id
            });
            ViewBag.Visit = Visit;
            ViewBag.LeftSum = Visit.Summ - Visit.visitPayments.Sum(x => x.Value);
            return View();
        }
        public IActionResult SendReport(int id)
        {
            var Visit = visit.GetFilteredList(new VisitBindingModel { ClientId = (int)Program.User.Id }, new DateTime(), DateTime.Now);
            var Title = $"Список визитов и платежей по ним клиента {Program.User.FIO}";
            var FileName = $"D:\\data\\ReportVisit{Program.User.FIO}.";
            SaveToPdf.CreateDoc(new Info
            {
                FileName = FileName + "pdf",
                Title = Title,
                Payment = payment.GetFullList(),
                Visit = Visit
            });
            SaveToExel.CreateDoc(new Info
            {
                FileName = FileName + "xlsx",
                Title = Title,
                Payment = payment.GetFullList(),
                Visit = Visit
            });
            SaveToWord.CreateDoc(new Info
            {
                FileName = FileName + "docx",
                Title = Title,
                Payment = payment.GetFullList(),
                Visit = Visit
            });
            MailAddress from = new MailAddress("yudenichevaforlab@gmail.com", "Отчет!");
            MailAddress to = new MailAddress(Program.User.Email);
            MailMessage m = new MailMessage(from, to);
            m.Subject = FileName;
            m.Attachments.Add(new Attachment($"{FileName}pdf"));
            m.Attachments.Add(new Attachment($"{FileName}docx"));
            m.Attachments.Add(new Attachment($"{FileName}xlsx"));
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential("yudenichevaforlab@gmail.com", "passwd2001");
            smtp.EnableSsl = true;
            smtp.Send(m);

            return RedirectToAction("Visit");
        }
        [HttpPost]
        public ActionResult Payment(PaymentModel model)
        {
            var Visit = visit.GetElement(new VisitBindingModel
            {
                Id = model.VisitId
            });
            int leftSum = (int)(Visit.Summ - Visit.visitPayments.Sum(x => x.Value));
            if (!ModelState.IsValid)
            {
                ViewBag.Visit = Visit;
                ViewBag.LeftSum = leftSum;
                return View(model);
            }
            if (leftSum < model.Sum)
            {
                ModelState.AddModelError("", "Вы ввели слишком большую сумму");
                ViewBag.Visit = Visit;
                ViewBag.LeftSum = leftSum;
                return View(model);
            }
            payment.Insert(new PaymentsBindingModel
            {
                VisitId = (int)Visit.Id,
                ClientId = (int)Program.User.Id,
                DatePayment = DateTime.Now,
                SumPaument = model.Sum
            });
            leftSum -= model.Sum;
            visit.Update(new VisitBindingModel
            {
                Id = Visit.Id,
                ClientId = Visit.ClientId,
                Date = Visit.Date,
                Summ = Visit.Summ,
                Status = leftSum > 0 ? BusinessLogic.Enum.StatusVisit.ЧастичноОплачен : BusinessLogic.Enum.StatusVisit.Оплачен
            });
            return RedirectToAction("Visit");
        }
        [HttpPost]
        public ActionResult CreateVisit(CreateVisitModel model)
        {
            var visitDoctors = new List<InspectionsDoctorsViewModels>();

            foreach (var doctor in model.VisitDoctors)
            {
                if (doctor.Value > 0)
                {
                    visitDoctors.Add(new InspectionsDoctorsViewModels
                    {
                        InspectionId = doctor.Key,
                        Count = doctor.Value
                    });
                }
            }
            if (visitDoctors.Count == 0)
            {
                ViewBag.Doctor = user.GetFullList();
                var inspection = inspections.GetFullList();
                Dictionary<int, decimal> cena = new Dictionary<int, decimal>();
                foreach (var i in inspection)
                {
                    cena.Add((int)i.Id,
                        i.costInspections.Sum(x => x.Value));
                }
                ViewBag.Cena = cena;
                ViewBag.Inspections = inspection;
                ModelState.AddModelError("", "Ни один doctor не выбран");
                return View(model);
            }
            decimal sum = CalculateSum(visitDoctors);

            BusinessLogic.Enum.StatusVisit t = BusinessLogic.Enum.StatusVisit.Неоплачен;
            if (sum == 0)
            {
                t = BusinessLogic.Enum.StatusVisit.Оплачен;
            }
            Dictionary<int, int> visitInspections = new Dictionary<int, int> { };
            foreach (var i in visitDoctors)
            {
                visitInspections.Add(i.InspectionId, i.Count);
            }
            visit.Insert(new VisitBindingModel
            {
                ClientId = (int)Program.User.Id,
                Date = DateTime.Now,
                Status = t,
                Summ = sum,
                visitInspections = visitInspections
            });
            return RedirectToAction("Visit");
        }

        private decimal CalculateSum(List<InspectionsDoctorsViewModels> visitDoctors)
        {
            decimal sum = 0;

            foreach (var doctor in visitDoctors)
            {
                var inspection = inspections.GetElement(new InspectionsBindingModel { Id = doctor.InspectionId });

                if (inspection != null)
                {

                    sum += inspections.GetElement(new InspectionsBindingModel { Id = (int)inspection.Id }).costInspections.Sum(x => x.Value) * doctor.Count;
                }
            }
            return sum;
        }
    }
}
