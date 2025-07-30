#nullable enable
#if !RUNI_ENGINE
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Rumi.CustomBinding.Editor.Patches
{
    static class ScriptAttributeUtility
    {
        public static Assembly UnityEditor_CoreModule => _UnityEditor_CoreModule ??= ReflectionUtility.assemblys.First(static x => x.GetName().Name == "UnityEditor.CoreModule");
        static Assembly? _UnityEditor_CoreModule;
        
        public static Type type { get; } = UnityEditor_CoreModule.GetType("UnityEditor.ScriptAttributeUtility");
        
        
        
        static MethodInfo? m_GetFieldInfoFromProperty;
        static readonly object?[] mp_GetFieldInfoFromProperty = new object?[2];
        static readonly Type[] mpt_GetFieldInfoFromProperty = new Type[] { typeof(SerializedProperty), typeof(Type).MakeByRefType() };
        public static FieldInfo? GetFieldInfoFromProperty(this SerializedProperty property, out Type? type)
        {
            m_GetFieldInfoFromProperty ??= ScriptAttributeUtility.type.GetMethod("GetFieldInfoFromProperty", BindingFlags.NonPublic | BindingFlags.Static, null, mpt_GetFieldInfoFromProperty, null);

            mp_GetFieldInfoFromProperty[0] = property;
            mp_GetFieldInfoFromProperty[1] = null;
            
            FieldInfo? result = (FieldInfo?)m_GetFieldInfoFromProperty!.Invoke(null, mp_GetFieldInfoFromProperty);
            
            type = (Type?)mp_GetFieldInfoFromProperty[1];
            return result;
        }
    }
}
#endif