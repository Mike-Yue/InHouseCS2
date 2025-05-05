using InHouseCS2.Core.Common.Contracts;
using System.Threading.Channels;

namespace InHouseCS2.Core.Common;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> queue;

    public BackgroundTaskQueue()
    {
        this.queue = Channel.CreateUnbounded<Func<IServiceProvider, CancellationToken, Task>>();
    }

    public async Task<Func<IServiceProvider, CancellationToken, Task>> DequeueBackgroundTask(CancellationToken cancellationToken)
    {
        var workItem = await this.queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }

    public void EnqueueBackgroundTask(Func<IServiceProvider, CancellationToken, Task> workItem)
    {
        if (workItem != null)
        {
            this.queue.Writer.TryWrite(workItem);
        }
        else
        {
            throw new ArgumentNullException(nameof(workItem));
        }
    }
}
