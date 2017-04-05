using System;
using System.Reflection;

namespace StarChef.Listener.Tests.Helpers
{
    internal class PayloadHelpers
    {
        public static T Construct<T>()
        {
            return Construct<T>(new Type[] {});
        }

        public static T Construct<T>(Type[] paramTypes, params object[] paramValues)
        {
            var t = typeof (T);

            var ci = t.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, paramTypes, null);

            return (T) ci.Invoke(paramValues);
        }

        public static void SetProperty(object obj, string propertyName, object value)
        {
            var propertyInfo = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (propertyInfo == null)
                propertyInfo = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (propertyInfo == null)
                throw new ArgumentException(string.Format("Property '{0}' is not found in the object of type '{1}'.", propertyName, obj.GetType().Name), "propertyName");

            propertyInfo.SetValue(obj, value);
        }

        public static void SetField(object obj, string fieldName, object value)
        {
            var fieldInfo = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
            if (fieldInfo == null)
                fieldInfo = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (fieldInfo == null)
                throw new ArgumentException(string.Format("Field '{0}' is not found in the object of type '{1}'.", fieldName, obj.GetType().Name), "fieldName");

            fieldInfo.SetValue(obj, value);
        }
    }
}