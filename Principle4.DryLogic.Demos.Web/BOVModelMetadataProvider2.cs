using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Principle4.DryLogic;

namespace Principle4.DryLogic.MVC
{
  //another good reference:
  //http://dotnetslackers.com/articles/aspnet/Customizing-ASP-NET-MVC-2-Metadata-and-Validation.aspx

  //An attempt and echoing back different metadata then the bound property
  public class BOVModelMetadataProvider2 : DataAnnotationsModelMetadataProvider
  {
    //public override IEnumerable<ModelMetadata> GetMetadataForProperties(object container, Type containerType)
    //{
    //  return base.GetMetadataForProperties(container, containerType);
    //}

    //public override ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType)
    //{
    //  return base.GetMetadataForType(modelAccessor, modelType);
    //}

    //public override ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, string propertyName)
    //{
    //  return base.GetMetadataForProperty(modelAccessor, containerType, propertyName);
    //}

    //protected override ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, System.ComponentModel.PropertyDescriptor propertyDescriptor)
    //{
    //  return base.GetMetadataForProperty(modelAccessor, containerType, propertyDescriptor);
    //}
    protected override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName)
    {
      //Reflected from base class
      List<Attribute> list = new List<Attribute>(attributes);
      DisplayColumnAttribute displayColumnAttribute = Enumerable.FirstOrDefault<DisplayColumnAttribute>(Enumerable.OfType<DisplayColumnAttribute>((IEnumerable) list));
      
      DataAnnotationsModelMetadata annotationsModelMetadata = null;

      //brl additions
      //if this is a BOV backed object
      if (containerType != null && Attribute.IsDefined(containerType, typeof(DryLogicObjectAttribute)) && propertyName != "OI")
      {
        Object container = null;
        ObjectInstance oi = null;
        //By having this code here instead of in GetMetadataForProperty, some of the normal features like ui hint will work
        if (modelAccessor != null)
        {
          var rootModelType = modelAccessor.Target.GetType();
          var field = rootModelType.GetField("container");
          if (field != null)
          {
            container = field.GetValue(modelAccessor.Target);
            //if we don't have a reference to the container yet...
            if (container.GetType() != containerType)
            {
              //...then try to break down the expression to get it
              //get the expression as text, ie "model.EmployeeViewModel.MyEmployee" and split it
              var expressionParts = ((LambdaExpression)rootModelType.GetField("expression").GetValue(modelAccessor.Target)).Body.ToString().Split('.');
              //var expressionParts = new string[] { };

              //loop thru the parts in the middle
              for (int i = 1; i < expressionParts.Length - 1; i++)
              {
                container = container.GetType().GetProperty(expressionParts[i]).GetValue(container);
              }
            }
            //could use an attribute instead to identify the object instance
            oi = ObjectInstance.GetObjectInstance(container);

            if (oi != null)//not really sure how this woudl fail at this point
            {
              annotationsModelMetadata = new PropertyValueMetadata(this, containerType, modelAccessor, modelType, propertyName, displayColumnAttribute);
              annotationsModelMetadata.Container = container;
              //internally, setting model wipes out modelAccessor (caching of sorts)
              annotationsModelMetadata.Model = oi.PropertyValues[propertyName];
              annotationsModelMetadata.TemplateHint = "PropertyValue";
            }
          }
        }
      }
      if (annotationsModelMetadata == null)
        annotationsModelMetadata = new DataAnnotationsModelMetadata(this, containerType, modelAccessor, modelType, propertyName, displayColumnAttribute);

      
      HiddenInputAttribute hiddenInputAttribute = Enumerable.FirstOrDefault<HiddenInputAttribute>(Enumerable.OfType<HiddenInputAttribute>((IEnumerable) list));
      if (hiddenInputAttribute != null)
      {
        annotationsModelMetadata.TemplateHint = "HiddenInput";
        annotationsModelMetadata.HideSurroundingHtml = !hiddenInputAttribute.DisplayValue;
      }
      IEnumerable<UIHintAttribute> source = Enumerable.OfType<UIHintAttribute>((IEnumerable) list);
      UIHintAttribute uiHintAttribute = Enumerable.FirstOrDefault<UIHintAttribute>(source, (Func<UIHintAttribute, bool>) (a => string.Equals(a.PresentationLayer, "MVC", StringComparison.OrdinalIgnoreCase))) ?? Enumerable.FirstOrDefault<UIHintAttribute>(source, (Func<UIHintAttribute, bool>) (a => string.IsNullOrEmpty(a.PresentationLayer)));
      if (uiHintAttribute != null)
        annotationsModelMetadata.TemplateHint = uiHintAttribute.UIHint;
      DataTypeAttribute attribute = Enumerable.FirstOrDefault<DataTypeAttribute>(Enumerable.OfType<DataTypeAttribute>((IEnumerable) list));
      if (attribute != null)
        annotationsModelMetadata.DataTypeName = DataTypeUtil.ToDataTypeName(attribute, (Func<DataTypeAttribute, bool>) null);
      EditableAttribute editableAttribute = Enumerable.FirstOrDefault<EditableAttribute>(Enumerable.OfType<EditableAttribute>((IEnumerable) attributes));
      if (editableAttribute != null)
      {
        annotationsModelMetadata.IsReadOnly = !editableAttribute.AllowEdit;
      }
      else
      {
        ReadOnlyAttribute readOnlyAttribute = Enumerable.FirstOrDefault<ReadOnlyAttribute>(Enumerable.OfType<ReadOnlyAttribute>((IEnumerable) list));
        if (readOnlyAttribute != null)
          annotationsModelMetadata.IsReadOnly = readOnlyAttribute.IsReadOnly;
      }
      DisplayFormatAttribute displayFormatAttribute = Enumerable.FirstOrDefault<DisplayFormatAttribute>(Enumerable.OfType<DisplayFormatAttribute>((IEnumerable) list));
      if (displayFormatAttribute == null && attribute != null)
        displayFormatAttribute = attribute.DisplayFormat;
      if (displayFormatAttribute != null)
      {
        annotationsModelMetadata.NullDisplayText = displayFormatAttribute.NullDisplayText;
        annotationsModelMetadata.DisplayFormatString = displayFormatAttribute.DataFormatString;
        annotationsModelMetadata.ConvertEmptyStringToNull = displayFormatAttribute.ConvertEmptyStringToNull;
        if (displayFormatAttribute.ApplyFormatInEditMode)
          annotationsModelMetadata.EditFormatString = displayFormatAttribute.DataFormatString;
        if (!displayFormatAttribute.HtmlEncode && string.IsNullOrWhiteSpace(annotationsModelMetadata.DataTypeName))
          annotationsModelMetadata.DataTypeName = DataTypeUtil.HtmlTypeName;
      }
      ScaffoldColumnAttribute scaffoldColumnAttribute = Enumerable.FirstOrDefault<ScaffoldColumnAttribute>(Enumerable.OfType<ScaffoldColumnAttribute>((IEnumerable) list));
      if (scaffoldColumnAttribute != null)
        annotationsModelMetadata.ShowForDisplay = annotationsModelMetadata.ShowForEdit = scaffoldColumnAttribute.Scaffold;
      DisplayAttribute displayAttribute = Enumerable.FirstOrDefault<DisplayAttribute>(Enumerable.OfType<DisplayAttribute>((IEnumerable) attributes));
      string str = (string) null;
      if (displayAttribute != null)
      {
        annotationsModelMetadata.Description = displayAttribute.GetDescription();
        annotationsModelMetadata.ShortDisplayName = displayAttribute.GetShortName();
        annotationsModelMetadata.Watermark = displayAttribute.GetPrompt();
        annotationsModelMetadata.Order = displayAttribute.GetOrder() ?? 10000;
        str = displayAttribute.GetName();
      }
      if (str != null)
      {
        annotationsModelMetadata.DisplayName = str;
      }
      else
      {
        DisplayNameAttribute displayNameAttribute = Enumerable.FirstOrDefault<DisplayNameAttribute>(Enumerable.OfType<DisplayNameAttribute>((IEnumerable) list));
        if (displayNameAttribute != null)
          annotationsModelMetadata.DisplayName = displayNameAttribute.DisplayName;
      }
      if (Enumerable.FirstOrDefault<RequiredAttribute>(Enumerable.OfType<RequiredAttribute>((IEnumerable) list)) != null)
        annotationsModelMetadata.IsRequired = true;
      return (ModelMetadata) annotationsModelMetadata;
    }

  }

  public class PropertyValueMetadata : DataAnnotationsModelMetadata
  {
    public PropertyValue CurrentPropertyValue { get; internal set; }

    public PropertyValueMetadata(DataAnnotationsModelMetadataProvider provider, Type containerType,  Func<object> modelAccessor, Type modelType, String propertyName, System.ComponentModel.DataAnnotations.DisplayColumnAttribute displayColumnAttribute) 
      : base(provider, containerType, modelAccessor, modelType, propertyName, displayColumnAttribute)
    {
      //CurrentPropertyValue = (PropertyValue)modelAccessor();
    }
  }

  //reflected from System.Web.Mvc.DataTypeUtil (it's called from CreateMetadata
  internal static class DataTypeUtil
  {
    internal static readonly string CurrencyTypeName = ((object)DataType.Currency).ToString();
    internal static readonly string DateTypeName = ((object)DataType.Date).ToString();
    internal static readonly string DateTimeTypeName = ((object)DataType.DateTime).ToString();
    internal static readonly string DurationTypeName = ((object)DataType.Duration).ToString();
    internal static readonly string EmailAddressTypeName = ((object)DataType.EmailAddress).ToString();
    internal static readonly string HtmlTypeName = ((object)DataType.Html).ToString();
    internal static readonly string ImageUrlTypeName = ((object)DataType.ImageUrl).ToString();
    internal static readonly string MultiLineTextTypeName = ((object)DataType.MultilineText).ToString();
    internal static readonly string PasswordTypeName = ((object)DataType.Password).ToString();
    internal static readonly string PhoneNumberTypeName = ((object)DataType.PhoneNumber).ToString();
    internal static readonly string TextTypeName = ((object)DataType.Text).ToString();
    internal static readonly string TimeTypeName = ((object)DataType.Time).ToString();
    internal static readonly string UrlTypeName = ((object)DataType.Url).ToString();
    private static readonly Lazy<Dictionary<object, string>> _dataTypeToName = new Lazy<Dictionary<object, string>>(new Func<Dictionary<object, string>>(DataTypeUtil.CreateDataTypeToName), true);

    static DataTypeUtil()
    {
    }

    internal static string ToDataTypeName(this DataTypeAttribute attribute, Func<DataTypeAttribute, bool> isDataType = null)
    {
      if (isDataType == null)
        isDataType = (Func<DataTypeAttribute, bool>)(t => t.GetType().Equals(typeof(DataTypeAttribute)));
      if (isDataType(attribute))
      {
        string str = DataTypeUtil.KnownDataTypeToString(attribute.DataType);
        if (str == null)
          DataTypeUtil._dataTypeToName.Value.TryGetValue((object)attribute.DataType, out str);
        if (str != null)
          return str;
      }
      return attribute.GetDataTypeName();
    }

    private static string KnownDataTypeToString(DataType dataType)
    {
      switch (dataType)
      {
        case DataType.DateTime:
          return DataTypeUtil.DateTimeTypeName;
        case DataType.Date:
          return DataTypeUtil.DateTypeName;
        case DataType.Time:
          return DataTypeUtil.TimeTypeName;
        case DataType.Duration:
          return DataTypeUtil.DurationTypeName;
        case DataType.PhoneNumber:
          return DataTypeUtil.PhoneNumberTypeName;
        case DataType.Currency:
          return DataTypeUtil.CurrencyTypeName;
        case DataType.Text:
          return DataTypeUtil.TextTypeName;
        case DataType.Html:
          return DataTypeUtil.HtmlTypeName;
        case DataType.MultilineText:
          return DataTypeUtil.MultiLineTextTypeName;
        case DataType.EmailAddress:
          return DataTypeUtil.EmailAddressTypeName;
        case DataType.Password:
          return DataTypeUtil.PasswordTypeName;
        case DataType.Url:
          return DataTypeUtil.UrlTypeName;
        case DataType.ImageUrl:
          return DataTypeUtil.ImageUrlTypeName;
        default:
          return (string)null;
      }
    }

    private static Dictionary<object, string> CreateDataTypeToName()
    {
      Dictionary<object, string> dictionary = new Dictionary<object, string>();
      foreach (DataType dataType in Enum.GetValues(typeof(DataType)))
      {
        if (dataType != DataType.Custom && DataTypeUtil.KnownDataTypeToString(dataType) == null)
        {
          string name = Enum.GetName(typeof(DataType), (object)dataType);
          dictionary[(object)dataType] = name;
        }
      }
      return dictionary;
    }
  }

  //public class BOVModelMetadata : ModelMetadata
  //{
  //  public BOVModelMetadata(ObjectInstance oi, BOVModelMetadataProvider provider)
  //    : base(provider, oi.ObjectDefinition, new Func<Object>(() => oi))
  //  {

  //  }
  //  override 
  //}
  //http://stackoverflow.com/a/16680163



  public abstract class GenericBaseCopy<Src, Dst> where Dst : Src
  {
    private static List<Tuple<FieldInfo, FieldInfo>> _fieldsMap;

    static GenericBaseCopy()
    {
      _fieldsMap = new List<Tuple<FieldInfo, FieldInfo>>();
      foreach (FieldInfo customFI in GetAllFields(typeof(Dst)))
        foreach (FieldInfo baseFI in GetAllFields(typeof(Src)))
          if (customFI.Name == baseFI.Name)
            _fieldsMap.Add(new Tuple<FieldInfo, FieldInfo>(customFI, baseFI));
    }

    private static List<FieldInfo> GetAllFields(Type t)
    {
      List<FieldInfo> res = new List<FieldInfo>();

      while (t != null)
      {
        foreach (FieldInfo fi in t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
          if (!fi.IsLiteral)
            res.Add(fi);
        t = t.BaseType;
      }

      return res;
    }

    public static void Copy(Src baseClassInstance, Dst dstClassInstance)
    {
      foreach (Tuple<FieldInfo, FieldInfo> t in _fieldsMap)
        t.Item1.SetValue(dstClassInstance, t.Item2.GetValue(baseClassInstance));
    }
  }
}