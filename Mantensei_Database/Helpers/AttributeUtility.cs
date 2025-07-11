
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MantenseiLib
{
    public static partial class AttributeUtility
    {
        /// <summary>
        /// 指定したプロパティと属性のペアを返します。
        /// </summary>
        public static IEnumerable<AttributedMember<T>> GetAttributedFields<T>(object obj) where T : Attribute
        {
            Type type = obj.GetType();

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            var fieldPairs = GetAttributePairs<T>(fields);
            var propertyPairs = GetAttributePairs<T>(properties);

            return fieldPairs.Concat(propertyPairs);
        }

        static IEnumerable<AttributedMember<T>> GetAttributePairs<T>(IEnumerable<MemberInfo> members) where T : Attribute
        {
            foreach (var member in members)
            {
                var attributes = GetAttributes<T>(member);
                foreach (var attribute in attributes)
                    yield return new AttributedMember<T>(member, attribute);
            }
        }

        public static IEnumerable<T> GetAttributes<T>(MemberInfo info) where T : Attribute
        {
            var attributes = Attribute.GetCustomAttributes(info, typeof(T), true);
            if (attributes == null)
                yield break;

            foreach (var attribute in attributes)
                yield return (T)attribute;
        }

        public static object? GetValue(this MemberInfo memberInfo, object forObject)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).GetValue(forObject);
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).GetValue(forObject);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class AttributedMember<T> where T : Attribute
    {
        public MemberInfo MemberInfo { get; }
        public T Attribute { get; }

        public AttributedMember(MemberInfo memberInfo, T attribute)
        {
            MemberInfo = memberInfo;
            Attribute = attribute;
        }

        public object? GetValue(object target)
        {
            return MemberInfo.GetValue(target);
        }
    }
}
