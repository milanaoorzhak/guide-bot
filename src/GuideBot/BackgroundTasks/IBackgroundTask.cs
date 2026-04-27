namespace GuideBot.BackgroundTasks;

public interface IBackgroundTask
{
  Task Start(CancellationToken ct);
}