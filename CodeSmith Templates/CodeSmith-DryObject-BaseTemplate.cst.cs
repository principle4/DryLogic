using System;
using System.Linq;
using System.Data;
using System.ComponentModel;
using CodeSmith.Engine;
using SchemaExplorer;
using System.Collections.Generic;

public class PropertyDefinition{
    public String Name { get; set; }
    public String Type { get; set; }
    public Boolean IsRequired { get; set;}
}

public static class Extensions
{
    public static IEnumerable<String> WithLeadingComma(this IEnumerable<String> stringEnumerator)
    {
        return WithLeading(stringEnumerator,",");
    }    
    public static IEnumerable<String> WithLeading(this IEnumerable<String> stringEnumerator, String separator)
    {
        using (var enumerator = stringEnumerator.GetEnumerator())
        {
            if (!enumerator.MoveNext())
                throw new InvalidOperationException("Property collection contained no elements");

            String first = enumerator.Current;
            do
            {
                yield return (enumerator.Current.Equals(first)?"":separator) + enumerator.Current;
            }
            while (enumerator.MoveNext());
        }
    }    
    
    
    
    public static IEnumerable<String> WithTrailingComma(this IEnumerable<String> stringEnumerator)
    {
        return WithTrailing(stringEnumerator,",");
    }
    
    public static IEnumerable<String> WithTrailing(this IEnumerable<String> stringEnumerator, String separator)
    {
        using (var enumerator = stringEnumerator.GetEnumerator())
        {
            if (!enumerator.MoveNext())
                throw new InvalidOperationException("Property collection contained no elements");
            
            var hasMore = false;
            do
            {
                var currentValue = enumerator.Current;
                hasMore = enumerator.MoveNext();
                yield return currentValue + (hasMore?separator:"");
            }
            while (hasMore);
        }
    }    

    
}

public class TemplateBase : CodeTemplate
{
    
    public enum DBTypeOption
    {
        SqlServer=1,
        Oracle=2
    }
    
    public MapCollection DbTypeMapCSharp {get; set;}
    
    private SchemaExplorer.TableSchema SourceTable {
        get
        {
            return (SchemaExplorer.TableSchema) GetProperty("SourceTable");
        }
    }
    private DBTypeOption DBType
    {
        get
        {
            return (DBTypeOption) GetProperty("DBType");
        }
    }

    protected String PP="";
    
    
    public void Write(String format, params object[] args)
    {
        //trim the leading carraige return.   this allows the template to be cleaner with the @" on the previous line
        //format = format.TrimStart('\r', '\n');
        
        Response.Write(format, args);
    }
    
    public void WriteForEach<T>(IEnumerable<T> collection, String format, String delim, Func<T,Object[]> argsFunction)
    {
        using (IEnumerator<T> enumerator = collection.GetEnumerator())
        {
            if (enumerator.MoveNext())  
            {
                T first = enumerator.Current;
                do
                {
                    //writing the delim for the previous item
                    if(enumerator.Current.Equals(first) == false)
                        Response.Write(delim);
                    Write(format, argsFunction(enumerator.Current));
                }
                while (enumerator.MoveNext());
            }
            else
            {
                throw new InvalidOperationException("Property collection contained no elements");
            }
        }
    }


    protected override void OnInit()
    {
        //System.Diagnostics.Debugger.Launch();
        //System.Diagnostics.Debugger.Break();
        if(DBType != DBTypeOption.SqlServer)
            PP="@";
        else
            PP=":";
    }

//    public string GetMemberVariableDeclarationStatement(ColumnSchema column) {
//        return GetMemberVariableDeclarationStatement("private", column);
//    }

//    public string GetMemberVariableDeclarationStatement(string protectionLevel, ColumnSchema column) {
//        string statement = protectionLevel + " ";
//        statement += GetCSharpVariableType(column) + " " + GetMemberVariableName(column);
//
//        string defaultValue = GetMemberVariableDefaultValue(column);
//        if (defaultValue != "") {
//            statement += " = " + defaultValue;
//        }
//
//        statement += ";";
//
//        return statement;
//    }

//    public string GetReaderAssignmentStatement(ColumnSchema column, int index) {
//        string statement = "if (!reader.IsDBNull(" + index.ToString() + ")) ";
//        statement += GetMemberVariableName(column) + " = ";
//
//        if (column.Name.EndsWith("TypeCode")) statement += "(" + column.Name + ")";
//
//        statement += "reader." + GetReaderMethod(column) + "(" + index.ToString() + ");";
//
//        return statement;
//    }

    public string GetCamelCaseName(string value) {
        return value.Substring(0, 1).ToLower() + value.Substring(1);
    }

    //only for delimited names (oracle)
    public string GetPascalCaseName(string value) {
        if(DBType == DBTypeOption.SqlServer)
            return value;

        
        value = value.ToLower();

        var parts = value.Split('_');

        for (int i = 0; i< parts.Length; i++) {
            parts[i] = parts[i].Substring(0, 1).ToUpper() + parts[i].Substring(1);
        }
        return String.Join("", parts);
    }

//    public string GetMemberVariableName(ColumnSchema column) {
//        string propertyName = GetPropertyName(column);
//        string memberVariableName = "_" + GetCamelCaseName(propertyName);
//
//        return memberVariableName;
//    }
    
    //public Func<List<ColumnSchema>> PrivateSetterPropertySelector {get; set;} = () => SourceTable.PrimaryKey.MemberColumns.ToList();
        //all columns set by an identity
        //not sure if this will work with oracle
        //= () => SourceTable.Columns.Where((c) => (bool)c.ExtendedProperties["CS_IsIdentity"].Value).ToList();
    
