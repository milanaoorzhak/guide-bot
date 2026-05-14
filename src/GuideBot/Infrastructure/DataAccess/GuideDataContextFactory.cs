namespace GuideBot.Infrastructure.DataAccess;

public class GuideDataContextFactory : IDataContextFactory<GuideDataContext>
{
    private readonly string _connectionString;

    public GuideDataContextFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public GuideDataContext CreateDataContext()
    {
        return new GuideDataContext(_connectionString);
    }
}
