#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rumi.CustomBinding.Editor
{
    static class ReflectionUtility
    {
        static ReflectionUtility() => Refresh();

        /// <summary>
        /// All loaded assemblys
        /// </summary>
        public static IReadOnlyList<Assembly> assemblys { get; private set; } = Array.Empty<Assembly>();

        /// <summary>
        /// All loaded types
        /// </summary>
        public static IReadOnlyList<Type> types { get; private set; } = Array.Empty<Type>();



        public static bool AttributeContains<T>(this MemberInfo element, bool inherit = true) where T : Attribute => element.AttributeContains(typeof(T), inherit);
        public static bool AttributeContains(this MemberInfo element, Type attribute, bool inherit = true) => Attribute.GetCustomAttributes(element, attribute, inherit).Length > 0;
        
        
        
        public static void Refresh()
        {
            assemblys = Array.AsReadOnly(AppDomain.CurrentDomain.GetAssemblies());
            types = Array.AsReadOnly(assemblys.SelectMany(static x => x.GetTypes()).ToArray());
        }

        public static MethodInfo GetMethodInfo(Delegate method) => method.Method;
        
        public static bool HasDefaultConstructor(this Type t, bool nonPublic = false)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            if (nonPublic)
                flags |= BindingFlags.NonPublic; 
            
            return t.IsValueType || t.GetConstructor(flags, null, Type.EmptyTypes, null) != null;
        }
    }
}
