using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Principle4.DryLogic.Tests;

namespace Principle4.DryLogic.Demos.WPF
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    Employee employee;
    DryLogicProxy employeeProxy;
    public MainWindow()
    {
      InitializeComponent();

      this.employee = new Employee();


      //the DynamicProxy is a dynamic object.  It's purpose is to reroute binding gets and sets to the StringValue 
      //of the DomainObject's DomainProperties.  It also exposes a couple of properties and methods on the DomainObject,
      //like IsValid and GetFirstInvalidMessage, but these are just shortcuts to exposing them explicitly on the DomainObject
      this.employeeProxy = new DryLogicProxy(employee);

      //supress validation while the form is loading - the bindings update the source values causing the validation to fire on a fresh form.
      //Set this to true to suppress validation messages until after an input gets data.
      this.employeeProxy.IsValidationSuppressed = true;



      InitializeComponent();
      PnlForm.DataContext = this.employeeProxy;
      this.employee.HireDate = DateTime.Today;
      //test toolkit's DateTimePicker resetting the property value
      //this.employee.DomainContainer.PropertyChanged += (sender, e) =>
      //{

      //};
    }
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {

      this.employeeProxy.IsValidationSuppressed = false;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
      List<RuleViolation> ruleViolations = null;
      if (!this.employeeProxy.Validate(out ruleViolations))
      {
        this.employeeProxy.RaiseChangedForAllProperties();
        MessageBox.Show(ruleViolations[0].ErrorMessage);
      }

      else
      {
        MessageBox.Show("It's good!");
      }
    }

  }
}
