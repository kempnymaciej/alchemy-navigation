using AlchemyBow.Navigation.Utilities;

namespace AlchemyBow.Navigation.BackgroundProcessing
{
    /// <summary>
    /// Describes a command with specyfic final task.
    /// </summary>
    public interface IBackgroundCommand : ICommand
    {
        /// <summary>
        /// Invokes the final task.
        /// </summary>
        void OnJoin();
    } 
}
