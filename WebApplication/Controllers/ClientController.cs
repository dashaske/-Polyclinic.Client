using BusinessLogic.BindingModel;
using BusinessLogic.HelperModels;
using BusinessLogic.Interfaces;
using BusinessLogic.ViewModels;
using Database.Implements;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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
            ViewBag.Doctor = user.Read(null);
            var inspection = inspections.Read(null);
            Dictionary<int, decimal> cena = new Dictionary<int, decimal>();
            foreach (var i in inspection)
            {
                cena.Add((int)i.Id,
                    inspections.ReadCI(new CostInspectionsBindingModel { InspectionId = (int)i.Id }).ToList().Sum(x => x.Cena));
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
            ViewBag.Doctor = user.Read(null);
            var inspection = inspections.Read(null);
            Dictionary<int, decimal> cena = new Dictionary<int, decimal>();
            foreach (var i in inspection)
            {
                cena.Add((int)i.Id,
                    inspections.ReadCI(new CostInspectionsBindingModel { InspectionId = (int)i.Id }).ToList().Sum(x => x.Cena));
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
            var Visits = visit.Read(new VisitBindingModel { ClientId = (int)Program.User.Id }, new DateTime(), DateTime.Now);
            if (model.DateTo != new DateTime())
                Visits = visit.Read(new VisitBindingModel { ClientId = (int)Program.User.Id }, model.DateFrom, model.DateTo);
            Dictionary<int, decimal> Cena = new Dictionary<int, decimal>();
            Dictionary<int, decimal> Pay = new Dictionary<int, decimal>();
            foreach (var visits in Visits)
            {
                var vd = visit.ReadD(new InspectionsDoctorsBindingModel { VisitId = (int)visits.Id });
                foreach (var i in vd)
                {
                    Cena.Add((int)i.Id,
                    inspections.ReadCI(new CostInspectionsBindingModel { InspectionId = (int)i.InspectionId }).ToList().Sum(x => x.Cena));

                }
                Pay.Add((int)visits.Id,
                 payment.Read(new PaymentsBindingModel { VisitId = (int)visits.Id }).ToList().Sum(x => x.SumPaument));

            }
            ViewBag.Cena = Cena;
            ViewBag.Pay = Pay;
            ViewBag.Doctor = user.Read(null);
            ViewBag.Payments = payment.Read(null);
            ViewBag.InspectionsDoctors = visit.ReadD(null);
            ViewBag.Inspections = inspections.Read(null);
            ViewBag.Visits = Visits;
            return View();
        }
        public IActionResult Payment(int id)
        {
            if (TempData["ErrorLack"] != null)
            {
                ModelState.AddModelError("", TempData["ErrorLack"].ToString());
            }
            var Visit = visit.Read(new VisitBindingModel
            {
                Id = id
            }, new DateTime(), DateTime.Now).FirstOrDefault();
            ViewBag.Visit = Visit;
            ViewBag.LeftSum = Visit.Summ - payment.Read(new PaymentsBindingModel { VisitId = id }).Sum(x => x.SumPaument);
            return View();
        }
        public IActionResult SendReport(int id)
        {
            var Visit = visit.Read(new VisitBindingModel { ClientId = (int)Program.User.Id }, new DateTime(), DateTime.Now);
            var Title = $"Список визитов и платежей по ним клиента {Program.User.FIO}";
            var FileName = $"D:\\data\\ReportVisit{Program.User.FIO}.pdf";
            if (id != 0)
            {
                Visit = visit.Read(new VisitBindingModel { Id = id }, new DateTime(), DateTime.Now);
                Title = $"Список платежей по визиту {id} клиента {Program.User.FIO}";
                FileName = $"D:\\data\\ReportVisitId{id}Client{Program.User.FIO}.pdf";
            }
            ReportLogic.CreateDoc(new PdfInfo
            {
                FileName = FileName,
                Title = Title,
                Payment = payment.Read(null),
                Visit = Visit
            });
            return RedirectToAction("Visit");
        }
        [HttpPost]
        public ActionResult Payment(PaymentModel model)
        {
            var Visit = visit.Read(new VisitBindingModel
            {
                Id = model.VisitId
            }, new DateTime(), DateTime.Now).FirstOrDefault();
            int leftSum = (int)(Visit.Summ - payment.Read(new PaymentsBindingModel { VisitId = model.VisitId }).Sum(x => x.SumPaument));
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
            payment.CreateOrUpdate(new PaymentsBindingModel
            {
                VisitId = (int)Visit.Id,
                ClientId = (int)Program.User.Id,
                DatePayment = DateTime.Now,
                SumPaument = model.Sum
            });
            leftSum -= model.Sum;
            visit.CreateOrUpdate(new VisitBindingModel
            {
                Id = Visit.Id,
                ClientId = Visit.ClientId,
                Date = Visit.Date,
                Summ = Visit.Summ,
                Status = leftSum > 0 ? BusinessLogic.Enum.StatusVisit.ЧастичноОплачен : BusinessLogic.Enum.StatusVisit.Оплачен,
            }, new List<InspectionsDoctorsViewModels> { });
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
                ViewBag.Doctor = user.Read(null);
                var inspection = inspections.Read(null);
                Dictionary<int, decimal> cena = new Dictionary<int, decimal>();
                foreach (var i in inspection)
                {
                    cena.Add((int)i.Id,
                        inspections.ReadCI(new CostInspectionsBindingModel { InspectionId = (int)i.Id }).ToList().Sum(x => x.Cena));
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
            visit.CreateOrUpdate(new VisitBindingModel
            {
                ClientId = (int)Program.User.Id,
                Date = DateTime.Now,
                Status = t,
                Summ = sum,


            }, visitDoctors);
            return RedirectToAction("Visit");
        }

        private decimal CalculateSum(List<InspectionsDoctorsViewModels> visitDoctors)
        {
            decimal sum = 0;

            foreach (var doctor in visitDoctors)
            {
                var inspection = inspections.Read(new InspectionsBindingModel { Id = doctor.InspectionId }).FirstOrDefault();

                if (inspection != null)
                {

                    sum += inspections.ReadCI(new CostInspectionsBindingModel { InspectionId = (int)inspection.Id }).Sum(x => x.Cena) * doctor.Count;
                }
            }
            return sum;
        }
    }
}
