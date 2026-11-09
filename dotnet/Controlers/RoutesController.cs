namespace service.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Vtex.Api.Context;
    using WishList.Data;
    using WishList.Exporting;
    using WishList.Models;

    public class RoutesController : Controller
    {
        private readonly IIOServiceContext _context;
        private readonly IWishListRepository _wishListRepository;

        public RoutesController(IIOServiceContext context, IWishListRepository wishListRepository)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._wishListRepository = wishListRepository ?? throw new ArgumentNullException(nameof(wishListRepository));
        }

        public async Task<IActionResult> ExportAllLists()
        {
            try
            {
                WishListsWrapper wishListsWrapper = await _wishListRepository.GetAllLists(HttpContext.RequestAborted);
                return Json(wishListsWrapper);
            }
            catch (ExportBusyException)
            {
                Response.Headers["Retry-After"] = "5";
                return StatusCode(StatusCodes.Status429TooManyRequests, new
                {
                    code = "EXPORT_IN_PROGRESS",
                    message = "Another export is already running on this instance.",
                    retryable = true
                });
            }
        }
    }
}
