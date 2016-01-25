using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Principle4.DryLogic
{
  public class Utility
  {
    public static string AddSpacesToCapitalCase(string value)
    {
      if (string.IsNullOrWhiteSpace(value)) return string.Empty;

      string final = string.Empty;
      for (int i = 0; i < value.Length; i++)
      {
        final += (Char.IsUpper(value[i]) && ((i == 0 || !Char.IsUpper(value[i - 1])) ||
                                             (i != (value.Length - 1) && !Char.IsUpper(value[i + 1]))) ?
                  " " : "") + value[i];
      }

      return final.TrimStart(' ');
    }

    public static string GetPropertyName<TModel, TProperty>(Expression<Func<TModel, TProperty>> property)
    {
      var lambda = (LambdaExpression)property;

      MemberExpression memberExpression;
      if (lambda.Body is UnaryExpression)
      {
        var unaryExpression = (UnaryExpression)lambda.Body;
        memberExpression = (MemberExpression)unaryExpression.Operand;
      }
      else
      {
        memberExpression = (MemberExpression)lambda.Body;
      }

      return memberExpression.Member.Name;
    }

  }
}