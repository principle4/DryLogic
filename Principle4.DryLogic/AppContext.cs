using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Principle4.DryLogic
{
  public class AppContext
  {
    public AppContext()
    {
      IsHumanInterface = true;
    }
    /// <summary>
    /// Indicates that the human, not system names, will be used for properties and types
    /// </summary>
    public Boolean IsHumanInterface { get; set; }
  }
}
