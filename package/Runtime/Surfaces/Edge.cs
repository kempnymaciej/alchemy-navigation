namespace AlchemyBow.Navigation.Surfaces
{
    /// <summary>
    /// Describes two connected vertices.
    /// </summary>
    public class Edge : UsageTrackingObject<Face>
    {
        private const int InitialUsersCapacity = 2;

        /// <summary>
        /// The vertex A.
        /// </summary>
        /// <returns>The vertex A.</returns>
        public readonly Vertex a;
        /// <summary>
        /// The vertex B.
        /// </summary>
        /// <returns>The vertex B.</returns>
        public readonly Vertex b;

        /// <summary>
        /// Creates an instance of the Edge class.
        /// </summary>
        /// <param name="a">The vertex A.</param>
        /// <param name="b">The vertex B.</param>
        public Edge(Vertex a, Vertex b) : base(InitialUsersCapacity)
        {
            this.a = a;
            this.b = b;
        }

        /// <summary>
        /// Gets the other user of the edge.
        /// </summary>
        /// <param name="user">The first user.</param>
        /// <returns>The other user or <c>null</c>.</returns>
        /// <remarks>In most cases, the edge should have a maximum of two users.</remarks>
        public Face GetOtherUser(Face user)
        {
            for (int i = 0; i < UsersCount; i++)
            {
                if(GetUser(i) != user)
                {
                    return GetUser(i);
                }
            }

            return null;
        }
    } 
}
