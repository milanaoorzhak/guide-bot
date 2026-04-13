using LinqToDB.Data;

namespace GuideBot.Infrastructure.DataAccess;

public interface IDataContextFactory<TDataContext> where TDataContext : DataConnection
{
    TDataContext CreateDataContext();
}
