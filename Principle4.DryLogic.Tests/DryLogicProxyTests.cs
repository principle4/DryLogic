//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using NUnit.Framework;

//namespace Principle4.DryLogic.Tests
//{
//  [TestFixture]
//  public class BOVProxyTests
//  {
//    [Test]
//    public void Get()
//    {
//      Employee employee = EmployeeTests.GetEmployee();
//      dynamic proxy = new BOVProxy(employee);

//      String ageString = proxy.Age;
//      Assert.AreEqual("10", ageString);

//      String age = proxy.Age;
//      Assert.AreEqual("10", age);
//      Assert.AreEqual(10, employee.Age);



//    }
//    [Test]
//    public void Set()
//    {
//      Employee employee = EmployeeTests.GetEmployee();
//      dynamic proxy = new BOVProxy(employee);

//      proxy.Age = "Invalid";
//      try
//      {
//        var ageTest = employee.Age;
//        Assert.Fail("Should have failed as age is currently invalid.");
//      }
//      catch (BOVException)
//      {

//      }

//      proxy.Age = "11";

//      String ageString = proxy.Age;
//      Assert.AreEqual("11", ageString);

//      String age = proxy.Age;
//      Assert.AreEqual("11", age);
//      Assert.AreEqual(11, employee.Age);



//    }

//  }
//}
