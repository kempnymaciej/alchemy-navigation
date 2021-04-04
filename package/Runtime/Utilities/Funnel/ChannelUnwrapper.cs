using UnityEngine;

namespace AlchemyBow.Navigation.Utilities
{
    /// <summary>
    /// Describes a 3d channel as 2d with standardized its portals.
    /// </summary>
    public sealed class ChannelUnwrapper
    {
        private int channelLength;
        private Vector3[] wrappedChannel;
        private Vector2[] unwrappedChannel;

        private int portalCount;
        private bool[] portalInverseStates;

        /// <summary>
        /// Gets the number of elements contained in the channel array.
        /// </summary>
        /// <returns>The number of elements contained in the channel array.</returns>
        /// <remarks>This is given by the formula 2 + 2 *  the number of portals.</remarks>
        public int ChannelLength => channelLength;

        /// <summary>
        /// Gets the unwrapped channel in form of an array where: the first index is a start point; the last index is an end point; other indexes are left(odd) and right(even) points of  the portals.
        /// </summary>
        /// <returns>The unwrapped channel in form of an array where: the first index is a start point; the last index is an end point; other indexes are left(odd) and right(even) points of  the portals.</returns>
        public Vector2[] UnwrappedChannel => unwrappedChannel;

        /// <summary>
        /// Unwraps and standardizes a channel.
        /// </summary>
        /// <param name="channel">An array where: the first index is the start point; the last index is the end point; other indexes are the channel portals pairs.</param>
        public void UnwrapAndStandardizePortals(Vector3[] channel)
        {
            channelLength = channel.Length;
            wrappedChannel = channel;

            if(unwrappedChannel == null || unwrappedChannel.Length < channelLength)
            {
                unwrappedChannel = new Vector2[channelLength];
            }

            for (int i = 0; i < channelLength; i++)
            {
                unwrappedChannel[i] = new Vector2(channel[i].x, channel[i].z);
            }

            StandardizePortals();
        }

        private void StandardizePortals()
        {
            portalCount = channelLength / 2 - 1;
            if (portalInverseStates == null || portalInverseStates.Length < portalCount)
            {
                portalInverseStates = new bool[portalCount];
            }

            Vector2 pivot = unwrappedChannel[0];
            Vector2 temp;
            for (int i = 1; i < channelLength - 1; i += 2)
            {
                if (Vector2.SignedAngle(unwrappedChannel[i] - pivot, unwrappedChannel[i + 1] - pivot) > 0)
                {
                    temp = unwrappedChannel[i];
                    unwrappedChannel[i] = unwrappedChannel[i + 1];
                    unwrappedChannel[i + 1] = temp;

                    portalInverseStates[i / 2] = true;
                    pivot = unwrappedChannel[i + 1];
                }
                else
                {
                    portalInverseStates[i / 2] = false;
                    pivot = unwrappedChannel[i];
                }
            }
        }

        /// <summary>
        /// Calculates points located on portals in given ratios.
        /// </summary>
        /// <param name="path">An array of ratios between points of each portal.</param>
        /// <returns>An array of points located on portals in given ratios.</returns>
        public Vector3[] WrapExactPath(float[] path)
        {
            var result = new Vector3[portalCount + 2];
            result[0] = wrappedChannel[0];
            result[portalCount + 1] = wrappedChannel[channelLength - 1];

            for (int portalIndex = 0; portalIndex < portalCount ; portalIndex++)
            {
                result[portalIndex + 1] = portalInverseStates[portalIndex] ?
                    Vector3.Lerp(wrappedChannel[2 + 2 * portalIndex], wrappedChannel[1 + 2 * portalIndex], path[portalIndex]) :
                    Vector3.Lerp(wrappedChannel[1 + 2 * portalIndex], wrappedChannel[2 + 2 * portalIndex], path[portalIndex]);
            }

            return result;
        }

        /// <summary>
        /// Gets the source point for the index of the unwrapped channel.
        /// </summary>
        /// <param name="index">An index of the unwrapped channel.</param>
        /// <returns>The source point for the index of the unwrapped channel</returns>
        public Vector3 WrapPoint(int index)
        {
            Vector3 result;
            if (index <= 0)
            {
                result = wrappedChannel[index];
            }
            else if (index >= channelLength - 1)
            {
                result = wrappedChannel[channelLength - 1];
            }
            else
            {
                int portalIndex = index / 2;
                if(index % 2 == 0)
                {
                    portalIndex--;
                }

                if (portalInverseStates[portalIndex])
                {
                    result = (index % 2 == 0) ? wrappedChannel[index - 1] : wrappedChannel[index + 1];
                }
                else
                {
                    result = wrappedChannel[index];
                }
            }

            return result;
        }
    } 
}
