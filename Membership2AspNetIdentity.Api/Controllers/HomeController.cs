using System;
using System.Diagnostics;
using System.Web.Mvc;
using System.Web.Security;

namespace Membership2AspNetIdentity.Api.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("/GetClearTextPassword")]
        public ActionResult GetClearTextPassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException();
            }

            var user = Membership.GetUser(username);

            if (user == null)
            {
                return HttpNotFound();
            }


            var isLockedOut = user.IsLockedOut;
            if (isLockedOut)
            {
                // we can do this because of the migration
                user.UnlockUser();
            }

            try
            {
                var password = user.GetPassword();

                return Json(new
                {
                    Pex = false,
                    Password = password,
                    IsLockedOut = isLockedOut
                }, JsonRequestBehavior.AllowGet);
            }
            catch (System.Configuration.Provider.ProviderException ex)
            {
                Console.WriteLine(ex.Message);
                Debug.WriteLine(ex.Message);

                return Json(new
                {
                    Pex = true,
                    PexMessage = ex.Message,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
