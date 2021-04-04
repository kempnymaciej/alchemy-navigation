using System.Collections.Generic;
using UnityEngine;

namespace AlchemyBow.Navigation.Simple.Elements
{
    /// <summary>
    /// A <c>SctiptableObject</c> that represents the avoidance group.
    /// </summary>
    [CreateAssetMenu(fileName = "AvoidanceGroup", menuName = "AlchemyBow/Navigation/AvoidanceGroup")]
    public sealed class AvoidanceGroup : ScriptableObject
    {
        private List<AvoidanceModifier> agents;

        /// <summary>
        /// Adds the agent to the group.
        /// </summary>
        /// <param name="agent">An agent to add.</param>
        public void AddToGruop(AvoidanceModifier agent)
        {
            if(agent == null)
            {
                Debug.LogWarning("You are trying to add null to avoidance group.");
                return;
            }

            if(agents == null)
            {
                agents = new List<AvoidanceModifier>();
            }

            if (agents.Contains(agent))
            {
                Debug.LogWarning($"{this.name}: this group already contains this agent({agent.gameObject.name}). Request will be ignored.");
            }
            else
            {
                agents.Add(agent);
            }
        }

        /// <summary>
        /// Removes the agent from the group.
        /// </summary>
        /// <param name="agent">An agent to remove.</param>
        public void RemoveFromGroup(AvoidanceModifier agent)
        {
            if (agent == null)
            {
                return;
            }

            agents.Remove(agent);
            if(agents.Count == 0)
            {
                agents = null;
            }
        }

        /// <summary>
        /// Calculates the avoidance acceleration direction for the agent.
        /// </summary>
        /// <param name="agent">The agent to the avoidance acceleration direction.</param>
        /// <returns>The avoidance acceleration direction for the agent.</returns>
        public Vector3 CalculateAvoidanceDirection(AvoidanceModifier agent)
        {
            Vector3 avoidanceMove = Vector3.zero;
            int avoidanceCount = 0;

            Vector3 agentPosition = agent.transform.position;
            float agentBot = agentPosition.y;
            float agentTop = agentBot + agent.Height;
            foreach (var obstacle in agents)
            {
                if (obstacle != agent)
                {
                    Vector3 obstaclePosition = obstacle.transform.position;
                    float obstacleBot = obstaclePosition.y;
                    float obstacleTop = obstacleBot + obstacle.Height;

                    if((obstacleBot >= agentBot && obstacleBot <= agentTop) 
                        || (obstacleTop >= agentBot && obstacleTop <= agentTop))
                    {
                        if (Distance_XZ(agentPosition, obstaclePosition)
                                < agent.Radius + obstacle.Radius)
                        {
                            avoidanceCount++;
                            avoidanceMove += agentPosition - obstaclePosition;
                            //Debug.DrawLine(agentPosition, obstaclePosition, Color.magenta, 1);
                        }
                    }
                }
            }

            if (avoidanceCount > 0)
            {
                avoidanceMove /= avoidanceCount;
                avoidanceMove.y = 0;
                return avoidanceMove.normalized;
            }
            
            return avoidanceMove;
        }

        private static float Distance_XZ(Vector3 a, Vector3 b)
        {
            a.y = 0;
            b.y = 0;
            return Vector3.Distance(a, b);
        }
    } 
}
