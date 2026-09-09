namespace Communication.Mqtt.Connection
{
    internal sealed class AsyncReadyStateTracker
    {
        private readonly Lock _lock = new();
        private TaskCompletionSource<bool> _signal = CreateSignal();


        public bool IsSet
        {
            get { lock (_lock) { return _signal.Task.IsCompletedSuccessfully; } }
        }



        public void Set()
        {
            lock (_lock) { _signal.TrySetResult(true); }
        }

        public void Reset()
        {
            lock (_lock) { if (_signal.Task.IsCompleted) _signal = CreateSignal(); }
        }

        public Task WaitAsync(CancellationToken cancellationToken)
        {
            Task task;
            lock (_lock) { task = _signal.Task; }
            return task.WaitAsync(cancellationToken);
        }


        private static TaskCompletionSource<bool> CreateSignal()
            => new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
