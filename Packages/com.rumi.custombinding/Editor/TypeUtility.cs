#nullable enable
#if !RUNI_ENGINE
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Rumi.CustomBinding.Editor
{
    static class TypeUtility
    {
        public static object? GetDefaultValue(this Type type)
        {
            if (!type.IsValueType)
                return null;

            return Activator.CreateInstance(type);
        }

        public static object GetDefaultValueNotNull(this Type type)
        {
            var delegates = NullableType.getNullableUnderlyingType.GetInvocationList().OfType<Func<Type, Type?>>();
            foreach (var getNullableType in delegates)
            {
                Type? nullableType = getNullableType.Invoke(type);
                if (nullableType != null)
                    return nullableType.GetDefaultValueNotNull();
            }

            if (type == typeof(string))
                return string.Empty;

            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// 주어진 <paramref name="givenType"/>이 특정 제네릭 타입 정의(<paramref name="genericTypeDefinition"/>)를
        /// 구현하거나 상속하는지 확인합니다.
        /// </summary>
        /// <remarks>
        /// 이 메서드는 <paramref name="givenType"/>의 인터페이스 및 상속 계층 구조를 탐색하여
        /// <paramref name="genericTypeDefinition"/>과 일치하는 제네릭 타입 정의가 있는지 검사합니다.<br/>
        /// 예를 들어, <c>List&lt;int&gt;</c>가 <c>IEnumerable&lt;&gt;</c>를 구현하는지,
        /// 또는 <c>MyDerivedClass&lt;T&gt;</c>가 <c>BaseClass&lt;&gt;</c>로부터 파생되었는지 등을 확인할 수 있습니다.
        /// </remarks>
        /// <param name="givenType">확인할 대상 <see cref="Type"/>입니다.</param>
        /// <param name="genericTypeDefinition">찾으려는 제네릭 타입 정의입니다 (예: <c>typeof(List&lt;&gt;)</c>, <c>typeof(IDictionary&lt;,&gt;)</c>).</param>
        /// <returns>
        /// <paramref name="givenType"/>이 <paramref name="genericTypeDefinition"/>을 구현하거나 상속하면
        /// <see langword="true"/>를 반환하고, 그렇지 않으면 <see langword="false"/>를 반환합니다.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="givenType"/> 또는 <paramref name="genericTypeDefinition"/>이 <see langword="null"/>인 경우 발생합니다.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="genericTypeDefinition"/>이 유효한 제네릭 타입 정의가 아닌 경우 발생할 수 있습니다.
        /// </exception>
        public static bool IsAssignableToGenericDefinition(this Type givenType, Type genericTypeDefinition) => IsAssignableToGenericDefinition(givenType, genericTypeDefinition, out _);

        /// <summary>
        /// 주어진 <paramref name="givenType"/>이 특정 제네릭 타입 정의(<paramref name="genericTypeDefinition"/>)를
        /// 구현하거나 상속하는지 확인합니다.
        /// </summary>
        /// <remarks>
        /// 이 메서드는 <paramref name="givenType"/>의 인터페이스 및 상속 계층 구조를 탐색하여
        /// <paramref name="genericTypeDefinition"/>과 일치하는 제네릭 타입 정의가 있는지 검사합니다.<br/>
        /// 예를 들어, <c>List&lt;int&gt;</c>가 <c>IEnumerable&lt;&gt;</c>를 구현하는지,
        /// 또는 <c>MyDerivedClass&lt;T&gt;</c>가 <c>BaseClass&lt;&gt;</c>로부터 파생되었는지 등을 확인할 수 있습니다.
        /// </remarks>
        /// <param name="givenType">확인할 대상 <see cref="Type"/>입니다.</param>
        /// <param name="genericTypeDefinition">찾으려는 제네릭 타입 정의입니다 (예: <c>typeof(List&lt;&gt;)</c>, <c>typeof(IDictionary&lt;,&gt;)</c>).</param>
        /// <param name="resolvedType">
        /// <paramref name="givenType"/>이 <paramref name="genericTypeDefinition"/>을 구현하거나 상속하는 경우,
        /// 실제로 발견된 구체적인 제네릭 타입(예: <c>List&lt;int&gt;</c>)이 반환됩니다.
        /// 찾지 못한 경우 <see langword="null"/>이 반환됩니다.
        /// </param>
        /// <returns>
        /// <paramref name="givenType"/>이 <paramref name="genericTypeDefinition"/>을 구현하거나 상속하면
        /// <see langword="true"/>를 반환하고, 그렇지 않으면 <see langword="false"/>를 반환합니다.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="givenType"/> 또는 <paramref name="genericTypeDefinition"/>이 <see langword="null"/>인 경우 발생합니다.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="genericTypeDefinition"/>이 유효한 제네릭 타입 정의가 아닌 경우 발생할 수 있습니다.
        /// </exception>
        public static bool IsAssignableToGenericDefinition(this Type givenType, Type genericTypeDefinition, [MaybeNullWhen(false)] out Type resolvedType)
        {
            // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (givenType == null)
                throw new ArgumentNullException(nameof(givenType), "The given type cannot be null.");
            if (genericTypeDefinition == null)
                throw new ArgumentNullException(nameof(genericTypeDefinition), "The generic type definition cannot be null.");
            else if (!genericTypeDefinition.IsGenericTypeDefinition)
                throw new ArgumentException("The provided genericTypeDefinition must be a valid generic type definition (e.g., typeof(List<>) or typeof(IDictionary<,>)).", nameof(genericTypeDefinition));
            // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

            Type? currentType = givenType;
            while (currentType != null)
            {
                // 인터페이스 확인
                var interfaceTypes = currentType.GetInterfaces();
                foreach (var it in interfaceTypes)
                {
                    if (it.IsGenericType && it.GetGenericTypeDefinition() == genericTypeDefinition)
                    {
                        resolvedType = it;
                        return true;
                    }
                }

                // 현재 타입 확인 (직접적인 제네릭 타입 정의 일치)
                if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == genericTypeDefinition)
                {
                    resolvedType = currentType;
                    return true;
                }

                // 기반 클래스 확인
                currentType = currentType.BaseType;
            }

            resolvedType = null;
            return false;
        }

        /// <summary>
        /// 주어진 <paramref name="givenType"/>의 인스턴스가 <paramref name="targetType"/>의 변수에 할당 가능한지 확인합니다.
        /// <paramref name="targetType"/>이 제네릭 타입 정의(<c>List&lt;&gt;</c> 등)인 경우,
        /// <paramref name="givenType"/>이 해당 제네릭 정의를 구현하거나 상속하는지도 함께 검사합니다.
        /// </summary>
        /// <remarks>
        /// 이 메서드는 다음과 같은 경우 <see langword="true"/>를 반환합니다:<br/>
        /// <list type="bullet">
        ///     <item><description><paramref name="givenType"/>이 <paramref name="targetType"/>과 동일한 경우.</description></item>
        ///     <item><description><paramref name="givenType"/>이 <paramref name="targetType"/>의 서브클래스인 경우.</description></item>
        ///     <item><description><paramref name="givenType"/>이 <paramref name="targetType"/> 인터페이스를 구현하는 경우.</description></item>
        ///     <item><description><paramref name="targetType"/>이 제네릭 타입 정의이고, <paramref name="givenType"/>이 해당 정의를 구현하거나 상속하는 경우.</description></item>
        /// </list>
        /// 이 메서드는 <paramref name="targetType"/>이 제네릭 타입 정의인 경우
        /// <paramref name="givenType"/>의 인터페이스 및 상속 계층 구조를 탐색하여 일치 여부를 검사합니다.
        /// </remarks>
        /// <param name="givenType">할당될 대상 인스턴스의 <see cref="Type"/>입니다.</param>
        /// <param name="targetType">할당받을 변수의 <see cref="Type"/>입니다. 제네릭 타입 정의일 수 있습니다.</param>
        /// <returns>
        /// <paramref name="givenType"/>이 <paramref name="targetType"/>에 할당 가능하면 <see langword="true"/>를 반환하고,
        /// 그렇지 않으면 <see langword="false"/>를 반환합니다.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="givenType"/> 또는 <paramref name="targetType"/>이 <see langword="null"/>인 경우 발생합니다.
        /// </exception>
        public static bool IsAssignableToAny(this Type givenType, Type targetType) => IsAssignableToAny(givenType, targetType, out _);

        /// <summary>
        /// 주어진 <paramref name="givenType"/>의 인스턴스가 <paramref name="targetType"/>의 변수에 할당 가능한지 확인합니다.
        /// <paramref name="targetType"/>이 제네릭 타입 정의(<c>List&lt;&gt;</c> 등)인 경우,
        /// <paramref name="givenType"/>이 해당 제네릭 정의를 구현하거나 상속하는지도 함께 검사합니다.
        /// </summary>
        /// <remarks>
        /// 이 메서드는 다음과 같은 경우 <see langword="true"/>를 반환합니다:<br/>
        /// <list type="bullet">
        ///     <item><description><paramref name="givenType"/>이 <paramref name="targetType"/>과 동일한 경우.</description></item>
        ///     <item><description><paramref name="givenType"/>이 <paramref name="targetType"/>의 서브클래스인 경우.</description></item>
        ///     <item><description><paramref name="givenType"/>이 <paramref name="targetType"/> 인터페이스를 구현하는 경우.</description></item>
        ///     <item><description><paramref name="targetType"/>이 제네릭 타입 정의이고, <paramref name="givenType"/>이 해당 정의를 구현하거나 상속하는 경우.</description></item>
        /// </list>
        /// 이 메서드는 <paramref name="targetType"/>이 제네릭 타입 정의인 경우
        /// <paramref name="givenType"/>의 인터페이스 및 상속 계층 구조를 탐색하여 일치 여부를 검사합니다.
        /// </remarks>
        /// <param name="givenType">할당될 대상 인스턴스의 <see cref="Type"/>입니다.</param>
        /// <param name="targetType">할당받을 변수의 <see cref="Type"/>입니다. 제네릭 타입 정의일 수 있습니다.</param>
        /// <param name="resolvedType">
        /// 할당이 가능한 경우, 발견된 구체적인 <see cref="Type"/> (예: <c>List&lt;int&gt;</c>) 또는
        /// <paramref name="targetType"/>이 제네릭 정의가 아닌 경우 <paramref name="targetType"/> 자체가 반환됩니다.
        /// 할당이 불가능한 경우 <see langword="null"/>이 반환됩니다.
        /// </param>
        /// <returns>
        /// <paramref name="givenType"/>이 <paramref name="targetType"/>에 할당 가능하면 <see langword="true"/>를 반환하고,
        /// 그렇지 않으면 <see langword="false"/>를 반환합니다.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="givenType"/> 또는 <paramref name="targetType"/>이 <see langword="null"/>인 경우 발생합니다.
        /// </exception>
        public static bool IsAssignableToAny(this Type givenType, Type targetType, [MaybeNullWhen(false)] out Type resolvedType)
        {
            // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (givenType == null)
                throw new ArgumentNullException(nameof(givenType), "The given type cannot be null.");
            else if (targetType == null)
                throw new ArgumentNullException(nameof(targetType), "The target type cannot be null.");
            // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

            if (targetType.IsGenericTypeDefinition)
                return givenType.IsAssignableToGenericDefinition(targetType, out resolvedType);
            else if (targetType.IsAssignableFrom(givenType))
            {
                resolvedType = targetType;
                return true;
            }

            resolvedType = null;
            return false;
        }

        /// <summary>
        /// 지정된 타입의 상위 계층(상속 체인)을 열거합니다.<br/>
        /// 이 메서드는 현재 타입부터 시작하여 <see cref="object"/> 타입까지 모든 기본 타입을 반환합니다.
        /// </summary>
        /// <param name="type">상위 계층을 가져올 시작 <see cref="Type"/>입니다.</param>
        /// <returns>
        /// 지정된 타입과 그 상위 기본 타입을 포함하는 <see cref="IEnumerable{T}"/> of <see cref="Type"/> 컬렉션입니다.<br/>
        /// 만약 <paramref name="type"/>이 null인 경우 빈 컬렉션을 반환합니다.
        /// </returns>
        public static IEnumerable<Type> GetHierarchy(this Type? type)
        {
            for (; type != null; type = type.BaseType)
                yield return type;
        }
    }
}
#endif