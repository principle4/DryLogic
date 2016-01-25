using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Principle4.DryLogic
{
  class DryLogicException : Exception
  {
    public DryLogicException(string message) : base(message)
    {
    }
    public DryLogicException(string message, Exception innerException)
      : base(message, innerException)
    { }
  }
}
