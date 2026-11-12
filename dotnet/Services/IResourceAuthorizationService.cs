using System.Net;
using System.Threading.Tasks;

namespace WishList.Services
{
    public interface IResourceAuthorizationService
    {
        /// <summary>
        /// Checks that the caller has a valid VTEX ID session for this account and that the
        /// License Manager resource is granted to the logged in user.
        /// Returns OK, Unauthorized (no or invalid session), Forbidden (resource not granted)
        /// or BadGateway (a dependency failed, the request is denied).
        /// </summary>
        Task<HttpStatusCode> HasExportResource();
    }
}
