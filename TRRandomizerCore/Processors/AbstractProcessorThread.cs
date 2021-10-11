using System;
using System.Threading;

namespace TRRandomizerCore.Processors
{
    internal abstract class AbstractProcessorThread<R> where R : ILevelProcessor
    {
        protected readonly R _outer;
        protected readonly Thread _thread;
        private bool _started;

        internal abstract int LevelCount { get; }

        internal AbstractProcessorThread(R outer)
        {
            _outer = outer;
            _thread = new Thread(Process);
            _started = false;
        }

        internal void Start()
        {
            if (!_started)
            {
                try
                {
                    StartImpl();
                    _thread.Start();
                    _started = true;
                }
                catch (Exception e)
                {
                    _outer.HandleException(e);
                }
            }
        }

        protected virtual void StartImpl() { }        

        internal void Join()
        {
            if (_thread.ThreadState != ThreadState.Unstarted)
            {
                _thread.Join();

                if (_outer.TriggerProgress(0))
                {
                    try
                    {
                        JoinImpl();
                    }
                    catch (Exception e)
                    {
                        _outer.HandleException(e);
                    }
                }

                _started = false;
            }
        }

        protected virtual void JoinImpl() { }

        private void Process()
        {
            try
            {
                ProcessImpl();
            }
            catch (Exception e)
            {
                _outer.HandleException(e);
            }
        }

        protected abstract void ProcessImpl();
    }
}