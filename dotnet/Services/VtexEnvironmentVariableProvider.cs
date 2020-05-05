namespace WishList.Services
{
    using System;

    public class VtexEnvironmentVariableProvider : IVtexEnvironmentVariableProvider
    {
        public VtexEnvironmentVariableProvider()
        {
            this.Account = Environment.GetEnvironmentVariable("VTEX_ACCOUNT");
            this.Workspace = Environment.GetEnvironmentVariable("VTEX_WORKSPACE");
            this.ApplicationName = Environment.GetEnvironmentVariable("VTEX_APP_NAME");
            this.ApplicationVendor = Environment.GetEnvironmentVariable("VTEX_APP_VENDOR");
            this.Region = Environment.GetEnvironmentVariable("VTEX_REGION");
        }

        public string Account { get; }
        public string Workspace { get; }
        public string ApplicationName { get; }
        public string ApplicationVendor { get; }
        public string Region { get; }
    }
}
