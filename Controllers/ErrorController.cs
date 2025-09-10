using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
    public class ErrorController : Controller
    {  // this is just a teacher's example for handling a general expection , but this is not a needed part of  the program. we directed here by
        // - app.UseExceptionHandler("/Error") from startup\configure(). to invoke this ,decomment 'throw expection' line in HomeController/Details
        [AllowAnonymous]
        [Route("Error")]
        public IActionResult Error()
        {
            // Retrieve the exception Details
            var exceptionHandlerPathFeature =
                    HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            ViewBag.ExceptionPath = exceptionHandlerPathFeature.Path;
            ViewBag.ExceptionMessage = exceptionHandlerPathFeature.Error.Message;
            ViewBag.StackTrace = exceptionHandlerPathFeature.Error.StackTrace;

            return View("Error");
        }
        // If there is 404 status code, the route path will become Error/404
        [Route("Error/{statusCode}")]// statusCode is a place holder
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult = // element that holds data of reexecuted (after correcting) the error response for the user
                            HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            switch (statusCode)
            {
                case 404:
                case 500:
                    ViewBag.ErrorMessage = "Sorry, the resource you requested could not be found";
                    ViewBag.Path = statusCodeResult.OriginalPath; // the not found url
                    ViewBag.QS = statusCodeResult.OriginalQueryString; // attached query to the url , if there is (ex': url/?a=56) 
                    break;
            }

            return View("NotFound");
        }
    }
}
