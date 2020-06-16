using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;


namespace ASC.Utilities.Navigation
{
    [ViewComponent(Name = "ASC.Utilities.Navigation.LeftNavigation")]
    public class LeftNavigationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(NavigationMenu navigationMenu)
        {
            navigationMenu.MenuItems = navigationMenu.MenuItems.OrderBy(m => m.Sequence).ToList();
            return View(navigationMenu);
        }
    }
}
