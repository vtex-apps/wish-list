namespace service.Controllers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Vtex.Api.Context;

    public class RoutesController : Controller
    {
        private readonly IIOServiceContext _context;

        public RoutesController(IIOServiceContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }
    }
}
