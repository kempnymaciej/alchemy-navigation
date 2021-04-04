using System;
using UnityEngine;

namespace AlchemyBow.Navigation.Surfaces
{
    /// <summary>
    /// Describes an object that can track it's users.
    /// </summary>
    /// <typeparam name="TUser">The type of users.</typeparam>
    public abstract class UsageTrackingObject<TUser> where TUser : class
    {
        private int usersCount;
        private Usage<TUser>[] users;

        /// <summary>
        /// The number of the unique users.
        /// </summary>
        /// <returns>The number of the unique users.</returns>
        public int UsersCount => usersCount;

        /// <summary>
        /// Determines whether the object has not users.
        /// </summary>
        /// <returns><c>true</c> if the object has not users; otherwise, <c>false</c>.</returns>
        public bool HasNoUsers => usersCount <= 0;

        /// <summary>
        /// Initializes an instance of the UsageTrackingObject class.
        /// </summary>
        /// <param name="initialUsersCapacity">The initial unique user capacity.</param>
        protected UsageTrackingObject(int initialUsersCapacity)
        {
            this.users = new Usage<TUser>[initialUsersCapacity];
            usersCount = 0;
        }

        /// <summary>
        /// Get the user at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the user.</param>
        /// <returns>The user at the specified index.</returns>
        public TUser GetUser(int index) => users[index].user;

        /// <summary>
        /// Adds the user.
        /// </summary>
        /// <param name="user">A user to add.</param>
        public void AddUser(TUser user)
        {
            if(GetUserIndex(user, out int index))
            {
                users[index].uses++;
            }
            else
            {
                EnsureUsersArraySize(usersCount + 1);
                users[usersCount] = new Usage<TUser>(user, 1);
                usersCount++;
            }
        }

        private bool GetUserIndex(TUser user, out int index)
        {
            for (int i = 0; i < usersCount; i++)
            {
                if(users[i].user == user)
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }

        /// <summary>
        /// Removes the user.
        /// </summary>
        /// <param name="user">A user to remove.</param>
        public void RemoveUser(TUser user)
        {
            if (GetUserIndex(user, out int index))
            {
                users[index].uses--;
                if (users[index].uses == 0)
                {
                    RemoveUserAtIndex(index);
                }
            }
            else
            {
                throw new ArgumentException("Attempt to remove user that does not exist.");
            }
        }

        private void RemoveUserAtIndex(int index)
        {
            usersCount--;
            for (int i = index; i < usersCount; i++)
            {
                users[i] = users[i + 1];
            }
            users[usersCount] = null;
        }

        /// <summary>
        /// Determines whether a user is tracked.
        /// </summary>
        /// <param name="user">The user to locate.</param>
        /// <returns><c>true</c> if the user is found; otherwise, <c>false</c>.</returns>
        public bool HasUser(TUser user)
        {
            for (int i = 0; i < usersCount; i++)
            {
                if(users[i].user == user) 
                { 
                    return true; 
                }
            }
            return false;
        }

        private void EnsureUsersArraySize(int minSize)
        {
            if(users.Length < minSize)
            {
                Array.Resize(ref users, Mathf.NextPowerOfTwo(minSize));
            }
        }
    } 
}
