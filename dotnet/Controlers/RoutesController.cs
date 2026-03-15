namespace service.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Vtex.Api.Context;
    using WishList.Models;
    using WishList.Services;

    public class RoutesController : Controller
    {
        private readonly IIOServiceContext _context;
        private readonly IWishListService _wishListService;

        public RoutesController(IIOServiceContext context, IWishListService wishListService)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._wishListService = wishListService ?? throw new ArgumentNullException(nameof(wishListService));
        }

        public async Task<IActionResult> ExportAllLists()
        {
            WishListsWrapper wishListsWrapper = await _wishListService.ExportAllWishLists();

            return Json(wishListsWrapper);
        }
    }
}