    protected virtual List<PropertyDefinition> PrivateSetterProperties{
        get{
            //return SourceTable.PrimaryKey.MemberColumns.Select(
            //    c => new PropertyDefinition(){Name=GetPropertyName(c), Type=GetCSharpVariableType(c)}).ToList();
            return IdentityValueColumns
                .Select(c => new PropertyDefinition() { Name = GetPropertyName(c), Type = c.SystemType.Name })
                .ToList();
            
        }
    }

    protected virtual List<MemberColumnSchema> PrimaryKeyColumns
    {
        get
        {
            return SourceTable?.PrimaryKey.MemberColumns.ToList();
        }
    }    
    
    protected virtual List<ColumnSchema> NonKeyColumns
    {
        get
        {
            return SourceTable?.NonPrimaryKeyColumns.ToList();
        }
    }
    
    protected virtual List<PropertyDefinition> RegularProperties
    {
        get
        {
            return SourceTable?.Columns.ToArray()
                .Select(c => new PropertyDefinition() { Name = GetPropertyName(c), Type = c.SystemType.Name + (c.AllowDBNull?"?":""), IsRequired = !c.AllowDBNull })
                //where it's not already a private setter col
                .Where(p => PrivateSetterProperties.Any(ps => ps.Name == p.Name) == false)
                .ToList();
        }
    }
    
    protected virtual List<PropertyDefinition> ParameterValueProperties
    {
        get
        {
            return RegularProperties;
        }
    }

    protected virtual List<ColumnSchema> IdentityValueColumns
    {
        get
        {
            //all columns set by an identity
            //not sure if this will work with oracle
            return SourceTable?.Columns
                .Where((c) => (bool)c.ExtendedProperties["CS_IsIdentity"].Value)
                .ToList();
        }
    }
    protected virtual List<ColumnSchema> ReturnedValueColumns
    {
        get
        {
            return IdentityValueColumns;
        }
    }
    protected virtual List<ColumnSchema> InsertableColumns
    {
        get
        {
            return SourceTable?.Columns
                .Where( c=> IdentityValueColumns.Contains(c) == false)
                .ToList();
        }
    }
    protected virtual List<ColumnSchema> SelectableColumns
    {
        get
        {
            return SourceTable?.Columns
                .ToList();
        }
    }
    	
    
    
//    public virtual Func<PropertyDefinition,String> ParameterValueOverrideProvider {get {return p => null;} }
    
//    public virtual Func<ColumnSchema,String> InsertValueOverrideProvider {get { return c => null;}}


    public string GetPropertyName(ColumnSchema column) {
        string propertyName = column.Name;
        propertyName = GetPascalCaseName(propertyName);

//        if (propertyName == column.Table.Name + "Name") return "Name";
//        if (propertyName == column.Table.Name + "Description") return "Description";

        //decided against this - too many type codes are NOT enums
//        if (propertyName.EndsWith("TypeCode")) propertyName = propertyName.Substring(0, propertyName.Length - 4);
//        
//        //if all that is left is "Type" than it must be a clasification of the object itself
//        if(propertyName.EndsWith("TypeCode") && propertyName != "Type")
//            propertyName = propertyName.Substring(0, propertyName.Length - 4);
            

        return propertyName;
    }

    public string GetMemberVariableDefaultValue(ColumnSchema column) {
        switch (column.DataType) {
            case DbType.Guid: {
                return "Guid.Empty";
            }
            case DbType.AnsiString:
            case DbType.AnsiStringFixedLength:
            case DbType.String:
            case DbType.StringFixedLength: {
                return "String.Empty";
            }
            default: {
                return "";
            }
        }
    }


//    public string GetReaderMethod(ColumnSchema column) {
//        return DbDataReader[column.DataType.ToString()];
//    }

    public string GetClassName(TableSchema table) {
        if (table.Name.EndsWith("s")) {
            return table.Name.Substring(0, table.Name.Length - 1);
        }
        else {
            return table.Name;
        }
    }

//    public string GetSqlDbType(ColumnSchema column) {
//        return SqlNativeSqlDb[column.NativeType.ToString()];
//    }

    public string GetPrimaryKeyType(TableSchema table) {
        if (table.PrimaryKey != null) {
            return table.PrimaryKey.MemberColumns[0].SystemType.Name;
//            if (table.PrimaryKey.MemberColumns.Count == 1) {
//                //return GetCSharpVariableType(table.PrimaryKey.MemberColumns[0]);
//                return table.PrimaryKey.MemberColumns[0].SystemType.Name;
//            }
//            else {
//                throw new ApplicationException("This template will not work on primary keys with more than one member column.");
//            }
        }
        else {
            throw new ApplicationException("This template will only work on tables with a primary key.");
        }
    }

    public override string GetFileName() {
        return this.GetClassName(this.SourceTable) + ".cs";
        //var sourceTable = (SchemaExplorer.TableSchema)GetProperty("DbSourceTable");
        //return this.GetClassName(sourceTable) + ".cs";
    }
    
    public string GetCSharpVariableType(ColumnSchema column) {
        return GetCSharpVariableType(DbTypeMapCSharp, column);
    }
    
    public string GetCSharpVariableType(MapCollection dbyTypeCSharp, ColumnSchema column) {
        if (column.Name.EndsWith("TypeCode")) 
            return column.Name;
        
        if(IsOracleBool(column))
        {
            return "bool";
        }   
        return dbyTypeCSharp[column.DataType.ToString()];
    }
    
    public Boolean IsOracleBool(ColumnSchema column)
    {
        return DBType == DBTypeOption.Oracle && column.NativeType == "NUMBER" && column.Scale==0 && column.Precision==1;
    }
    
}