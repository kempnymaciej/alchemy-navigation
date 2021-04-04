using System.Threading;

namespace AlchemyBow.Navigation.BackgroundProcessing
{
    /// <summary>
    /// Handles background processes.
    /// </summary>
    public sealed class BackgroundProcessor
    {
        /// <summary>
        /// Describes the current status of the instance.
        /// </summary>
        public enum States 
        { 
            /// <summary>
            /// The instance is ready to start the next process.
            /// </summary>
            ReadyToStart,
            /// <summary>
            /// The instance is processing a process.
            /// </summary>
            Processing,
            /// <summary>
            /// The instance is waiting to join the process.
            /// </summary>
            WaitingForJoin
        }

        private Thread thread;
        private IBackgroundCommand process;

        /// <summary>
        /// Starts the process.
        /// </summary>
        /// <param name="process">The process to to be computed.</param>
        /// <throws cref="System.Exception">Throws if the instance is already occupied by another process.</throws>
        public void StartProcess(IBackgroundCommand process)
        {
            if (thread == null)
            {
                this.process = process;
                thread = new Thread(process.Execute);
                thread.Start();
            }
            else
            {
                throw new System.Exception("Only one background process can be active. Join or Abort active process before starting another one.");
            }
        }

        /// <summary>
        /// Joins the process.
        /// </summary>
        /// <throws cref="System.Exception">Throws if no process is waiting to be joined.</throws>
        public void JoinProcess()
        {
            if (thread != null)
            {
                if (GetState() == States.WaitingForJoin)
                {
                    thread.Join();
                    thread = null;
                    process.OnJoin();
                    process = null;
                }
                else
                {
                    throw new System.Exception("Process cannot be joined becouse it is not finished.");
                }
            }
            else
            {
                throw new System.Exception("No process to join.");
            }
        }

        /// <summary>
        /// Aborts any process.
        /// </summary>
        public void AbortProcess()
        {
            if (thread != null)
            {
                thread.Abort();
            }
            thread = null;
            process = null;
        }

        /// <summary>
        /// Calculates the current status of the instance.
        /// </summary>
        /// <returns>The current status of the instance.</returns>
        public States GetState()
        {
            States result;
            if (thread == null)
            {
                result = States.ReadyToStart;
            }
            else
            {
                if (thread.ThreadState == ThreadState.Stopped)
                {
                    result = States.WaitingForJoin;
                }
                else
                {
                    result = States.Processing;
                }
            }
            return result;
        }
    } 
}
