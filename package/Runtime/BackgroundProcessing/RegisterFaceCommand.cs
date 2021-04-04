using AlchemyBow.Navigation.Surfaces;
using AlchemyBow.Navigation.Utilities;
using UnityEngine;

namespace AlchemyBow.Navigation.BackgroundProcessing
{
    /// <summary>
    /// Describes the command to register a face to a surface.
    /// </summary>
    public sealed class RegisterFaceCommand : ICommand
    {
        private readonly Vector3 a;
        private readonly Vector3 b;
        private readonly Vector3 c;
        private readonly NavigationFaceWrapper handle;
        private readonly NavigationSurface surface;

        /// <summary>
        /// Creates a new instance of the RegisterFaceCommand class.
        /// </summary>
        /// <param name="a">Point a of the triangle.</param>
        /// <param name="b">Point b of the triangle.</param>
        /// <param name="c">Point c of the triangle.</param>
        /// <param name="surface">The target surface.</param>
        /// <param name="handle">The face handle.</param>
        public RegisterFaceCommand(Vector3 a, Vector3 b, Vector3 c, 
            NavigationSurface surface, NavigationFaceWrapper handle)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.handle = handle;
            this.surface = surface;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        public void Execute()
        {
            surface.RegisterFace(a, b, c, handle);
        }
    }
}
