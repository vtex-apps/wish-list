namespace service.Controllers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Vtex.Api.Context;
    using WishList.Data;
    using WishList.Models;
    using WishList.Services;

    public class RoutesController : Controller
    {
        private readonly IIOServiceContext _context;
        private readonly IWishListRepository _wishListRepository;
        private readonly IWishListService _wishListService;

        public RoutesController(IIOServiceContext context, IWishListRepository wishListRepository, IWishListService wishListService)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._wishListRepository = wishListRepository ?? throw new ArgumentNullException(nameof(wishListRepository));
            this._wishListService = wishListService ?? throw new ArgumentNullException(nameof(wishListService));
        }

        public async Task<IActionResult> ExportAllLists()
        {
            HttpStatusCode isAdminUser = await _wishListService.IsValidAuthUser();
            if (isAdminUser.Equals(HttpStatusCode.OK))
            {
                WishListsWrapper wishListsWrapper = await _wishListRepository.GetAllLists();

                return Json(wishListsWrapper);
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
