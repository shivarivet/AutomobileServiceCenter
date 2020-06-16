using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ElCamino.AspNetCore.Identity.AzureTable.Model;

namespace ASC.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
       public List<Claim> Claims { get; set; }
    }
}
