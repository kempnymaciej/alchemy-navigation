using AlchemyBow.Navigation.Surfaces;
using System;

namespace AlchemyBow.Navigation.Utilities
{
    /// <summary>
    /// A <c>Face</c> wrapper used during <c>FaceAStar</c> calculations.
    /// </summary>
    public sealed class FaceStarUnit : IComparable<FaceStarUnit>
    {
        /// <summary>
        /// The wrapped face.
        /// </summary>
        /// <returns>The wrapped face.</returns>
        public readonly Face face;

        /// <summary>
        /// The A* parent.
        /// </summary>
        /// <returns>The A* parent.</returns>
        public FaceStarUnit parent;

        /// <summary>
        /// The A* g cost.
        /// </summary>
        /// <returns>The A* g cost.</returns>
        public float gCost;

        /// <summary>
        /// The A* h cost.
        /// </summary>
        /// <returns>The A* h cost.</returns>
        public float hCost;

        /// <summary>
        /// The A* f cost.
        /// </summary>
        /// <returns>The A* f cost.</returns>
        public float FCost { get => gCost + hCost; }

        /// <summary>
        /// Creates a new instance of the FaceStarUnit class with default values.
        /// </summary>
        /// <param name="face">A face to wrap.</param>
        public FaceStarUnit(Face face) : this(face, null, 0, 0) { }

        /// <summary>
        /// Creates a new instance of the FaceStarUnit class.
        /// </summary>
        /// <param name="face">A face to wrap.</param>
        /// <param name="parent">An A* parent.</param>
        /// <param name="gCost">An A* g cost.</param>
        /// <param name="hCost">An A* h cost.</param>
        public FaceStarUnit(Face face, FaceStarUnit parent, float gCost, float hCost)
        {
            this.face = face;
            this.parent = parent;
            this.gCost = gCost;
            this.hCost = hCost;
        }


        /// <summary>
        /// Updates values of the unit.
        /// </summary>
        /// <param name="parent">The new A* parent.</param>
        /// <param name="gCost">The new A* gCost.</param>
        /// <param name="hCost">The new A* hCost.</param>
        public void Update(FaceStarUnit parent, float gCost, float hCost)
        {
            this.parent = parent;
            this.gCost = gCost;
            this.hCost = hCost;
        }

        /// <summary>
        /// Compares the current instance with another instance of the <c>FaceStarUnit</c> class based on f cost and (then) g cost of the A* algorithm.
        /// </summary>
        /// <param name="other">The other instance to compare with this instance.</param>
        /// <returns>An integer that indicates whether the current instance precedes, follows, or occurs in the same position in the A* order as the other object.</returns>
        public int CompareTo(FaceStarUnit other)
        {
            int result;
            if(FCost < other.FCost)
            {
                result = -1;
            }
            else if(FCost > other.FCost)
            {
                result = 1;
            }
            else
            {
                if(hCost < other.hCost)
                {
                    result = -1;
                }
                else if (hCost > other.hCost)
                {
                    result = 1;
                }
                else
                {
                    result = 0;
                }
            }

            return result;
        }
    } 
}
