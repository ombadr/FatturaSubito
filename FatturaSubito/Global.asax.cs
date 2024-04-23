using FatturaSubito.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace FatturaSubito
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        DBContext db = new DBContext();
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("it-IT");
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo("it-IT");
        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                int utenteId;

                if (int.TryParse(HttpContext.Current.User.Identity.Name, out utenteId))
                {
 
                    if (db == null)
                    {
                        db = new DBContext();
                    }

                    var loggedUser = db.Utente.Find(utenteId);

                    if (loggedUser != null)
                    {

                        if (HttpContext.Current.Session != null)
                        {
                            HttpContext.Current.Session["NomeUtente"] = loggedUser.Nome + " " + loggedUser.Cognome;
                            HttpContext.Current.Session["Logo"] = loggedUser.Logo;
                        }
                    }
                }
            }
        }


    }
}
