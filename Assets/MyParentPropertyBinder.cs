#nullable enable
using Rumi.CustomBinding.Editor;
using System;
using UnityEditor;
using UnityEngine.UIElements;

[CustomPropertyBinder(typeof(MyParent), true)]
public class MyParentPropertyBinder : PropertyBinder
{
    public override object Read(VisualElement element, SerializedProperty property, Type propertyType) => property.boxedValue;
    public override void Write(VisualElement element, SerializedProperty property, Type propertyType, object? value) => property.boxedValue = value;
}