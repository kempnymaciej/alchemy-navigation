namespace AlchemyBow.Navigation.DebugUnits
{
    /// <summary>
    /// A multi surface debug drawer component.
    /// </summary>
    public sealed class CompositeSurfaceDrawer : ISurfaceDrawer
    {
        private readonly ISurfaceDrawer[] drawers;

        /// <summary>
        /// Creates a new instance of the CompositeSurfaceDrawer class.
        /// </summary>
        /// <param name="drawers">Drawers to be used.</param>
        public CompositeSurfaceDrawer(ISurfaceDrawer[] drawers)
        {
            this.drawers = drawers;
        }

        /// <summary>
        /// Draws surface with drawers.
        /// </summary>
        public void DrawSurface()
        {
            foreach (var drawer in drawers)
            {
                drawer.DrawSurface();
            }
        }
    }
}
