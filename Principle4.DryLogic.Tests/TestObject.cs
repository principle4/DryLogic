using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Principle4.DryLogic.Validation;

namespace Principle4.DryLogic.Tests
{
  public class TestObject
  {
    static ObjectDefinition<TestObject> OD = new ObjectDefinition<TestObject>();
    internal ObjectInstance OI { get; set; }

    static TestObject()
    {

      //Assert.That(OD) //create the rule builder
      //  .AdhearsTo(oi => AgeProperty.GetValue(oi) > 40) //create and assign the rule inside of the rule builder
      //  .When(oi => IsPresidentProperty.GetValue(oi))
      //  .Notifying(AgeProperty,IsPresidentProperty);//Will require a separate set of rules - must be evaluated after the individual rules

    }

    public TestObject()
    {
      OI = OD.CreateInstance(this);
    }

    #region PropertyWithoutErrorFormatter
    public static readonly PropertyDefinition<String> PropertyWithoutErrorFormatterProperty
      //  = OD.Properties.Add(PropertyWithoutErrorFormatterProperty,"PropertyWithoutErrorFormatter", "First Name", p =>
      = OD.Properties.Add<String>("PropertyWithoutErrorFormatter", "Last Name", p =>
      {
        Assert.That(p)
          .IsRequired()
          ;
      });

    public String PropertyWithoutErrorFormatter
    {
      get { return OI.GetValue(PropertyWithoutErrorFormatterProperty); }
      set { OI.SetValue(PropertyWithoutErrorFormatterProperty, value); }
    }
    #endregion

  }
}
