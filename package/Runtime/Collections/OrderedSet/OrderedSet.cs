using System;
using System.Collections.Generic;

namespace AlchemyBow.Navigation.Collections
{
    /// <summary>
    /// Represents a collection of objects that is maintained in sorted order and allows duplicates.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    /// <remarks>
    /// <para>This collection is a hybrid of an AVL tree and a linked list.</para>
    /// <para>The AVL tree part was based on this cool article - https://www.geeksforgeeks.org/avl-tree-set-1-insertion/ .</para>
    /// </remarks>
    public sealed class OrderedSet<T> where T : class, IComparable<T>
    {
        private OrderedSetNode<T> root;

        /// <summary>
        /// Determines whether the set contains any value.
        /// </summary>
        /// <returns><c>true</c> if the set contains any item; otherwise, <c>false</c>.</returns>
        public bool Any => root != null;

        /// <summary>
        /// Gets the first minimum value in the set.
        /// </summary>
        /// <returns>The first minimum value in the set.</returns>
        public T Min
        {
            get => root == null ? null : GetMin(root).value;
        }
        private OrderedSetNode<T> GetMin(OrderedSetNode<T> root)
        {
            return root.left == null ? root : GetMin(root.left);
        }

        /// <summary>
        /// Determines whether the set contains a specific item.
        /// </summary>
        /// <param name="value">The element to locate in the set.</param>
        /// <returns><c>true</c> if the set contains item; otherwise, <c>false</c>.</returns>
        public bool Contains(T value)
        {
            return Contains(root, value);
        }
        private bool Contains(OrderedSetNode<T> root, T value)
        {
            if (root == null)
            {
                return false;
            }

            int compareResult = value.CompareTo(root.value);
            if (compareResult < 0)
            {
                return Contains(root.left, value);
            }
            else if (compareResult > 0)
            {
                return Contains(root.right, value);
            }
            else
            {
                return NextContains(root, value);
            }
        }
        private bool NextContains(OrderedSetNode<T> node, T value)
        {
            if (node == null)
            {
                return false;
            }

            return node == value || NextContains(node.next, value);
        }

        /// <summary>
        /// Adds an item to the set.
        /// </summary>
        /// <param name="value">The element to add.</param>
        public void Add(T value)
        {
            root = Insert(root, value);
        }

        /// <summary>
        /// Removes a specified item from the set.
        /// </summary>
        /// <param name="value">The element to remove.</param>
        public void Remove(T value)
        {
            root = DeleteNode(root, value);
        }


        /// <summary>
        /// Searches for an item that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire set.
        /// </summary>
        /// <param name="predicate">The predicate that defines the conditions of the element to search for.</param>
        /// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, <c>null</c>.</returns>
        public T BruteFind(Predicate<T> predicate)
        {
            return BruteFind(root, predicate);
        }
        private T BruteFind(OrderedSetNode<T> root, Predicate<T> predicate)
        {
            if (root == null)
            {
                return null;
            }
            else
            {
                var result = BruteNextFind(root, predicate);
                if (result == null) result = BruteFind(root.left, predicate);
                if (result == null) result = BruteFind(root.right, predicate);
                return result;
            }
        }
        private T BruteNextFind(OrderedSetNode<T> node, Predicate<T> predicate)
        {
            if (node == null)
            {
                return null;
            }

            return predicate(node.value) ? node.value : BruteNextFind(node.next, predicate);
        }

        /// <summary>
        /// Uses the comparer on the set.
        /// </summary>
        /// <param name="comparer">The comparer to use.</param>
        public void UseRangeComparer(OrderedSetRangeComparer<T> comparer)
        {
            comparer.Calculate(root);
        }

        private OrderedSetNode<T> Insert(OrderedSetNode<T> node, T value)
        {
            if (node == null)
            {
                return new OrderedSetNode<T>(value);
            }

            int compareResult = value.CompareTo(node.value);
            if (compareResult < 0)
            {
                node.left = Insert(node.left, value);
            }
            else if (compareResult > 0)
            {
                node.right = Insert(node.right, value);
            }
            else
            {
                InsertNext(node, value);
                // Returns immediately because inserting as next does not affect the balance of the tree.
                return node;
            }

            node.height = 1 + DetermineMaxHeight(node.left, node.right);
            int balance = GetBalance(node);

            //Left Left Case
            if (balance > 1 && value.CompareTo(node.left.value) < 0)
            {
                return RotateRight(node);
            }

            // Right Right Case  
            if (balance < -1 && value.CompareTo(node.right.value) > 0)
            {
                return RotateLeft(node);
            }

            // Left Right Case  
            if (balance > 1 && value.CompareTo(node.left.value) > 0)
            {
                node.left = RotateLeft(node.left);
                return RotateRight(node);
            }

            // Right Left Case  
            if (balance < -1 && value.CompareTo(node.right.value) < 0)
            {
                node.right = RotateRight(node.right);
                return RotateLeft(node);
            }

            // The tree is balanced.
            return node;
        }
        private void InsertNext(OrderedSetNode<T> node, T value)
        {
            if (node.next == null)
            {
                node.next = new OrderedSetNode<T>(value);
            }
            else
            {
                InsertNext(node.next, value);
            }
        }

