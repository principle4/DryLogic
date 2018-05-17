using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Principle4.DryLogic.Tests;

namespace Principle4.DryLogic.Demos.Web.Models
{
  public class EmployeeViewModel
  {
    public Employee MyEmployee { get; set; }

        public List<SelectListItem> ScoreOptions = new List<SelectListItem> {
          new SelectListItem() { Value = "0" , Text = "Red"  },
          new SelectListItem() { Value = "1" , Text = "Blue"  },
          new SelectListItem() { Value = "2" , Text = "Green"  }
    };

    public EmployeeViewModel()
    {
      MyEmployee = new Employee();
    }
  }

}