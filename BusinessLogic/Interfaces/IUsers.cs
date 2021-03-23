﻿using System;
using System.Collections.Generic;
using BusinessLogic.BindingModel;
using BusinessLogic.ViewModels;
using System.Text;

namespace BusinessLogic.Interfaces
{
    public interface IUsers
    {
        List<UsersViewModels> Read(UsersBindingModel model);
        void CreateOrUpdate(UsersBindingModel model);
    }
}
