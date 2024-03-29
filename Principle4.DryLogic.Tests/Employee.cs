﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Principle4.DryLogic;
using Principle4.DryLogic.Validation;
using System.ComponentModel;

namespace Principle4.DryLogic.Tests
{
	[DryLogicObject]
	public class Employee : IDataErrorInfo
	{
		static ObjectDefinition<Employee> OD = new ObjectDefinition<Employee>();
		public ObjectInstance OI { get; private set; }

		//public static Expression<Func<Employee, PropertyValue>> GetValueAccessorExpression(string propertyName)
		//{
		//  return employee => employee.OI.PropertyValues[propertyName];
		//}

		static Employee()
		{

			//Assert.That(OD) //create the rule builder
			//  .AdhearsTo(oi => AgeProperty.GetValue(oi) > 40) //create and assign the rule inside of the rule builder
			//  .When(oi => IsPresidentProperty.GetValue(oi))
			//  .Notifying(AgeProperty,IsPresidentProperty);//Will require a separate set of rules - must be evaluated after the individual rules

		}

		public Employee()
		{
			OI = OD.CreateInstance(this);
			IsPresident = false;

			OI.PropertyChanged += OI_PropertyChanged;
		}
		private void OI_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(this.HireDate))
			{
				//do your secondary drop down update here
			}
		}



		#region HireDate
		public static readonly PropertyDefinition<DateTime> HireDateProperty
	  = OD.AddProperty(pi => pi.HireDate, p =>
	  {
		  Assert.That(p)
			.IsRequired()
			.IsConvertable()
			;
		  p.Formatter = v => v.ToString("MM/dd/yyyy");
	  });

		public DateTime HireDate
		{
			get { return OI.GetValue(HireDateProperty); }
			set { OI.SetValue(HireDateProperty, value); }
		}
		#endregion



		#region BirthDate
		public static readonly PropertyDefinition<DateTime> BirthDateProperty
		  //  = OD.Properties.Add(BirthDateProperty,"BirthDate", "First Name", p =>
		  = OD.AddProperty(pi => pi.BirthDate, p =>
		  {
			  p.Formatter = v => v.ToString("MM/dd/yyyy");
			  Assert.That(p)
			.IsRequired()
			.IsConvertable()
			.IsAdhearingTo(oi => p[oi] < DateTime.Today.AddYears(-18))
			  .IdentifiedBy(">18") //this is where an 'and' operator would be nice.  Without it, we rely on the 'Is' convention to identify the start of a new rule
			  .WithMessage("Employee must be at least 18 years old.")
		  ;
		  });

		public DateTime BirthDate
		{
			get { return OI.GetValue(BirthDateProperty); }
			set { OI.SetValue(BirthDateProperty, value); }
		}
		#endregion


		#region TerminationDate
		public static readonly PropertyDefinition<DateTime?> TerminationDateProperty
		  = OD.AddProperty(pi => pi.TerminationDate, p =>
		  {
			  Assert.That(p)
			.IsConvertable()
			;
		  });

		public DateTime? TerminationDate
		{
			get { return OI.GetValue(TerminationDateProperty); }
			set { OI.SetValue(TerminationDateProperty, value); }
		}
		#endregion

		#region LastName
		public static readonly PropertyDefinition<String> LastNameProperty
		  //  = OD.Properties.Add(LastNameProperty,"LastName", "First Name", p =>
		  = OD.AddProperty(pi => pi.LastName, p =>
		  {
			  Assert.That(p)
			.IsRequired()
			;
		  });

		public String LastName
		{
			get { return OI.GetValue(LastNameProperty); }
			set { OI.SetValue(LastNameProperty, value); }
		}
		#endregion

		#region FirstName
		public static readonly PropertyDefinition<String> FirstNameProperty
		  //  = OD.Properties.Add(FirstNameProperty,"FirstName", "First Name", p =>
		  = OD.AddProperty(pi => pi.FirstName, p =>
		  {
			  Assert.That(p)
			.IsRequired()
			  .WithMessage("FirstName is required, yo.")
			;
		  });

		public String FirstName
		{
			get { return OI.GetValue(FirstNameProperty); }
			set { OI.SetValue(FirstNameProperty, value); }
		}
		#endregion

		#region MiddleName
		internal static readonly PropertyDefinition<String> MiddleNameProperty
		  = OD.Properties.Add<String>("MiddleName", "Middle Name", p =>
		  {
		  //no rules here for simple get/set tests
	  });
		public String MiddleName
		{
			get { return OI.GetValue(MiddleNameProperty); }
			set { OI.SetValue(MiddleNameProperty, value); }
		}
		#endregion


		#region IsPresident
		internal static readonly PropertyDefinition<Boolean> IsPresidentProperty
		= OD.Properties.Add<Boolean>("IsPresident", "Is President?", p =>
		{
			Assert.That(p)
		  .IsRequired()
		  .IsConvertable()
		  .IsAdhearingTo(oi => oi.GetValue(IsPresidentProperty) == false)
			.When(oi => BirthDateProperty[oi].AddYears(40) > DateTime.Today)
			.WithMessage("{0} cannot be true if the employee is not yet 40")
			.IdentifiedBy("PRES40");
			//New idea:  I've started TypedPropertyRuleBuilder<T>
			//OD.Assert.That(p) //OD knows the object type, so we could conceivably have a typed assert function rather than passing around oi
			//	.IsAdhearingTo( emp => emp.BirthDate.AddYears(40) >= DateTime.Today)
			//		.DependingOn(emp => emp.BirthDate) //this would indicate that BirthDate needs to be valid before we can continue with this rule.  
			//										//That might be extractible from IsAdhearingTo, but that might be a lot of expression parsing.
			//										//And maybe this isn't really necessary...it might be a better idea to "remember" that rule parsing failed
			//										//and then try the other properties first, throwing an error for the original problem only if a "better" violation could not be found

		});
		public Boolean IsPresident
		{
			get { return OI.GetValue(IsPresidentProperty); }
			set { OI.SetValue(IsPresidentProperty, value); }
		}
		#endregion


		#region Salary
		internal static readonly PropertyDefinition<Int32?> SalaryProperty
	= OD.Properties.Add<Int32?>("Salary", "Salary", p =>
	{
		Assert.That(p)
		  .IsConvertable();

	});

		public Int32? Salary
		{
			get { return OI.GetValue(SalaryProperty); }
			set { OI.SetValue(SalaryProperty, value); }
		}



		#endregion

		#region Score
		internal static readonly PropertyDefinition<int> ScoreProperty
		= OD.Properties.Add<int>("Score", "What's your score?", p =>
		{
			Assert.That(p)
		  .IsRequired();
		});

		public int Score
		{
			get { return OI.GetValue(ScoreProperty); }
			set { OI.SetValue(ScoreProperty, value); }
		}

		#endregion




		public string Error
		{
			get
			{
				return ((IDataErrorInfo)OI).Error;
			}
		}

		public string this[string columnName]
		{
			get
			{
				return ((IDataErrorInfo)OI)[columnName];
			}
		}

		public Boolean IsValid
		{
			get
			{
				return OI.Validate();
			}
		}




		public void MakeValid()
		{
			//MiddleName = "Roy";
			HireDate = DateTime.Today;
			BirthDate = DateTime.Today.AddYears(-25);
			LastName = "Levitt";
			FirstName = "Brandon";
			Score = 0;
		}
		public void MakeValidForPresident()
		{
			MakeValid();
			BirthDate = DateTime.Today.AddYears(-40);
			IsPresident = true;
		}


	}
}
