using System.Collections.Generic;
using System.Text.RegularExpressions;
using Fourth.Import.Model;

namespace Fourth.Import.Sql
{
    public static class Sqlify
    {
         private static readonly Dictionary<string, string> BooleanValues = new Dictionary<string, string>();
         private static readonly Dictionary<string, string> IntoleranceValues = new Dictionary<string, string>();

        static Sqlify()
        {
            BooleanValues.Add("Y", "1");
            BooleanValues.Add("YES", "1");
            BooleanValues.Add("N", "0");
            BooleanValues.Add("NO", "0");
            BooleanValues.Add("FOOD", "0");
            BooleanValues.Add("NON-FOOD", "1");

            IntoleranceValues.Add("Y", "1");
            IntoleranceValues.Add("YES", "1");
            IntoleranceValues.Add("N", "0");
            IntoleranceValues.Add("NO", "0");
            IntoleranceValues.Add("M", "2");
            IntoleranceValues.Add("MAY", "2");
        }

        public static string WithSqlSyntax(this string importValue, Datatype datatype)
        {           
            importValue = string.IsNullOrEmpty(importValue)? string.Empty : importValue.Trim();
            switch (datatype)
            {
                case Datatype.Numeric:
                    
                    return string.IsNullOrEmpty(importValue) ? "NULL" : importValue.RemoveInvalidCharacters();
                case Datatype.Dynamic:
                    return importValue;

                case Datatype.DateTime:
                    return string.IsNullOrEmpty(importValue) ? "NULL" : "'" + importValue.Replace("'", "''") + "'";

                case Datatype.String:
                    return "'" + importValue.Replace("'","''") + "'";

                case Datatype.Bit:
                    string val;
                    if (BooleanValues.TryGetValue(importValue.ToUpperInvariant(), out val))
                        return val;
                    else  
                    {
                        if (string.IsNullOrEmpty(importValue))
                            return "NULL";
                        else
                            return "INVALID";
                    }
                case Datatype.Intolerance:
                    string intolVal;
                    if (IntoleranceValues.TryGetValue(importValue.ToUpperInvariant(), out intolVal))
                        return intolVal;
                    else
                    {
                        if (string.IsNullOrEmpty(importValue))
                            return "NULL";
                        else
                            return "INVALID";
                    }
            }
            return importValue;
        }

        public static string RemoveInvalidCharacters(this string importValue)
        {
            //Replace , and ' with blank values. Insert all invalid charaters in the same expression to remove it from string.
            return Regex.Replace(importValue, @"[,']", "");
        }
    }
}