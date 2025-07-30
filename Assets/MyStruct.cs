#nullable enable
using System;

[Serializable]
public struct MyStruct : IEquatable<MyStruct>
{
    public string name;
    public float value;
    public bool Equals(MyStruct other) => name == other.name && value.Equals(other.value);
    public override bool Equals(object? obj) => obj is MyStruct other && Equals(other);
    public override int GetHashCode()
    {
        unchecked
        {
            return (name.GetHashCode() * 397) ^ value.GetHashCode();
        }
    }
    public static bool operator ==(MyStruct left, MyStruct right) => left.Equals(right);
    public static bool operator !=(MyStruct left, MyStruct right) => !left.Equals(right);
}
