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
        public CostViewModels GetElement(CostBindingModel model)
        {
            if (model == null)
            {
                return null;
            }
            using (var context = new Database())
            {
                var component = context.Costs
                .FirstOrDefault(rec => rec.Name == model.Name ||
               rec.Id == model.Id);
                return component != null ?
                new CostViewModels
                {
                    Id = component.Id,
                    Name = component.Name
                } :
               null;
            }
        }
        public List<CostViewModels> GetFullList()
        {
            using (var context = new Database())
            {
                return context.Costs
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
