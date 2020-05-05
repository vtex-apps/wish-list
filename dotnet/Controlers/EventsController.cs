namespace service.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public class EventsController : Controller
    {
        public string OnAppsLinked(string account, string workspace)
        {
            return $"OnAppsLinked event detected for {account}/{workspace}";
        } 
    }
}
