using System;
using System.Collections.Generic;
using BusinessLogic.BindingModel;
using BusinessLogic.Interfaces;
using BusinessLogic.ViewModels;
using System.Linq;
using System.Text;

namespace Database.Implements
{
    public class CostLogic : ICost
    {
        public List<CostViewModels> Read(CostBindingModel model)
        {
            using (var context = new Database())
            {
                return context.Costs
                 .Where(rec => model == null
                   || rec.Id == model.Id
                   || rec.Name == model.Name)
               .Select(rec => new CostViewModels
               {
                   Id = rec.Id,
                   Name = rec.Name
               })
                .ToList();
            }
        }
    }
}
