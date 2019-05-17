using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBV = Principle4.DryLogic.Validation;
using NUnit.Framework;
using System.ComponentModel;

namespace Principle4.DryLogic.Tests
{
    [TestFixture]
    public class EmployeeTests
    {
        Employee Employee;
        dynamic EmployeeProxy;

        [SetUp]
        public void SetUp()
        {
            Employee = new Employee();
            EmployeeProxy = new DryLogicProxy(Employee);
        }


        [Test]
        public void GetSetStringByProxy()
        {
            EmployeeProxy.MiddleName = "Roy";
            Assert.AreEqual("Roy", EmployeeProxy.MiddleName, "Field should be set.");
            Assert.AreEqual("Roy", Employee.MiddleName, "Field should be set.");
        }

		[Test]
		public void GetSetBooleanByProxy()
		{
			Employee.MakeValidForPresident();

			EmployeeProxy.IsPresident = "False";

			Assert.IsFalse(Employee.IsPresident, "Value should be false.");
		}


		[Test]
        public void GetSetValue()
        {
            Employee.HireDate = DateTime.Today;
            Assert.That(Employee.HireDate == DateTime.Today, "HireDate should be set.");
        }

        [Test]
        public void HireDateIsRequired()
        {
            var violatedRule = Employee.HireDateProperty.GetFirstRuleViolation(Employee.OI);
            Assert.That(violatedRule.AppliedRule is DBV.RequiredRule, "Employee should not be valid since hire date is required.");
        }

		[Test]
		public void SalaryIsOptionalNullable()
		{
			var violatedRule = Employee.SalaryProperty.GetFirstRuleViolation(Employee.OI);
			Assert.That(violatedRule == null, "Salary is not required, null allowed.");

			Assert.IsNull(Employee.Salary);
		}

		[Test]
        public void HireDateShouldBeAValidDate()
        {
            EmployeeProxy.HireDate = "asdf";
            var violatedRule = Employee.HireDateProperty.GetFirstRuleViolation(Employee.OI);
            Assert.That(violatedRule.AppliedRule is DBV.TypeConvertableRule, "Employee should not be valid since hire date cannot be converted to an datetime.");

            EmployeeProxy.HireDate = "13/13/2013";
            violatedRule = Employee.HireDateProperty.GetFirstRuleViolation(Employee.OI);
            Assert.That(violatedRule.AppliedRule is DBV.TypeConvertableRule, "Employee should not be valid since hire date cannot be converted to an datetime.");
        }

        [Test]
        public void NeedsAScore()
        {
            Employee.MakeValidForPresident();
            var violatedRule = Employee.ScoreProperty.GetFirstRuleViolation(Employee.OI);
            Assert.That(Employee.Score == null);

        }

		[Test]

        public void PropertyChanged()
        {
            Boolean oiChanged = false;
            Boolean proxyChanged = false;
            Employee.OI.PropertyChanged += (o, e) =>
              {
                  Assert.That(e.PropertyName == "HireDate", "HireDate should have changed");
                  oiChanged = true;
              };

            ((DryLogicProxy)EmployeeProxy).PropertyChanged += (o, e) =>
              {
                  Assert.That(e.PropertyName == "HireDate", "HireDate should have changed");
                  proxyChanged = true;
              };

            Employee.BirthDate = DateTime.Today;
            Assert.That(oiChanged && proxyChanged, "One or more events didn't fire.");
            oiChanged = false;
            proxyChanged = false;

            EmployeeProxy.HireDate = DateTime.Today.AddYears(1);
            Assert.That(oiChanged && proxyChanged, "One or more events didn't fire.");
        }

        [Test]
        public void TestConditionalRule_IsPresident()
        {
            Employee.MakeValid();
            Assert.That(Employee.OI.Validate(), "Employee should be valid.");
            Employee.BirthDate.AddDays(-1);

            var violatedRule = Employee.IsPresidentProperty.GetFirstRuleViolation(Employee.OI);
            //TODO: Need rule ID setter so we can identify the proper rule.
            Assert.That(violatedRule.AppliedRule.Id == "PRES40", "Employee should have violated the PRES40 rule.");



        }

        [Test]
        public void IDataErrorInfoTests()
        {
            IDataErrorInfo employeeIDEI = Employee.OI as IDataErrorInfo;

            Assert.That(employeeIDEI.Error != "", "Employee should be invalid.");
            Assert.That(employeeIDEI["HireDate"] != "", "HireDate should be invalid.");

            Employee.MakeValid();

            Assert.That(employeeIDEI.Error == "", employeeIDEI.Error);
            Assert.That(employeeIDEI["HireDate"] == "", "HireDate should be valid.");

        }
    }
}
