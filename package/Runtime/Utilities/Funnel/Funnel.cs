using System;
using UnityEngine;

namespace AlchemyBow.Navigation.Utilities
{
    public sealed class Funnel
    {
        private ChannelUnwrapper channelUnwrapper;

        private Vector2[] channel;
        private int channelLength;

        private int[] tail;
        private int tailLength;
        private int[] leftWedge;
        private int leftWedgeLength;
        private int[] rightWedge;
        private int rightWedgeLength;

        private Vector2 apex;
        private int progress;

        private bool IsProgressOnLeftSideOfChannel => progress % 2 == 1;

        private void CalculateFunnel(Vector2[] standarizedChannel, int standarizedChannelLength)
        {
            channel = standarizedChannel;
            channelLength = standarizedChannelLength;
            ResetAndEnsureArraysSize();

            tail[0] = 0;
            tailLength = 1;
            ResetFunnelAtTailEnd();

            while (progress < channelLength)
            {
                if (IsOnLeft(channel[progress], channel[leftWedge[0]]))
                {
                    HandleProgressOnLeftOfLeftWedge();
                }
                else if (IsOnRight(channel[progress], channel[rightWedge[0]]))
                {
                    HandleProgressOnRigthOfRigthWedge();
                }
                else
                {
                    ReplaceWedgeByProgress();
                }
            }
            FinalStep();
        }

        private void ResetAndEnsureArraysSize()
        {
            int portalCount = channelLength / 2 - 1;

            int maxTailLength = portalCount + 2;
            if (tail == null || tail.Length < maxTailLength)
            {
                tail = new int[maxTailLength];
            }
            tailLength = 0;

            int maxWegdeLength = portalCount + 1;
            if (leftWedge == null || leftWedge.Length < maxWegdeLength)
            {
                leftWedge = new int[maxWegdeLength];
            }
            leftWedgeLength = 0;
            if (rightWedge == null || rightWedge.Length < maxWegdeLength)
            {
                rightWedge = new int[maxWegdeLength];
            }
            rightWedgeLength = 0;
        }
        private void ResetFunnelAtTailEnd()
        {
            int apexIndex = tail[tailLength - 1];
            apex = channel[apexIndex];
            leftWedgeLength = 1;
            rightWedgeLength = 1;

            int offset = apexIndex % 2;
            leftWedge[0] = apexIndex + 1 + offset;
            rightWedge[0] = apexIndex + 2 + offset;
            //Debug.Log("ResetFunnelAtTailEnd " + progress + "->" + (apexIndex + 3 + offset));
            progress = apexIndex + 3 + offset;
        }

        private void AddToTail(int channelIndex)
        {
            tail[tailLength] = channelIndex;
            tailLength++;
        }

        private void FinalStep()
        {
            int goalIndex = channelLength - 1;
            if (leftWedge[leftWedgeLength-1] == goalIndex)
            {
                for (int i = 0; i < leftWedgeLength; i++)
                {
                    AddToTail(leftWedge[i]);
                }
            }
            else if(rightWedge[rightWedgeLength - 1] == goalIndex)
            {
                for (int i = 0; i < rightWedgeLength; i++)
                {
                    AddToTail(rightWedge[i]);
                }
            }
            else
            {
                AddToTail(goalIndex);
            }
        }

        private void ReplaceWedgeByProgress()
        {
            if (IsProgressOnLeftSideOfChannel)
            {
                leftWedge[0] = progress;
                leftWedgeLength = 1;
            }
            else
            {
                rightWedge[0] = progress;
                rightWedgeLength = 1;
            }

            //Debug.Log("ReplaceWedgeByProgress " + progress + "->" + (progress + 1));
            progress++;
        }
        private void HandleProgressOnLeftOfLeftWedge()
        {
            if (IsProgressOnLeftSideOfChannel)
            {
                InsertProgressInLeftWedge();
                //Debug.Log("InsertProgressInLeftWedge " + progress + "->" + (progress + 1));
                progress++;
            }
            else
            {
                LayRightWedgeOnLeftWedgeByProgress();
            }
        }
        private void HandleProgressOnRigthOfRigthWedge()
        {
            if (!IsProgressOnLeftSideOfChannel)
            {
                InsertProgressInRigthWedge();
                //Debug.Log("InsertProgressInRigthWedge " + progress + "->" + (progress + 1));
                progress++;
            }
            else
            {
                LayLeftWedgeOnRightWedgeByProgress();
            }
        }

        private void InsertProgressInLeftWedge()
        {
            int insertIndex = 1;
            while(insertIndex < leftWedgeLength && IsOnLeft(channel[progress], channel[leftWedge[insertIndex]]))
            {
                insertIndex++;
            }

            leftWedge[insertIndex] = progress;
            leftWedgeLength = insertIndex + 1;
        }
        private void InsertProgressInRigthWedge()
        {
            int insertIndex = 1;
            while (insertIndex < rightWedgeLength && IsOnRight(channel[progress], channel[rightWedge[insertIndex]]))
            {
                insertIndex++;
            }

            rightWedge[insertIndex] = progress;
            rightWedgeLength = insertIndex + 1;
        }

