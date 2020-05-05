namespace WishList.Services
{
    public interface IVtexEnvironmentVariableProvider
    {
        string Account { get; }
        string Workspace { get; }
        string ApplicationName { get; }
        string ApplicationVendor { get; }
        string Region { get; }
    }
}
