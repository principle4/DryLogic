using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Principle4.DryLogic.Tests
{
  public class TestBase
  {
    public Employee Employee{get; set;}
    [SetUp]
    public void Setup()
    {
      Employee = new Employee();
    }
  }
}