        private void LayRightWedgeOnLeftWedgeByProgress()
        {
            AddToTail(leftWedge[0]);
            int index = 1;
            while (index < leftWedgeLength && IsOnLeft(channel[progress], channel[leftWedge[index]]))
            {
                AddToTail(leftWedge[index]);
                index++;
            }
            ResetFunnelAtTailEnd();
        }
        private void LayLeftWedgeOnRightWedgeByProgress()
        {
            AddToTail(rightWedge[0]);
            int index = 1;
            while (index < rightWedgeLength && IsOnRight(channel[progress], channel[rightWedge[index]]))
            {
                AddToTail(rightWedge[index]);
                index++;
            }
            ResetFunnelAtTailEnd();
        }

        private bool IsOnLeft(Vector2 point, Vector2 from) => Vector2.SignedAngle(from - apex, point - apex) > 0;
        private bool IsOnRight(Vector2 point, Vector2 from) => Vector2.SignedAngle(from - apex, point - apex) < 0;


        /// <summary>
        /// Extracts path.
        /// </summary>
        /// <returns>The path represented by indexes of the given channel. (Including start point and goal.)</returns>
        private int[] ExtractIndexPath()
        {
            var result = new int[tailLength];
            Array.Copy(tail, result, tailLength);
            return result;
        }


        private Vector3[] ExtractOptimizedPath()
        {
            var result = new Vector3[tailLength];
            for (int i = 0; i < tailLength; i++)
            {
                result[i] = channelUnwrapper.WrapPoint(tail[i]);
            }
            return result;
        }

        /// <summary>
        /// Extracts path as 0(left)-1(right) number for each portal.
        /// </summary>
        /// <returns>The path represented by normalized distances between points for each portal - 
        /// left(0), right(1), others between.
        /// </returns>
        private float[] ExtractExactPath()
        {
            int portalsCount = channelLength / 2 - 1;
            var result = new float[portalsCount];

            int progress = 1;
            for (int portalIndex = 0; portalIndex < portalsCount; portalIndex++)
            {
                int pathPortalIndex = (tail[progress] - 1) / 2;
                if (portalIndex < pathPortalIndex)
                {
                    Vector2 portalL = channel[1 + 2 * portalIndex];
                    Vector2 portalR = channel[2 + 2 * portalIndex];
                    Vector2 intersectionPoint = IntersectionPoint(
                        channel[tail[progress - 1]], channel[tail[progress]],
                        portalL, portalR);

                    result[portalIndex] = Vector2.Distance(portalL, intersectionPoint) / Vector2.Distance(portalL, portalR);
                }
                else
                {
                    result[portalIndex] = tail[progress] % 2 == 1 ? 0 : 1;
                    progress++;
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates the shortest path for a 3d channel.
        /// </summary>
        /// <param name="channel">An array where: the first index is the start point; the last index is the end point; other indexes are the channel portals pairs.</param>
        /// <param name="exactPath">If <c>true</c>, adds points for each portal, otherwise only adds corner points.</param>
        /// <returns>The calculated path.</returns>
        /// <remarks>Portal points don't have to be standardized.</remarks>
        /// <remarks>The length of the channel array must be given by the formula: 2 + 2 *  the number of portals.</remarks>
        public Vector3[] Calculate3DPath(Vector3[] channel, bool exactPath)
        {
            if(channelUnwrapper == null)
            {
                channelUnwrapper = new ChannelUnwrapper();
            }
            channelUnwrapper.UnwrapAndStandardizePortals(channel);
            CalculateFunnel(channelUnwrapper.UnwrappedChannel, channelUnwrapper.ChannelLength);

            if (exactPath)
            {
                return channelUnwrapper.WrapExactPath(ExtractExactPath());
            }
            else
            {
                return ExtractOptimizedPath();
            }
        }

        private static Vector2 IntersectionPoint(Vector2 line1A, Vector2 line1B, Vector2 line2A, Vector2 line2B)
        {
            float A1 = line1B.y - line1A.y;
            float B1 = line1A.x - line1B.x;
            float C1 = A1 * line1A.x + B1 * line1A.y;

            float A2 = line2B.y - line2A.y;
            float B2 = line2A.x - line2B.x;
            float C2 = A2 * line2A.x + B2 * line2A.y;

            float denominator = A1 * B2 - A2 * B1;

            return new Vector2(B2 * C1 - B1 * C2, A1 * C2 - A2 * C1) / denominator;
        }
    } 
}
