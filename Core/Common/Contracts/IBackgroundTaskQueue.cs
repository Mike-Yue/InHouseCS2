namespace InHouseCS2.Core.Common.Contracts;

public interface IBackgroundTaskQueue
{
    public void EnqueueBackgroundTask(Func<IServiceProvider, CancellationToken, Task> workItem);

    public Task<Func<IServiceProvider, CancellationToken, Task>> DequeueBackgroundTask(CancellationToken cancellationToken);
}
