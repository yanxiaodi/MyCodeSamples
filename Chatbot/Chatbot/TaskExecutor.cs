namespace Chatbot;
internal class TaskExecutor
{
    private readonly Queue<Func<Task>> _taskQueue = new();
    private readonly Task _taskExecutionTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public TaskExecutor()
    {
        _taskExecutionTask = Task.Run(TaskExecutionLoop);
    }

    public void AddTask(Func<Task> taskFunc)
    {
        lock (_taskQueue)
        {
            _taskQueue.Enqueue(taskFunc);
        }
    }

    public async Task StartTaskExecution()
    {
        await _taskExecutionTask;
    }

    public void StopTaskExecution()
    {
        _cancellationTokenSource.Cancel();
    }

    private async Task TaskExecutionLoop()
    {
        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                Func<Task> taskFunc;
                lock (_taskQueue)
                {
                    if (_taskQueue.Count == 0)
                    {
                        continue;
                    }

                    taskFunc = _taskQueue.Dequeue();
                }

                try
                {
                    await taskFunc.Invoke();
                }
                catch (Exception ex)
                {
                    // Handle any exception from the task
                    Console.WriteLine($"Task execution error: {ex}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Cancellation requested, exit the loop
        }
    }
}
