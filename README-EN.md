# UI Toolkit Binding Support

Languages available in README: \[[한국어 (대한민국)](README.md)\] \[[**English (US)**](README-EN.md)\]

## Introduction

This package was created out of great shock upon realizing that Unity's next-generation UI system ~~(or so they claim)~~, UI Toolkit, does not support SerializedProperty binding for **custom classes**.

---

This package helps the Unity binding system recognize custom classes by patching Unity's internal code at runtime using the [HarmonyX](https://github.com/BepInEx/HarmonyX) library.

Naturally, you can completely customize how types will be bound and whether specific types can be assigned as null.\
The reason for allowing specific types to be recognized as nullable is to ensure that libraries like Serializable Nullable, which enable serializing null values, can also function correctly.

You can install it with Git!\
URL : ``https://github.com/Rumi727/UI-Toolkit-Binding-Support.git?path=Packages/com.rumi.custombinding``

## Notes

Please remember that this package injects binder code into Unity's runtime using the [HarmonyX](https://github.com/BepInEx/HarmonyX) library!\
In simple terms, **it's modding Unity!**\
Please note that it might be unstable!

If you find any bugs, please report them in the issues!

## Supported Versions

It works with Unity 2021.3 and later, but it's not recommended due to a few minor bugs.\
Please use it only if absolutely necessary.

Officially, it supports Unity 6 and later!

## Libraries Used

* [HarmonyX](https://github.com/BepInEx/HarmonyX)
  * Many thanks to the developers of Harmony and HarmonyX! This library wouldn't exist without Harmony.
  * Dependencies
    * [Microsoft.Bcl.AsyncInterfaces](https://www.nuget.org/packages/microsoft.bcl.asyncinterfaces)
    * [Mono.Cecil](https://www.nuget.org/packages/Mono.cecil)
    * [MonoMod.Backports](https://www.nuget.org/packages/MonoMod.Backports)
    * [MonoMod.Core](https://www.nuget.org/packages/MonoMod.Core)
    * [MonoMod.ILHelpers](https://www.nuget.org/packages/MonoMod.ILHelpers)
    * [MonoMod.RuntimeDetour](https://www.nuget.org/packages/MonoMod.RuntimeDetour)
    * [MonoMod.Utils](https://www.nuget.org/packages/MonoMod.Utils)
    * [System.IO.Pipelines](https://www.nuget.org/packages/System.IO.Pipelines)
    * [System.Runtime.CompilerServices.Unsafe](https://www.nuget.org/packages/System.Runtime.CompilerServices.Unsafe)
    * [System.Text.Encodings.Web](https://www.nuget.org/packages?q=System.Text.Encodings.Web)
    * [System.Text.Json](https://www.nuget.org/packages/System.Text.Json)

## Preview
<img width="684" height="255" alt="image" src="https://github.com/user-attachments/assets/56b8a196-6936-40c7-8b1d-efe69ed22c97" />

## Usage

[SerializedProperty.boxedValue]: https://docs.unity3d.com/2022.1/Documentation/ScriptReference/SerializedProperty-boxedValue.html

By default, the [ObjectPropertyBinder](Packages/com.rumi.custombinding/Editor/Bindings/ObjectPropertyBinder.cs) is included,\
so simply installing the package will enable proper binding to custom controls (e.g., `MyStructField` inheriting from `BaseField<MyStruct>`).

This means that you can simply do ``field.bindingPath = "myStruct";`` just like you would with built-in controls like `Vector3Field` or `ObjectField`, and everything will work automatically!

However, there might be situations where the [SerializedProperty.boxedValue] property doesn't work (this is a very rare case; I've experienced it due to **conflicts**...)\
or where you need to custom-bind, such as with Serializable Nullable.\
(Oh, and I just learned while making this that [SerializedProperty.boxedValue] has only been around since 2022.1; how did people get values before that?)

Let's say you have a `MyStruct` struct like this:

```csharp
#nullable enable
using System;

[Serializable]
public struct MyStruct
{
    public string? name;
    public float value;
}
```

Then, you can create a binder as shown below.\
It's similar to [PropertyDrawer](https://docs.unity3d.com/kr/2021.3/Manual/editor-PropertyDrawers.html), right?

```csharp
#nullable enable
using Rumi.CustomBinding.Editor;
using System;
using UnityEditor;
using UnityEngine.UIElements;

[CustomPropertyBinder(typeof(MyStruct))] // The CustomPropertyBinder attribute is required for the binder to be recognized!
public class MyStructPropertyBinder : PropertyBinder // And it must also inherit from PropertyBinder!
{
    // We need to read the value from SerializedProperty and deserialize it into MyStruct.
    public override object Read(VisualElement element, SerializedProperty property, Type propertyType)
    {
        property.Next(true); // name
        string name = property.stringValue;
        
        property.Next(false); // value
        float value = property.floatValue;

        return new MyStruct { name = name, value = value };
    }
    
    // We need to read the value from MyStruct and serialize it to SerializedProperty.
    public override void Write(VisualElement element, SerializedProperty property, Type propertyType, object? value)
    {
        if (value is MyStruct myStruct)
        {
            property.Next(true); // name
            property.stringValue = myStruct.name;
            
            property.Next(false); // value
            property.floatValue = myStruct.value;
        }
    }
}
```

Wait! **There's something important!**\
By default, unlike PropertyDrawer, a binder will only work if the defined type (in this case, `MyStruct`) and the type of the property to be bound are **exactly the same type**!

However, the `CustomPropertyBinder` attribute has one more hidden parameter: `isSubtypeCompatible`!

If you set the `isSubtypeCompatible` parameter to <see langword="true"/>, like ``[CustomPropertyBinder(typeof(MyStruct), true)]``,\
then that binder will be recognized as a **binder that can handle subtypes** of `MyStruct`! ~~(How can a struct have subtypes?)~~\
In this case, it behaves exactly like a PropertyDrawer! (It correctly recognizes interfaces, generic type definitions (e.g., `List<>`), etc., as valid binders.)

However! Because of this, you need to be more careful when writing binders that are compatible with subtypes!\
While it might sound obvious, the return value of the binder being <see cref="System.Object"/> doesn't mean you can return any object\
it must always be a type that can be assigned to the **type of the property being bound**! (Here, `MyStruct` or a type inheriting (??) from `MyStruct`).

Below is an example of a binder that is compatible with subtypes!

```csharp
#nullable enable
using System;

[Serializable]
public class MyClass
{
    public string name = string.Empty;
    public float value;
}
```

```csharp
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
```

Oh! To be a bindable type, like `MyStruct` or `MyParent`, it must have a public parameterless constructor, so please keep that in mind!

## I want to register Nullable types!

First, you need to register your desired type as Nullable by assigning a delegate to `NullableTypeRegister.isNullable`!\
For example, ``NullableTypeRegister.isNullable += x => Nullable.GetUnderlyingType(x) != null;``\
will treat the corresponding type as Nullable if the return value is true!

And, to be able to determine the underlying type of a Nullable, you must also register `NullableTypeRegister.getNullableUnderlyingType`!\
For example, ``NullableTypeRegister.getNullableUnderlyingType += static x => Nullable.GetUnderlyingType(x);``\
will return the underlying type of the given type if it exists, and **must return null if it's not a Nullable type** and no underlying type can be found!

Also, there's a point to be aware of!\
Types marked as Nullable must absolutely be implicitly convertible to their underlying type!\
Otherwise, you'll get a casting exception!!
