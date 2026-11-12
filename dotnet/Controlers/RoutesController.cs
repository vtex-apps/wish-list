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
    using WishList.Services;

    public class RoutesController : Controller
    {
        private readonly IIOServiceContext _context;
        private readonly IWishListRepository _wishListRepository;
        private readonly IResourceAuthorizationService _resourceAuthorizationService;

        public RoutesController(IIOServiceContext context, IWishListRepository wishListRepository, IResourceAuthorizationService resourceAuthorizationService)
        {
            this._resourceAuthorizationService = resourceAuthorizationService ?? throw new ArgumentNullException(nameof(resourceAuthorizationService));
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._wishListRepository = wishListRepository ?? throw new ArgumentNullException(nameof(wishListRepository));
        }

        public async Task<IActionResult> ExportAllLists()
        {
            HttpStatusCode authStatus = await _resourceAuthorizationService.HasExportResource();
            if (authStatus != HttpStatusCode.OK)
            {
                return StatusCode((int)authStatus);
            }

            WishListsWrapper wishListsWrapper = await _wishListRepository.GetAllLists();
            
            return Json(wishListsWrapper);
        }
    }
}
