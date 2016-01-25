using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Principle4.DryLogic.Tests;

namespace Principle4.DryLogic.Demos.Web.Models
{
  public class EmployeeViewModel
  {
    public Employee MyEmployee { get; set; }
    public EmployeeViewModel()
    {
      MyEmployee = new Employee();
    }
  }

}