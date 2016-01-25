using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Principle4.DryLogic
{
	public class App
	{
    [ThreadStatic]
    public static AppContext currentContext;
    public static AppContext CurrentContext
    {
      get{
        if(currentContext == null)
          currentContext = new AppContext();
        return currentContext;
      }
    }
		
    //at some point will probably move to some sort of utility class, but this is fine for now
    public static readonly ReadOnlyCollection<Type> KnownValueTypes = new List<Type>{
			typeof(Int64),
      typeof(UInt64),
      typeof(Int32),
      typeof(UInt32),
      typeof(Int16),
      typeof(UInt16),
      typeof(SByte),
      typeof(Byte),
      typeof(Single),
      typeof(Decimal),
      typeof(Double),
      typeof(DateTime),
			typeof(Char),
      typeof(Boolean)
    }.AsReadOnly();

	}
}
