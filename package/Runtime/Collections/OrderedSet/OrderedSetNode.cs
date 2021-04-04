using System;

namespace AlchemyBow.Navigation.Collections
{
    /// <summary>
    /// Represents a node of the <c>OrderedSet</c>.
    /// </summary>
    /// <typeparam name="T">The type of the node value.</typeparam>
    public sealed class OrderedSetNode<T> where T : class, IComparable<T>
    {
        /// <summary>
        /// The height of the node.
        /// </summary>
        /// <returns>The height of the node.</returns>
        public int height;

        /// <summary>
        /// The value of the node.
        /// </summary>
        /// <returns>The value of the node.</returns>
        public T value;

        /// <summary>
        /// The left child of the node (or <c>null</c>).
        /// </summary>
        /// <returns>The left child of the node (or <c>null</c>).</returns>
        public OrderedSetNode<T> left;

        /// <summary>
        /// The left child of the node (or <c>null</c>).
        /// </summary>
        /// <returns>The left child of the node (or <c>null</c>).</returns>
        public OrderedSetNode<T> right;

        /// <summary>
        /// The linked child of the node (or <c>null</c>).
        /// </summary>
        /// <returns>The linked child of the node (or <c>null</c>).</returns>
        public OrderedSetNode<T> next;

        /// <summary>
        /// Creates an instance of the OrderedSetNode class.
        /// </summary>
        /// <param name="value">The value of the node.</param>
        public OrderedSetNode(T value)
        {
            this.value = value;
            this.height = 1;
        }
    } 
}
