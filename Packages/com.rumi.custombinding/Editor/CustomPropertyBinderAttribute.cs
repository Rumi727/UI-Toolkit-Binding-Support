#nullable enable
#if !RUNI_ENGINE
using System;

namespace Rumi.CustomBinding.Editor
{
    /// <summary>
    /// Specifies that a class is a custom property binder for a specific type.
    /// This attribute is used to associate a <see cref="PropertyBinder"/> with the type it handles.
    /// <br/>
    /// 클래스가 특정 타입에 대한 커스텀 속성 바인더임을 지정합니다.
    /// 이 속성은 <see cref="PropertyBinder"/>를 해당 바인더가 처리하는 타입과 연결하는 데 사용됩니다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CustomPropertyBinderAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomPropertyBinderAttribute"/> class.
        /// <br/>
        /// <see cref="CustomPropertyBinderAttribute"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="targetType">The <see cref="Type"/> that this binder is intended to handle.<br/>이 바인더가 처리하도록 의도된 <see cref="Type"/>입니다.</param>
        /// <param name="isSubtypeCompatible">
        /// A value indicating whether this binder should also handle subtypes of the <paramref name="targetType"/>.
        /// <br/>If <see langword="true"/>, the binder will apply to <paramref name="targetType"/> and its derived types.
        /// <br/>If <see langword="false"/>, the binder will only apply to the exact <paramref name="targetType"/>.
        /// <br/><br/>
        /// 이 바인더가 <paramref name="targetType"/>의 서브타입도 처리해야 하는지를 나타내는 값입니다.
        /// <br/><see langword="true"/>인 경우, 바인더는 <paramref name="targetType"/> 및 해당 파생 타입에 적용됩니다.
        /// <br/><see langword="false"/>인 경우, 바인더는 정확히 <paramref name="targetType"/>에만 적용됩니다.
        /// </param>
        public CustomPropertyBinderAttribute(Type targetType, bool isSubtypeCompatible = false)
        {
            this.targetType = targetType;
            this.isSubtypeCompatible = isSubtypeCompatible;
        }

        /// <summary>
        /// Gets the target <see cref="Type"/> that this property binder is designed to handle.
        /// <br/>이 바인더가 처리하도록 설계된 대상 <see cref="Type"/>을 가져옵니다.
        /// </summary>
        public Type targetType { get; }
        
        /// <summary>
        /// Gets a value indicating whether this property binder is compatible with subtypes of the <see cref="targetType"/>.
        /// <br/>이 바인더가 <see cref="targetType"/>의 서브타입과 호환되는지 여부를 나타내는 값을 가져옵니다.
        /// </summary>
        public bool isSubtypeCompatible { get; }
    }
}
#endif