#nullable enable
using Rumi.CustomBinding.Editor.UIElements.Bindings;
using System;
using UnityEditor;
using UnityEngine.UIElements;

[CustomPropertyBinder(typeof(MyStruct))]
public class MyStructPropertyBinder : PropertyBinder
{
    public override object Read(VisualElement element, SerializedProperty property, Type propertyType)
    {
        property.Next(true);
        string name = property.stringValue;
        
        property.Next(false);
        float value = property.floatValue;

        return new MyStruct { name = name, value = value };
    }
    public override void Write(VisualElement element, SerializedProperty property, Type propertyType, object? value)
    {
        if (value is MyStruct myStruct)
        {
            property.Next(true);
            property.stringValue = myStruct.name;
            
            property.Next(false);
            property.floatValue = myStruct.value;
        }
    }
}
