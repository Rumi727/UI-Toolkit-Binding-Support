#nullable enable
using System;
using UnityEditor;

namespace Rumi.CustomBinding.Editor
{
    /// <summary>
    /// Handles the registration and checking of nullable types within the editor.
    /// This class is initialized when the Unity editor loads.
    /// <br/>
    /// 에디터 내에서 nullable 타입의 등록 및 확인을 처리합니다.
    /// 이 클래스는 유니티 에디터 로드 시 초기화됩니다.
    /// </summary>
    [InitializeOnLoad]
    public static class NullableTypeRegister
    {
        /// <summary>
        /// A delegate that determines if a given <see cref="Type"/> is nullable.
        /// <br/>This is used by the <see cref="Rumi.CustomBinding.Editor.UIElements.Bindings.PropertyBinder.Read"/> method to check if the type of the value being read can accept <see langword="null"/>.
        /// <br/><br/>
        /// 주어진 <see cref="Type"/>이 nullable인지 여부를 결정하는 델리게이트입니다.
        /// <br/>이것은 <see cref="Rumi.CustomBinding.Editor.UIElements.Bindings.PropertyBinder.Read"/> 메소드에서 읽히는 값의 타입이 <see langword="null"/>을 허용할 수 있는지 확인하는 용도로 사용됩니다.
        /// </summary>
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public static Func<Type, bool>? isNullable { get; }

        /// <summary>
        /// Initializes the <see cref="NullableTypeRegister"/> class.
        /// <br/>The <see cref="isNullable"/> delegate is populated to check for class types, interface types, and <see cref="Nullable{T}"/> types.
        /// <br/><br/>
        /// <see cref="NullableTypeRegister"/> 클래스를 초기화합니다.
        /// <br/><see cref="isNullable"/> 델리게이트는 클래스 타입, 인터페이스 타입 및 <see cref="Nullable{T}"/> 타입을 확인하도록 채워집니다.
        /// </summary>
        static NullableTypeRegister() => isNullable += static x => x.IsClass || x.IsInterface || Nullable.GetUnderlyingType(x) != null;
    }
}
