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
            WishListsWrapper wishListsWrapper = await _wishListRepository.GetAllLists();
            var queryString = HttpContext.Request.Query;
            int from = int.Parse(queryString["from"]);
            int to = int.Parse(queryString["to"]);
            IList<WishListWrapper> wishListsWrappers = wishListsWrapper.WishLists;
            wishListsWrapper.WishLists = wishListsWrappers.Skip(from - 1).Take(to - from).ToList();

            return Json(wishListsWrapper);
        }
    }
}
