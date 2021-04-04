using AlchemyBow.Navigation.Surfaces;
using AlchemyBow.Navigation.Utilities;

namespace AlchemyBow.Navigation.BackgroundProcessing
{
    /// <summary>
    /// Describes the command to unregister a face from a surface.
    /// </summary>
    public sealed class UnregisterFaceCommand : ICommand
    {
        private readonly NavigationFaceWrapper handle;
        private readonly NavigationSurface surface;

        /// <summary>
        /// Creates a new instance of the UnregisterFaceCommand class.
        /// </summary>
        /// <param name="handle">The face handle.</param>
        /// <param name="surface">The target surface.</param>
        public UnregisterFaceCommand(NavigationFaceWrapper handle, NavigationSurface surface)
        {
            this.handle = handle;
            this.surface = surface;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        public void Execute()
        {
            surface.UnregisterFace(handle);
        }
    }
}
