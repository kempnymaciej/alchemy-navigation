using AlchemyBow.Navigation.Surfaces.SafeAccess;
using UnityEngine;

namespace AlchemyBow.Navigation.Simple.Elements
{
    /// <summary>
    /// Caches the face and provides additional vector math methods.
    /// </summary>
    public sealed class CachedFace
    {
        private const float EpsilonOffset = NavigationInfo.EpsilonOffset;

        /// <summary>
        /// The source face.
        /// </summary>
        /// <returns>The source face.</returns>
        public readonly IImmutableFace source;

        /// <summary>
        /// The cached plane of the source face.
        /// </summary>
        /// <returns>The cached plane of the source face.</returns>
        public readonly Plane plane;

        /// <summary>
        /// The cached A vertex value of the source face.
        /// </summary>
        /// <returns>The cached A vertex value of the source face.</returns>
        public readonly Vector3 a;

        /// <summary>
        /// The cached B vertex value of the source face.
        /// </summary>
        /// <returns>The cached B vertex value of the source face.</returns>
        public readonly Vector3 b;

        /// <summary>
        /// The cached C vertex value of the source face.
        /// </summary>
        /// <returns>The cached C vertex value of the source face.</returns>
        public readonly Vector3 c;

        /// <summary>
        /// Creates an instance of the CachedFace class.
        /// </summary>
        /// <param name="source">The source face.</param>
        public CachedFace(IImmutableFace source)
        {
            this.source = source;

            this.plane = source.Plane;
            this.a = source.A;
            this.b = source.B;
            this.c = source.C;
        }

        /// <summary>
        /// Snaps the result of the displacement to the face.
        /// </summary>
        /// <param name="current">The current position.</param>
        /// <param name="displacement">The displacement.</param>
        /// <returns>The snapped displacement result.</returns>
        /// <remarks>The <c>current</c> must be inside the face, and displacement must point outside.</remarks>
        public Vector3 SnapToFace(Vector3 current, Vector3 displacement)
        {
            //TODO: Introduce better decomposition.

            float aAngle = Vector3.SignedAngle(displacement, a - current, plane.normal);
            if (aAngle < 0) aAngle += 360;
            float bAngle = Vector3.SignedAngle(displacement, b - current, plane.normal);
            if (bAngle < 0) bAngle += 360;
            float cAngle = Vector3.SignedAngle(displacement, c - current, plane.normal);
            if (cAngle < 0) cAngle += 360;

            if (current == a)
                return SnapToEdgeFromVertex(current, b, c, displacement);
            if (current == b)
                return SnapToEdgeFromVertex(current, a, c, displacement);
            if (current == c)
                return SnapToEdgeFromVertex(current, a, b, displacement);

            if (aAngle == 0)
                return a;
            if (bAngle == 0)
                return b;
            if (cAngle == 0)
                return c;

            Vector3 rightVertex;
            Vector3 leftVertex;
            if (aAngle < bAngle)
            {
                if (aAngle < cAngle)
                {
                    rightVertex = a;
                    leftVertex = bAngle > cAngle ? b : c;
                }
                else
                {
                    rightVertex = c;
                    leftVertex = aAngle > bAngle ? a : b;
                }
            }
            else
            {
                if (bAngle < cAngle)
                {
                    rightVertex = b;
                    leftVertex = aAngle > cAngle ? a : c;
                }
                else
                {
                    rightVertex = c;
                    leftVertex = aAngle > bAngle ? a : b;
                }
            }

            return SnapToEdge(rightVertex, leftVertex, current, displacement);
        }

        private Vector3 SnapToEdgeFromVertex(Vector3 current, Vector3 v1, Vector3 v2, Vector3 displacement)
        {
            float v1Angle = Vector3.Angle(displacement, v1 - current);
            float v2Angle = Vector3.Angle(displacement, v2 - current);

            if (v1Angle < 90 && v1Angle < v2Angle)
            {
                return current + Vector3.ClampMagnitude(v1 - current, Mathf.Min(displacement.magnitude, (v1 - current).magnitude));
            }
            if (v2Angle < 90 && v2Angle < v1Angle)
            {
                return current + Vector3.ClampMagnitude(v2 - current, Mathf.Min(displacement.magnitude, (v2 - current).magnitude));
            }

            return current;
        }

        private Vector3 SnapToEdge(Vector3 a, Vector3 b, Vector3 current, Vector3 displacement)
        {
            Vector3 aPrim = current + Vector3.Project(a - current, displacement);
            Vector3 bPrim = current + Vector3.Project(b - current, displacement);

            
            float aaPrimMagnitude = (aPrim - a).magnitude;
            if (aaPrimMagnitude <= EpsilonOffset) return a;
            float bbPrimMagnitude = (bPrim - b).magnitude;
            if (bbPrimMagnitude <= EpsilonOffset) return b;

            float lengthsAAPrimPerBBPrim = aaPrimMagnitude / bbPrimMagnitude;
            float length = ((b - a).magnitude * lengthsAAPrimPerBBPrim) / (1 + lengthsAAPrimPerBBPrim);
            return Vector3.MoveTowards(a, b, length);
        }

        /// <summary>
        /// Casts displacement on the closest edge of the face.
        /// </summary>
        /// <param name="current">The current position.</param>
        /// <param name="displacement">The displacement.</param>
        /// <returns>The casted displacement.</returns>
        public Vector3 CastDisplacementOnClosestEdge(Vector3 current, Vector3 displacement)
        {
            float aAngle = Vector3.Angle(displacement, a - current);
            float bAngle = Vector3.Angle(displacement, b - current);
            float cAngle = Vector3.Angle(displacement, c - current);


            if (aAngle < bAngle && aAngle < cAngle)
            {
                return Vector3.ClampMagnitude(Vector3.Project(displacement, a - current), Mathf.Min(displacement.magnitude, (a - current).magnitude));
            }
            else if (bAngle < aAngle && bAngle < cAngle)
            {
                return Vector3.ClampMagnitude(Vector3.Project(displacement, b - current), Mathf.Min(displacement.magnitude, (b - current).magnitude));
            }
            else
            {
                return Vector3.ClampMagnitude(Vector3.Project(displacement, c - current), Mathf.Min(displacement.magnitude, (c - current).magnitude));
            }
        }
    } 
}
