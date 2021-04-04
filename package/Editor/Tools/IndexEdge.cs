using System;

namespace AlchemyBow.Navigation.Editor
{
    public sealed class IndexEdge : IEquatable<IndexEdge>
    {
        public readonly int a;
        public readonly int b;

        public IndexEdge(int a, int b)
        {
            this.a = a;
            this.b = b;
        }

        public bool Equals(IndexEdge other)
        {
            return (a == other.a && b == other.b) || (a == other.b && b == other.a);
        }
    } 
}
