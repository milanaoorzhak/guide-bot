namespace GuideBot;

public class TaskCountLimitException : Exception
{
    public TaskCountLimitException(int taskCountLimit)
        : base($"Превышено максимальное количество задач равное {taskCountLimit}") { }
}