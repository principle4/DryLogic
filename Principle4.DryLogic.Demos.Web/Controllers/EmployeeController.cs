using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Principle4.DryLogic.Demos.Web.Models;
using Principle4.DryLogic.Tests;

namespace Principle4.DryLogic.Demos.Web.Controllers
{
  public class EmployeeController : Controller
  {

    [HttpPost]
    public ActionResult Test(String id)
    {
      throw new NotImplementedException();
    }

    //
    // GET: /Employee/
    public ActionResult Index()
    {
      return View();
    }

    //
    // GET: /Employee/Details/5
    public ActionResult Details(int id)
    {
      return View();
    }

    //
    // GET: /Employee/Create
    public ActionResult Create()
    {
      //return View(new Employee() { HireDate=DateTime.Now, BirthDate=DateTime.Today.AddYears(-40) });

      var vm = new EmployeeViewModel();
      vm.MyEmployee.HireDate = DateTime.Today;
      return View(vm);
      //return View();
    }
    public ActionResult Create2()
    {
      //return View(new Employee() { HireDate=DateTime.Now, BirthDate=DateTime.Today.AddYears(-40) });

      var vm = new EmployeeViewModel();
      vm.MyEmployee.HireDate = DateTime.Today;
      return View(vm);
      //return View();
    }

    [HttpPost]
    public ActionResult Create2(EmployeeViewModel vm)
    {
      //return View(new Employee() { HireDate=DateTime.Now, BirthDate=DateTime.Today.AddYears(-40) });

      return View(vm);
      //return View();
    }



    public ActionResult Create_SimpleType()
    {
      return View(new Employee() { HireDate = DateTime.Now });
    }
    public ActionResult Create_HtmlHelper_SimpleType()
    {
      return View(new Employee() { HireDate = DateTime.Now });
    }

    [HttpPost]
    public ActionResult Create_SimpleType(Employee employee)
    {
      return View(employee);
    }


    //
    // POST: /Employee/Create
    [HttpPost]
    public ActionResult Create(EmployeeViewModel employeeVM)
    {
      return View(employeeVM);
    }
  }
}
