using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StarChef.BackgroundServices.Common
{
    public static class ExtensionMethods
    {
        public static string ToGenericTypeString(this Type type)
        {
            return !type.GetTypeInfo().IsGenericType
                ? type.GetFullNameWithoutNamespace().ReplacePlusWithDotInNestedTypeName()
                : type.GetGenericTypeDefinition()
                    .GetFullNameWithoutNamespace()
                    .ReplacePlusWithDotInNestedTypeName()
                    .ReplaceGenericParametersInGenericTypeName(type);
        }

        private static string GetFullNameWithoutNamespace(this Type type)
        {
            if (type.IsGenericParameter)
                return type.Name;
            if (string.IsNullOrEmpty(type.Namespace))
                return type.FullName;
            return type.FullName.Substring(type.Namespace.Length + 1);
        }

        private static string ReplacePlusWithDotInNestedTypeName(this string typeName)
        {
            return typeName.Replace('+', '.');
        }

        private static string ReplaceGenericParametersInGenericTypeName(this string typeName, Type type)
        {
            var genericArguments = type.GetTypeInfo().GetAllGenericArguments();

            typeName = new Regex("`[1-9]\\d*").Replace(typeName, (MatchEvaluator)(match =>
            {
                var count = int.Parse(match.Value.Substring(1));
                var str = string.Join(",", genericArguments.Take(count).Select(ToGenericTypeString));

                genericArguments = genericArguments.Skip(count).ToArray();
                return "<" + str + ">";
            }));
            return typeName;
        }

        public static Type[] GetAllGenericArguments(this TypeInfo type)
        {
            return type.GenericTypeArguments.Length == 0
                ? type.GenericTypeParameters
                : type.GenericTypeArguments;
        }
    }
}
