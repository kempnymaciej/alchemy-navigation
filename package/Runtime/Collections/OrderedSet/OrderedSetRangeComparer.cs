using System;

namespace AlchemyBow.Navigation.Collections
{
    /// <summary>
    /// Describes a comparer that allows you to enumerate nodes of the <c>OrderedSet</c> within a specified range.
    /// </summary>
    /// <typeparam name="T">The type of the node value.</typeparam>
    public abstract class OrderedSetRangeComparer<T> where T : class, IComparable<T>
    {
        private IComparable<T> min;
        private IComparable<T> max;
        private bool calculated;

        /// <summary>
        /// Creates the minimum boundary.
        /// </summary>
        /// <returns>The minimum boundary.</returns>
        protected abstract IComparable<T> CreateMinBaundary();

        /// <summary>
        /// Creates the maximum boundary.
        /// </summary>
        /// <returns>The maximum boundary.</returns>
        protected abstract IComparable<T> CreateMaxBaundary();

        /// <summary>
        /// Triggers the calculations.
        /// </summary>
        /// <param name="root">The root node of the <c>OrderedSet</c>.</param>
        public void Calculate(OrderedSetNode<T> root)
        {
            if (calculated)
            {
                throw new Exception("RangeComparer cannot be used more then once.");
            }

            min = CreateMinBaundary();
            max = CreateMaxBaundary();

            RecalculateResult(root);
            calculated = true;
        }

        private void RecalculateResult(OrderedSetNode<T> root)
        {
            if (root == null)
            {
                return;
            }

            int aToValue = min.CompareTo(root.value);
            int bToValue = max.CompareTo(root.value);

            if (aToValue < 0)
                RecalculateResult(root.left);

            if (aToValue <= 0 && bToValue >= 0)
            {
                OnMove(root.value);
                HandleNext(root);
            }

            if (bToValue > 0)
                RecalculateResult(root.right);
        }

        private void HandleNext(OrderedSetNode<T> node)
        {
            if (node.next != null)
            {
                OnMove(node.next.value);
                HandleNext(node.next);
            }
        }

        /// <summary>
        /// Called for each node in the range.
        /// </summary>
        /// <param name="value">The node.</param>
        public abstract void OnMove(T value);
    } 
}
