using System;
using System.Collections.Generic;
using AlchemyBow.Navigation.Utilities;

namespace AlchemyBow.Navigation.BackgroundProcessing
{
    /// <summary>
    /// Describes the tasks of the process and what to do when finished.
    /// </summary>
    public sealed class BuildingProcess : IBackgroundCommand
    {
        private readonly Action onJoin;
        private Queue<ICommand> commands;

        /// <summary>
        /// Creates a new instance of the BuildingProcess class.
        /// </summary>
        /// <param name="commands">The tasks to be perform.</param>
        /// <param name="onJoin">What to do when finished.</param>
        public BuildingProcess(Queue<ICommand> commands, Action onJoin)
        {
            this.onJoin = onJoin;
            this.commands = commands;
        }

        /// <summary>
        /// Executes all commands.
        /// </summary>
        public void Execute()
        {
            foreach (var command in commands)
            {
                command.Execute();
            }
            commands = null;
        }

        /// <summary>
        /// Invokes the final task.
        /// </summary>
        public void OnJoin()
        {
            onJoin.Invoke();
        }
    }
}
