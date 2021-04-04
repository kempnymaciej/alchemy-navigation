namespace AlchemyBow.Navigation.Surfaces
{
    /// <summary>
    /// Describe the usage of an object.
    /// </summary>
    /// <typeparam name="TUser">The type of the user.</typeparam>
    public class Usage<TUser> where TUser : class
    {
        /// <summary>
        /// The user.
        /// </summary>
        /// <returns>The user.</returns>
        public readonly TUser user;
        /// <summary>
        /// The number of uses.
        /// </summary>
        /// <returns>The number of uses.</returns>
        public int uses;

        /// <summary>
        /// Creates an instance of the Usage class.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="uses">The number of uses.</param>
        public Usage(TUser user, int uses)
        {
            this.user = user;
            this.uses = uses;
        }
    } 
}