        private OrderedSetNode<T> DeleteNode(OrderedSetNode<T> root, T value)
        {
            if (root == null)
                return root;

            int compareResult = value.CompareTo(root.value);
            if (compareResult < 0)
            {
                root.left = DeleteNode(root.left, value);
            }
            else if (compareResult > 0)
            {
                root.right = DeleteNode(root.right, value);
            }
            else
            {
                if (root.next != null)
                {
                    if (root.value == value)
                    {
                        root.value = root.next.value;
                        root.next = root.next.next;
                    }
                    else
                    {
                        DeleteNext(root, value);
                    }
                    return root;
                }

                if ((root.left == null) || (root.right == null))
                {
                    root = root.left ?? root.right;
                }
                else
                {
                    var temp = DetermineMinValueNode(root.right);
                    root.value = temp.value;
                    root.next = temp.next;
                    temp.next = null;
                    root.right = DeleteNode(root.right, temp.value);
                }
            }

            if (root == null)
            {
                return root;
            }

            root.height = DetermineMaxHeight(root.left, root.right) + 1;
            int balance = GetBalance(root);

            // Left Left Case  
            if (balance > 1 && GetBalance(root.left) >= 0)
            {
                return RotateRight(root);
            }

            // Left Right Case  
            if (balance > 1 && GetBalance(root.left) < 0)
            {
                root.left = RotateLeft(root.left);
                return RotateRight(root);
            }

            // Right Right Case  
            if (balance < -1 && GetBalance(root.right) <= 0)
            {
                return RotateLeft(root);
            }

            // Right Left Case  
            if (balance < -1 && GetBalance(root.right) > 0)
            {
                root.right = RotateRight(root.right);
                return RotateLeft(root);
            }

            // The tree is balanced.
            return root;
        }
        private void DeleteNext(OrderedSetNode<T> node, T value)
        {
            if (node.next.value == value)
            {
                node.next = node.next.next;
            }
            else
            {
                DeleteNext(node.next, value);
            }
        }

        /// <summary>
        /// Copies the elements of the set to a new array.
        /// </summary>
        /// <returns>An array containing copies of the elements of the set.</returns>
        public T[] ToArray()
        {
            var nodes = new List<T>();
            InOrder(root, nodes);
            return nodes.ToArray();
        }
        private void InOrder(OrderedSetNode<T> current, List<T> nodes)
        {
            if (current != null)
            {
                InOrder(current.left, nodes);
                nodes.Add(current.value);
                AddNext(current, nodes);
                InOrder(current.right, nodes);
            }
        }
        private void AddNext(OrderedSetNode<T> current, List<T> nodes)
        {
            if (current.next != null)
            {
                nodes.Add(current.next.value);
                AddNext(current.next, nodes);
            }
        }

        #region Common BST
        private static int DetermineHeight(OrderedSetNode<T> node)
        {
            return node == null ? 0 : node.height;
        }

        private static int DetermineMaxHeight(OrderedSetNode<T> a, OrderedSetNode<T> b)
        {
            int aHeight = DetermineHeight(a);
            int bHeight = DetermineHeight(b);
            return (aHeight > bHeight) ? aHeight : bHeight;
        }

        private static OrderedSetNode<T> DetermineMinValueNode(OrderedSetNode<T> node)
        {
            var current = node;
            while (current.left != null)
            {
                current = current.left;
            }
            return current;
        }

        private static OrderedSetNode<T> RotateRight(OrderedSetNode<T> y)
        {
            var x = y.left;
            var T2 = x.right;

            x.right = y;
            y.left = T2;

            y.height = DetermineMaxHeight(y.left, y.right) + 1;
            x.height = DetermineMaxHeight(x.left, x.right) + 1;

            return x;
        }

        private static OrderedSetNode<T> RotateLeft(OrderedSetNode<T> x)
        {
            var y = x.right;
            var T2 = y.left;

            y.left = x;
            x.right = T2;

            x.height = DetermineMaxHeight(x.left, x.right) + 1;
            y.height = DetermineMaxHeight(y.left, y.right) + 1;

            return y;
        }

        private static int GetBalance(OrderedSetNode<T> node)
        {
            if (node == null)
                return 0;

            return DetermineHeight(node.left) - DetermineHeight(node.right);
        }
        #endregion
    }
}
