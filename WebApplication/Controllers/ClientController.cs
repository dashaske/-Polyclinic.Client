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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication.Controllers
{
    public class ClientController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IInspections inspections;
        private readonly IVisit visit;
        private readonly IUsers user;
        private readonly IPayment payment;
        private readonly ReportLogic logic;
        public ClientController(IWebHostEnvironment environment, IUsers user, ReportLogic logic, IPayment payment, IInspections inspections, IVisit visit)
        {
            this.logic = logic;
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
                    Cena.Add(
                        (int)visit.GetElement(
                            new InspectionsDoctorsBindingModel
                            {
                                InspectionId = ins.Key,
                                VisitId = (int)visits.Id
                            }
                            ).Id
                       ,
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
        public IActionResult SendReport(ReportModel model)
        {
            if (model.DateFrom > model.DateTo)
            {
                ModelState.AddModelError("", "Даты должны быть разными");
                return View("Visit");
            }
            logic.SaveToPdfFile(new ReportBindingModel
            {
                FileName = @".\wwwroot\Report\ReportVisit.pdf",
                Title = $"Список визитов и платежей по ним клиента {Program.User.FIO}",
                User = Program.User,
                dateof = model.DateFrom,
                dateto = model.DateTo
            });
            return RedirectToAction("Visit");
        }

        public IActionResult SendReportExel(ReportModel model)
        {
            if (model.DateFrom > model.DateTo)
            {
                ModelState.AddModelError("", "Даты должны быть разными");
                return View("Visit");
            }
            logic.SaveDetailToExcelFile(new ReportBindingModel
            {
                FileName = @".\wwwroot\Report\ListVisit.xlsx",
                Title = $"Список визитов и платежей по ним клиента {Program.User.FIO}",
                User = Program.User,
                dateof = model.DateFrom,
                dateto = model.DateTo,
            });            
            return RedirectToAction("Visit");
        }
        public IActionResult SendReportWord(ReportModel model)
        {
            if (model.DateFrom > model.DateTo)
            {
                ModelState.AddModelError("", "Даты должны быть разными");
                return View("Visit");
            }
            logic.SaveDetailsToWordFile(new ReportBindingModel
            {
                FileName = @".\wwwroot\Report\ListVisit.docx",
                Title = $"Список визитов и платежей по ним клиента {Program.User.FIO}",
                User = Program.User,
                dateof = model.DateFrom,
                dateto = model.DateTo
            });
            return RedirectToAction("Visit");
        }
        public IActionResult Spisok()
        {
            ViewData["Id"] = new MultiSelectList(
                visit.GetFilteredList(
                    new VisitBindingModel { ClientId = (int)Program.User.Id }, new DateTime(), DateTime.Now), "Id", "Date");
            return View();
        }
        [HttpPost]
        public IActionResult MakeListDoc([Bind("Selected")] ReportBindingModel model)
        {
            model.FileName = @".\wwwroot\Report\ListVisit.docx";
            model.User = Program.User;
            model.dateof = new DateTime();
            model.dateto = DateTime.Now;
            model.Title = $"Список визитов и платежей по ним клиента {Program.User.FIO}";
            logic.SaveDetailsToWordFile(model);
            ViewData["Id"] = new MultiSelectList(
                visit.GetFilteredList(
                    new VisitBindingModel { ClientId = (int)Program.User.Id }, new DateTime(), DateTime.Now), "Id", "Date");

            return View("Spisok");
        }

        [HttpPost]
        public IActionResult MakeListXls([Bind("Selected")] ReportBindingModel model)
        {
            model.FileName = @".\wwwroot\Report\ListVisit.xlsx";
            model.User = Program.User;
            model.dateof = new DateTime();
            model.dateto = DateTime.Now;
            model.Title = $"Список визитов и платежей по ним клиента {Program.User.FIO}";
            logic.SaveDetailToExcelFile(model);
            ViewData["Id"] = new MultiSelectList(
                visit.GetFilteredList(
                    new VisitBindingModel { ClientId = (int)Program.User.Id }, new DateTime(), DateTime.Now), "Id", "Date");

            return View("Spisok");
        }
        public IActionResult Diagram()
        {

            return View();
        }
        [HttpGet]
        public JsonResult Metod()
        {
            List<DiagramViewModel> testDataFirst = new List<DiagramViewModel>();
            for (int i = 1; i < 13; i++)
            {
                var visits = visit.GetFilteredList(new VisitBindingModel { ClientId = (int)Program.User.Id },
                  new DateTime(DateTime.Now.Year, i, 1),
                  new DateTime(DateTime.Now.Year, i, 1).AddMonths(1).AddDays(-1));
                var count = 0;
                foreach (var v in visits)
                    count = count + v.visitInspections.Count();// считает не визиты а обследования . 
                //то есть сколько разных процедур ты посетил за месяц, если бы у тебя было 5 визитов в каждом было две процедуры пломбирование и электрофарез, 
                //то посчитается как 10 Вопросы? понятно все я пошла?
                // а можно сделать формирование отчёта пдф на форму? типо чтоб сначала можжно было сформировтаь а потом отправить только пдф?да
                testDataFirst.Add(new DiagramViewModel()
                {
                    cityName = new DateTime(DateTime.Now.Year, i, 1).Month.ToString(),
                    PopulationYear2020 = count
                });
            }
            return Json(testDataFirst);
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
