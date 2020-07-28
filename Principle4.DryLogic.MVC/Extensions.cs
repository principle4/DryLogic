using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Principle4.DryLogic.MVC.Extensions
{
	public static class HelperExtensions
	{
		public static MvcHtmlString DryDropDownListFor<TModel, TProperty>(
			  this HtmlHelper<TModel> htmlHelper,
			  Expression<Func<TModel, TProperty>> expression,
			  IEnumerable<SelectListItem> selectList,
			  string optionLabel,
			  object htmlAttributes)
		{
			var name = ExpressionHelper.GetExpressionText((LambdaExpression)expression);
			string fullHtmlFieldName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);

			ModelMetadata metadata = ModelMetadata.FromLambdaExpression<TModel, TProperty>(expression, htmlHelper.ViewData);
			var currentValue = metadata.Model;

			//list of selected values
			List<String> values = new List<string>();

			//is the currentValue a list (but not a string which is a list of chars)...
			var enumerable = currentValue as System.Collections.IEnumerable;
			if (enumerable != null && currentValue?.GetType() != typeof(String))
			{
				foreach(var item in enumerable)
				{
					values.Add(item.ToString());
				}
			}
			///otherwise, is it a single, non-null value?
			else if(currentValue != null)
			{
				values.Add(currentValue.ToString());
			}

			var selectListWithSelected = selectList.Select(s =>
				new SelectListItem()
				{
					Text = s.Text,
					Value = s.Value,
					Disabled = s.Disabled,
					Group = s.Group,
					Selected = values.Contains(s.Value)
				}
			);

			///this didn't work...ModelState cannot be injected because it is scoped (internally by CopyOnWriteDictionary):
			///https://github.com/aspnet/Mvc/issues/878
			/// dougbu commented on Jul 28, 2014

			//			@NickCraver please note your last suggestion would undermine the contract MVC upholds w.r.t.ViewDataDictionary instances 
			//				--individual settings are scoped, not global. For example, a change to ViewBag.MyItem in a ViewComponent does not 
			//				affect reads of ViewBag.MyItem(or equivalently ViewData["MyItem"]) in the containing IView.

			//T			herefore the solution to your last issue must involve copy-on - write semantics.
			//				This would be relatively straightforward since ViewDataDictionary does not directly expose 
			//				_data(_innerDictionary in MVC 5.2) to callers. That is, we have complete control of how the 
			//				indexer's setter, Add(), Clear(), and Remove() are implemented.


			//htmlHelper.ViewData.ModelState.SetModelValue(fullHtmlFieldName
			//	,new ValueProviderResult("", "", System.Globalization.CultureInfo.CurrentUICulture));
			//return htmlHelper.DropDownList(name, selectListWithSelected, optionLabel, htmlAttributes);
			return htmlHelper.SelectInternal(metadata, optionLabel, name, selectListWithSelected, false, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}


		//based on https://github.com/mono/aspnetwebstack/blob/master/src/System.Web.Mvc/Html/SelectExtensions.cs
		private static MvcHtmlString SelectInternal(this HtmlHelper htmlHelper, ModelMetadata metadata,
			string optionLabel, string name, IEnumerable<SelectListItem> selectList, bool allowMultiple,
			IDictionary<string, object> htmlAttributes)
		{
			string fullName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
			if (String.IsNullOrEmpty(fullName))
			{
				throw new ArgumentNullException(nameof(name));
			}


			//DRY - Don't need it - we ARE passing in the select list
			//bool usedViewData = false;
			//// If we got a null selectList, try to use ViewData to get the list of items.
			//if (selectList == null)
			//{
			//	selectList = htmlHelper.GetSelectData(name);
			//	usedViewData = true;
			//}

			//DRY - Don't need default - setting that on the items
			//object defaultValue = (allowMultiple) ? htmlHelper.GetModelStateValue(fullName, typeof(string[])) : htmlHelper.GetModelStateValue(fullName, typeof(string));

			//// If we haven't already used ViewData to get the entire list of items then we need to
			//// use the ViewData-supplied value before using the parameter-supplied value.
			//if (defaultValue == null)
			//{
			//	if (!usedViewData && !String.IsNullOrEmpty(name))
			//	{
			//		defaultValue = htmlHelper.ViewData.Eval(name);
			//	}
			//	else if (metadata != null)
			//	{
			//		defaultValue = metadata.Model;
			//	}
			//}

			//if (defaultValue != null)
			//{
			//	selectList = GetSelectListWithDefaultValue(selectList, defaultValue, allowMultiple);
			//}

			// Convert each ListItem to an <option> tag and wrap them with <optgroup> if requested.
			StringBuilder listItemBuilder = BuildItems(optionLabel, selectList);

			TagBuilder tagBuilder = new TagBuilder("select")
			{
				InnerHtml = listItemBuilder.ToString()
			};
			tagBuilder.MergeAttributes(htmlAttributes);
			tagBuilder.MergeAttribute("name", fullName, true /* replaceExisting */);
			tagBuilder.GenerateId(fullName);
			if (allowMultiple)
			{
				tagBuilder.MergeAttribute("multiple", "multiple");
			}

			// If there are any errors for a named field, we add the css attribute.
			ModelState modelState;
			if (htmlHelper.ViewData.ModelState.TryGetValue(fullName, out modelState))
			{
				if (modelState.Errors.Count > 0)
				{
					tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
				}
			}

			tagBuilder.MergeAttributes(htmlHelper.GetUnobtrusiveValidationAttributes(name, metadata));

			//https://stackoverflow.com/a/3428203/852208
			//return tagBuilder.ToMvcHtmlString(TagRenderMode.Normal);
			return MvcHtmlString.Create(tagBuilder.ToString(TagRenderMode.Normal));
		}

		private static StringBuilder BuildItems(
	  string optionLabel,
	  IEnumerable<SelectListItem> selectList)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (optionLabel != null)
				stringBuilder.AppendLine(ListItemToOption(new SelectListItem()
				{
					Text = optionLabel,
					Value = string.Empty,
					Selected = false
				}));
			foreach (IGrouping<int, SelectListItem> source in selectList.GroupBy<SelectListItem, int>((Func<SelectListItem, int>)(i =>
			{
				if (i.Group != null)
					return i.Group.GetHashCode();
				return i.GetHashCode();
			})))
			{
				SelectListGroup group = source.First<SelectListItem>().Group;
				TagBuilder tagBuilder = (TagBuilder)null;
				if (group != null)
				{
					tagBuilder = new TagBuilder("optgroup");
					if (group.Name != null)
						tagBuilder.MergeAttribute("label", group.Name);
					if (group.Disabled)
						tagBuilder.MergeAttribute("disabled", "disabled");
					stringBuilder.AppendLine(tagBuilder.ToString(TagRenderMode.StartTag));
				}
				foreach (SelectListItem selectListItem in (IEnumerable<SelectListItem>)source)
					stringBuilder.AppendLine(ListItemToOption(selectListItem));
				if (group != null)
					stringBuilder.AppendLine(tagBuilder.ToString(TagRenderMode.EndTag));
			}
			return stringBuilder;
		}

		internal static string ListItemToOption(SelectListItem item)
		{
			TagBuilder builder = new TagBuilder("option")
			{
				InnerHtml = System.Web.HttpUtility.HtmlEncode(item.Text)
			};
			if (item.Value != null)
			{
				builder.Attributes["value"] = item.Value;
			}
			if (item.Selected)
			{
				builder.Attributes["selected"] = "selected";
			}
			if (item.Disabled)
			{
				builder.Attributes["disabled"] = "disabled";
			}
			return builder.ToString(TagRenderMode.Normal);
		}



	}


}
