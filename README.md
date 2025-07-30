# UI Toolkit Binding Support

Languages available in README: \[[**한국어 (대한민국)**](README.md)\] \[[English (US)](README-EN.md)\]

## 소개

유니티의 차세대 UI 시스템 ~~(이라고 주장하는)~~ UI Toolkit에서 **커스텀 클래스**의 SerializedProperty 바인딩을 지원하지 않는다는 것을 깨닫고\
정말 큰 충격을 받아서 만든 패키지입니다.

---

이 패키지는 유니티를 [HarmonyX](https://github.com/BepInEx/HarmonyX) 라이브러리를 사용해서 런타임에 유니티 내부 코드를 패치하여\
유니티 바인딩 시스템이 커스텀 클래스를 인식할 수 있도록 도와줍니다.

당연히? 어떻게 바인딩 될 것인지, 원하는 타입을 null로 할당할 수 있게끔 설정하는것 또한 전부 커스텀 가능합니다.\
특정 타입이 null로 할당 가능으로 인식하게끔 설정하는 기능이 있는 이유가, Serializable Nullable 같이, null을 직렬화할 수 있게 한 라이브러리 또한 정상적으로 작동할 수 있게 하기 위함입니다.

## 참고 사항

[HarmonyX](https://github.com/BepInEx/HarmonyX) 라이브러리로 런타임에서 유니티 코드에 바인더 코드를 집어넣는다는 것을 기억해주세요!\
쉽게 말하면, 유니티를 모딩한겁니다!\
불안정적일 수 있으니, 참고해주세요!

버그가 있다면 꼭! 이슈에 올려주세요!

## 지원되는 버전

Unity 2021.3 버전 부터 작동하지만, 마이너한 버그 소수 존재하여 추천하진 않습니다.\
꼭 필요하다 싶을때만 사용해주세요.\

공식적으론 Unity 6 버전 부터 지원합니다!

## 사용한 라이브러리

* [HarmonyX](https://github.com/BepInEx/HarmonyX)
  * Harmony, HarmonyX를 개발하신 분들에게 정말 감사합니다! Harmony가 없었다면 이 라이브러리는 나오지 못했을거예요.
  * 종속성
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

## 미리보기
<img width="684" height="255" alt="image" src="https://github.com/user-attachments/assets/56b8a196-6936-40c7-8b1d-efe69ed22c97" />

## 사용 방법

[SerializedProperty.boxedValue]: https://docs.unity3d.com/2022.1/Documentation/ScriptReference/SerializedProperty-boxedValue.html

기본적으로, [ObjectPropertyBinder](Packages/com.rumi.custombinding/Editor/Bindings/ObjectPropertyBinder.cs) 바인더가 포함되어있기에 그냥 패키지 설치만 하시면\
커스텀 컨트롤에 (예: BaseField<MyStruct> 클래스를 상속한 MyStructField 등) 바인딩이 정상적으로 될 것입니다.

그러니까 ``field.bindingPath = "myStruct";`` 이런식으로 평소처럼 Vector3Field, ObjectField 같은 내장 컨트롤에 바인딩하듯이 하면 모든게 알아서 될거예요!

하지만, [SerializedProperty.boxedValue] 속성이 동작하지 않는다거나 (매우 특이한 케이스이긴 합니다. 저는 이걸 **충돌로** 경험해봤어요...),\
Serializable Nullable 같이 바인더를 커스텀해야할 상황도 있으실거예요.\
(아, 게다가 이거 만들면서 처음 알았는데 [SerializedProperty.boxedValue]는 2022.1부터 있더라고요?, 그 전엔 어떻게 값 얻으라는거였지...)

자, 이런 MyStruct 구조체가 하나 있다고 해볼게요.

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

그러면 아래와 같이 바인더를 짜면 됩니다.
[PropertyDrawer](https://docs.unity3d.com/kr/2021.3/Manual/editor-PropertyDrawers.html) 이거랑 비슷하죠?

```csharp
#nullable enable
using Rumi.CustomBinding.Editor;
using System;
using UnityEditor;
using UnityEngine.UIElements;

[CustomPropertyBinder(typeof(MyStruct))] // CustomPropertyBinder 어트리뷰트가 있어야 바인더로 인식합니다!
public class MyStructPropertyBinder : PropertyBinder // 물론 PropertyBinder 이것도 같이 상속해야해요!
{
    // SerializedProperty에서 값을 읽어서 MyStruct로 역직렬화 해야합니다.
    public override object Read(VisualElement element, SerializedProperty property, Type propertyType)
    {
        property.Next(true); // name
        string name = property.stringValue;
        
        property.Next(false); // value
        float value = property.floatValue;

        return new MyStruct { name = name, value = value };
    }
    
    // MyStruct에서 값을 읽어서 SerializedProperty에 직렬화 해야합니다.
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

잠깐! **중요한게 있어요!**\
기본적으로 PropertyDrawer랑은 다르게 정한 타입과 (여기선 MyStruct) 바인딩할 프로퍼티의 타입이 **서로 동일한 타입만** 바인더가 동작합니다!

하지만, CustomPropertyBinder 어트리뷰트는 숨겨진 매개변수가 하나 더 있는데,\
바로 ``isSubtypeCompatible`` 입니다!

``[CustomPropertyBinder(typeof(MyStruct), true)]`` 이런식으로 isSubtypeCompatible 매개변수를 true로 설정해주면,\
그 바인더는 MyStruct의 **하위 타입도 처리할 수 있는 바인더**로 인식해요! ~~(구조체가 어떻게 하위 타입이 있는거지?)~~\
이때는, PropertyDrawer와 완전 동일하게 동작합니다! (인터페이스, 제네릭 타입 정의 (예: List<>) 등도 올바른 바인더로 인식함)

하지만! 그렇기에 하위 타입과 호환되는 바인더를 작성할 때는 더 조심하셔야해요!\
당연한 소리이긴 하지만, 바인더의 반환 값이 object라고 아무 오브젝트나 반환해도 되는 것이 아닌,\
항상 **바인딩 할 프로퍼티의 타입**에 할당 가능한 타입이여야합니다! (여기선 MyStruct 또는 MyStruct를 상속(??)하고 있는 타입)

아래는 하위 타입과 호환되는 바인더의 예시예요!

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

아! 바인딩 가능한 타입이 되려면 MyStruct, MyParent 처럼 꼭 매개변수 없는 public 생성자가 있어야하니까 참고해주세요!

## Nullable 타입을 등록하고 싶어요!

우선, NullableTypeRegister.isNullable 딜리게이트에 원하는 타입이 Nullable이라고 인식할 수 있게 등록해주셔야해요!\
``NullableTypeRegister.isNullable += x => Nullable.GetUnderlyingType(x) != null;``\
이런식으로 반환값이 true이면 해당하는 타입은 Nullable 처럼 취급해요!

그리고, Nullable의 기본 타입을 알 수 있게 NullableTypeRegister.getNullableUnderlyingType 또한 등록해주셔야해요!\
``NullableTypeRegister.getNullableUnderlyingType += static x => Nullable.GetUnderlyingType(x);``\
이런식으로 매개변수로 들어온 타입의 기본 타입이 있으면 그 타입을 반환해주시면 되고, Nullable 타입이 아니라서 **기본 타입을 찾을 수 없다면 꼭 null을 반환**해주세요!

또한 주의하실점이 있어요!\
Nullable로 표시된 타입은 꼭꼭 기본 타입으로 암시적 형변환할 수 있어야합니다\
안그러면 캐스팅 예외나요!!

## 유용한 유틸리티들!

* ``public static T PropertyFieldUtility.SetPropertyPath(this T element, SerializedProperty property)``
  * ```
    element.bindingPath = property.propertyPath;
    return element;
    ```
    요거 함수로 만든거
* ``public static BaseField<T> PropertyFieldUtility.ConfigureFieldStyles<T>(this BaseField<T> field)`` 라벨 정렬과 프로퍼티 필드로 표시하는 USS를 추가하고 자기 자신을 반환
