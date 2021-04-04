using UnityEngine;

namespace AlchemyBow.Navigation.Simple.Elements
{
    /// <summary>
    /// The <c>MovementModifier</c> that provides a simple avoidance behavior.
    /// </summary>
    public sealed class AvoidanceModifier : MovementModifier
    {
        [SerializeField]
        [Min(0)]
        private float radius = 1;
        [SerializeField]
        [Min(0)]
        private float height = 1;

        [SerializeField]
        private AvoidanceGroup avoidanceGroup = null;

        /// <summary>
        /// Gets or sets the avoidance radius for the agent.
        /// </summary>
        /// <returns>The avoidance radius.</returns>
        public float Radius { get => radius; set => radius = value >= 0 ? value : 0; }

        /// <summary>
        /// Gets or sets the avoidance height for the agent.
        /// </summary>
        /// <returns>The avoidance height.</returns>
        public float Height { get => height; set => height = value >= 0 ? value : 0; }

        /// <summary>
        /// Gets or sets the group to which the object belongs.
        /// </summary>
        /// <returns>The group to which the object belongs.</returns>
        public AvoidanceGroup AvoidanceGroup
        {
            get => avoidanceGroup;
            set
            {
                if(avoidanceGroup != null)
                {
                    avoidanceGroup.RemoveFromGroup(this);
                }
                avoidanceGroup = value;
                avoidanceGroup.AddToGruop(this);
            }
        }

        /// <summary>
        /// Calculates the avoidance acceleration direction.
        /// </summary>
        /// <returns>The avoidance acceleration direction.</returns>
        public override Vector3 CalculateDirection()
        {
            return avoidanceGroup.CalculateAvoidanceDirection(this);
        }

        private void OnEnable()
        {
            if (avoidanceGroup != null)
            {
                avoidanceGroup.AddToGruop(this); 
            }
        }

        private void OnDisable()
        {
            if (avoidanceGroup != null)
            {
                avoidanceGroup.RemoveFromGroup(this);
            }
        }
    }
}
