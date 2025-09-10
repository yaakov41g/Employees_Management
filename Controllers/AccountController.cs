using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using EmployeeManagement.Models;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System;

namespace EmployeeManagement.Controllers
{
    public class AccountController : Controller
    {// private readonly UserManager<IdentityUser> userManager; // IdentityUser was the model we used in the project before we extended it to ApplicationUser 
        private readonly UserManager<ApplicationUser > userManager;
        private readonly SignInManager<ApplicationUser > signInManager;
        private readonly ILogger<AccountController> logger;

        public AccountController(UserManager<ApplicationUser > userManager, 
            SignInManager<ApplicationUser > signInManager, ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
            //@@@ I commented this action because I inserted the login action of the external , see ahead.
        }     //  Because I set the whole application to 'authorization demand' (in Startup.cs /-ConfigureServices /-services.AddMvc(config => var policy...
       /* [AllowAnonymous] //- , I must free this action from authorization demand, so the user can get here to log-in. else , we get endless redirection chain in the url ,endig with error.
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
       */
        

        [AllowAnonymous]// see the remark above
        [HttpPost]  // we came here when 'Login' button is clicked in 'Login' view (Login.cshtml)
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl) // the 'returnUrl' parameter appreas on the url (in the address bar) when the user tried to to 'Create' or other action
        {                                //- without being logging in. ASP binds these identical parameters so we can use the value of it in the action here (I mean: in the method here)
            // I inserted this line here in order to prevent 'non refference' error in Login.cshtml :  line - [if (Model.ExternalLogins.Count == 0)]
            model.ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {  // here we check if the user has confirmed his email (by link confirmation that he got in his email inbox)
                var user = await userManager.FindByEmailAsync(model.Email);//- we also check here if he filled the password correctly to prevent a little option of  account enumeration and brute force attacks. 
                if (user != null && !user.EmailConfirmed &&  //there is configuration of this 'EmailConfirmed' option in Startup.cs/ConfigureServices(..)
                            (await userManager.CheckPasswordAsync(user, model.Password)))
                {
                    ModelState.AddModelError(string.Empty, "Email not confirmed yet");
                    return View(model);
                }
                // The last boolean parameter lockoutOnFailure indicates if the account
                // should be locked on failed logon attempt. On every failed logon
                // attempt AccessFailedCount column value in AspNetUsers table is
                // incremented by 1. When the AccessFailedCount reaches the configured
                // MaxFailedAccessAttempts which in our case is 5, the account will be
                // locked and LockoutEnd column is populated. After the account is
                // lockedout, even if we provide the correct username and password,
                // PasswordSignInAsync() method returns Lockedout result and the login
                // will not be allowed for the duration the account is locked.
                var result = await signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, true); // , false);

                if (result.Succeeded)
                {                                         //  alternative way \/
                    if ( !string.IsNullOrEmpty(returnUrl)) // if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl)
                                                           //- else return RedirectToAction(....
                    { // if we want to prevent 'result url' attack we need to use 'LocalRedirect' , so if an attacker deceits a user (by email he sends to him with a link to your application)
                        return LocalRedirect(returnUrl); //- and redirects him to his site which looks the same as your log-in page, then, to prevent we ne to asure to redirect  to a local url
                                                         // there is a minus inthe above solution by that if the url is not local the user gets exception, and we need to deal with that.
                                                         //- as a alternation we can check the url : see the comment after if ( !string.  ...


                    }
                    else
                        return RedirectToAction("index", "home");
                }
                // If account is lockedout send the use to AccountLocked view
                if (result.IsLockedOut)
                {
                    return View("AccountLocked");
                }
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            }

            return View(model);
        }
        [HttpGet]  // we came here when 'Login' menu is clicked, as you can see in _Layout.cshtml file.
        [AllowAnonymous]  //- or, automaticaly when the user trys to use 'Create' menu without being logging in.
        public async Task<IActionResult> Login(string returnUrl) // ASP binds the  user's goal url to this 'returnUrl' parameter (if the user came here directly by clicking 'Login') menu then the value of 'returnUrl' is null.
        {
            LoginViewModel model = new LoginViewModel
            {
                ReturnUrl = returnUrl, // the url to redirect the user back after its connection to our app by google login. 
                ExternalLogins =  // the list of the external login providers which registered in our application (like google etc.)
                (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View(model);
        }
        /*[AllowAnonymous]
        [HttpPost] // I will explain this action more later
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account",
                                new { ReturnUrl = returnUrl });
            var properties = signInManager
                .ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }
        */
        [AllowAnonymous]
        [HttpPost]  // we came here when one of the external authenticators buttons is clicked in 'Login' view (it is trigered in Login.cshtml) 
        public IActionResult ExternalLogin(string provider, string returnUrl) // here 'provider' binded to 'provider' name of the button , 'returnUrl' binded by 'asp-route-returnUrl'
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account",  // here the function name to which we come back after the user confirmed his details on the external (google etc) dialog board.
                                new { ReturnUrl = returnUrl });
            var properties = signInManager
                .ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties); // this line call the login page of the external(google etc)
        }
        [AllowAnonymous]
        public async Task<IActionResult> // this function is registered in IActionResult ExternalLogin(see there)
            ExternalLoginCallback(string returnUrl = null, string remoteError = null) 
        {
            returnUrl = returnUrl ?? Url.Content("~/"); // if returnUrl is null then make it to the base application view (/#_=_)

            LoginViewModel loginViewModel = new LoginViewModel
            {  // this fullfilling is needed if there is an error and we need to return to the Login view and to display the external authenticators (google etc), for example.
                ReturnUrl = returnUrl,
                ExternalLogins =
                        (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState// I think "string.Empty" is same as: ""
                    .AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                
                return View("Login", loginViewModel);
            }

            // Get the login information about the user from the external login provider
            var info = await signInManager.GetExternalLoginInfoAsync(); // you can see details by setting here a breakpoint and run with debug
            if (info == null)
            {
                ModelState
                    .AddModelError(string.Empty, "Error loading external login information.");

                return View("Login", loginViewModel);
            }
            // Get the email claim from external login provider (Google, Facebook etc)
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            ApplicationUser user = null;
            // here we check only if the user's email still not confirmed.
            if (email != null)
            {
                // Find the user
                user = await userManager.FindByEmailAsync(email);

                // If email is not confirmed, display login view with validation error
                if (user != null && !user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "Email not confirmed yet");
                    return View("Login", loginViewModel);
                }
            }
            // If the user already has a login (i.e if there is a record in AspNetUserLogins
            // table) then sign-in the user with this external login provider
            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)  // if the user has earlier registered and confirmed email and now he is only signing in 
            {
                return LocalRedirect(returnUrl);
            }
            // If there is no record in AspNetUserLogins table, the user may not have
            // a local account     @@@ even if I don't think this is needed because the exsternal has all that needed , anyway in our app it is needed
            else  // there can be some kinds of error : the user didn't fill email , or this is his first time to sign (indeed- to register) and we need to create him and fill his record in dba.AspNetUsers
            {
                // Get the email claim value  (the email address)
                // var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                if (email != null)
                {
                    // Create a new user without password if we do not have a user already
                   // var user = await userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };

                        await userManager.CreateAsync(user); // this method adds line in AspNetUserRoles table
                                                             // After a local user account is created, generate and log the
                                                             // email confirmation link
                        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                        var confirmationLink = Url.Action("ConfirmEmail", "Account",
                                        new { userId = user.Id, token = token }, Request.Scheme);

                        logger.Log(LogLevel.Warning, confirmationLink);  // see the newest file in C:\DemoLogs. copy the newes url there and paste it to 

                        ViewBag.ErrorTitle = "Registration successful";
                        ViewBag.ErrorMessage = "Before you can Login, please confirm your " +
                            "email, by clicking on the confirmation link we have emailed you";
                        return View("Error");
                    }

                    
                    await userManager.AddLoginAsync(user, info);// Add a login (i.e insert a row for the user in AspNetUserLogins table)
                    await signInManager.SignInAsync(user, isPersistent: false);

                    return LocalRedirect(returnUrl);
                }

                // If we cannot find the user email we cannot continue
                ViewBag.ErrorTitle = $"Email claim not received from: {info.LoginProvider}";
                ViewBag.ErrorMessage = "Please contact support on Pragim@PragimTech.com";

                return View("Error");
            }
        }
        [AllowAnonymous]
        [HttpGet] 
        public IActionResult Register()
        {
            return View();
        }
        [AcceptVerbs("Get", "Post")]  // allows also 'get' and also 'post' requests (similar as the syntax:[HttpGet][HttpPost])
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailInUse(string email)
        {  // find if TYPED email is already exits. the request is made by ajax request which invoked by the Remote method-attribute on the 'Email' field
            var user = await userManager.FindByEmailAsync(email);  //-  of registerviewmodel

            if (user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Email {email} is already in use.");
            }
        }
            [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        { // ModelState will give the error remarks onto the form if the model fields don't fit the criteries /not valid.
            if (ModelState.IsValid)
            {
                // Copy data from RegisterViewModel to ApplicationUser 
                var user = new ApplicationUser 
                {
                    UserName = model.Email,
                    Email = model.Email,
                    City = model.City
                };

                // Store user data in AspNetUsers database table
                var result = await userManager.CreateAsync(user, model.Password);// in this signature of the method there is  a password indeed

                // If user is successfully created, sign-in the user using
                // SignInManager and redirect to index action of HomeController
                if (result.Succeeded)
                {  // creating a token which will be sent within the confirmation link.
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, token = token }, Request.Scheme);
                    // meanwhile , instead of sending mail with the confirmation link , we using logging it to a file. (the newest file in C:\DemoLogs)
                    logger.Log(LogLevel.Warning, confirmationLink);
                    // If the user is signed in and in the Admin role, then it(he/she) is
                    // the Admin user that is creating a new user. So redirect the
                    // Admin user to ListRoles action and not change the Log menu to  the new user but staing with the admin name (when I say Log menu I mean to the Login/out 
                    if (signInManager.IsSignedIn(User) && User.IsInRole("Administrator")) //- on the right side of the menu). Pay attantion to 'User' with uppercase 'U' which is a member of ControllerBase class , this User is apparently connected with "ApplicationUser : IdentityUser" model.
                    {
                        /* tempoarily commented  return RedirectToAction("ListUsers", "Administration");*/
                        //these lines tempoarily added to display Error view when the mail is still not confirmed
                        ViewBag.ErrorTitle = "Hello Admin !  Registration successful";
                        ViewBag.ErrorMessage = "Before you can Login, please confirm your " +
                                "email, by clicking on the confirmation link we have emailed you";
                        return View("Error");
                    }
                    // I think the goal of this line is to exchange between Logged status and Login menu items of the new user.
                    /* tempoarily commented await signInManager.SignInAsync(user, isPersistent: false);// if we want to adopt a non permanent cookie then 'isPersistent= false' // 
                    return RedirectToAction("index", "home");*/
                    //these lines tempoarily added to display Error view when the mail is still not confirmed (of non administrator)
                    ViewBag.ErrorTitle = "Registration successful";
                    ViewBag.ErrorMessage = "Before you can Login, please confirm your " +
                            "email, by clicking on the confirmation link we have emailed you";
                    return View("Error");
                }

                // If there are any errors, add them to the ModelState object
                // which will be displayed by the validation summary tag helper
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("index", "home");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"The User ID {userId} is invalid";
                return View("NotFound");
            }

            var result = await userManager.ConfirmEmailAsync(user, token); // this changes the flag of Confirmed Email to 'true' in dba.aspnetUser table record
            if (result.Succeeded)
            {
                return View();
            }

            ViewBag.ErrorTitle = "Email cannot be confirmed";
            return View("Error");
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = await userManager.FindByEmailAsync(model.Email);
                // If the user is found AND Email is confirmed
                if (user != null && await userManager.IsEmailConfirmedAsync(user))
                {
                    // Generate the reset password token
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);

                    // Build the password reset link
                    var passwordResetLink = Url.Action("ResetPassword", "Account",
                            new { email = model.Email, token = token }, Request.Scheme);

                    // Log the password reset link
                    logger.Log(LogLevel.Warning, passwordResetLink);

                    // Send the user to Forgot Password Confirmation view
                    return View("ForgotPasswordConfirmation");
                }

                // To avoid account enumeration and brute force attacks, don't
                // reveal that the user does not exist or is not confirmed
                return View("ForgotPasswordConfirmation");
            }

            return View(model);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            // If password reset token or email is null, most likely the
            // user tried to tamper the password reset link
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = await userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    // reset the user password
                    var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        // Upon successful password reset and if the account is lockedout, set
                        // the account lockout end date to current UTC date time, so the user
                        // can login with the new password
                        if (await userManager.IsLockedOutAsync(user))
                        {
                            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                        }
                        return View("ResetPasswordConfirmation");
                    }
                    // Display validation errors. For example, password reset token already
                    // used to change the password or password complexity rules not met
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

                // To avoid account enumeration and brute force attacks, don't
                // reveal that the user does not exist
                return View("ResetPasswordConfirmation");
            }
            // Display validation errors if model state is not valid
            return View(model);
        }
        /*[HttpGet] // we came here from the Manage/Password rolled menu. this version had been used before we added 'AddPassword' Actions
        public IActionResult ChangePassword()
        {
            return View();
        }
        */
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await userManager.GetUserAsync(User); // the logginned user

            var userHasPassword = await userManager.HasPasswordAsync(user);

            if (!userHasPassword)
            {
                return RedirectToAction("AddPassword");
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null)  // if there is no user's record in dbase
                {
                    return RedirectToAction("Login");
                }

                // ChangePasswordAsync changes the user password
                var result = await userManager.ChangePasswordAsync(user,
                    model.CurrentPassword, model.NewPassword);

                // The new password did not meet the complexity rules or  //@@ I don't understand why these checks are not rendered while the first line [if (ModelState.IsValid)]
                // the current password is incorrect. Add these errors to
                // the ModelState and rerender ChangePassword view
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                // Upon successfully changing the password refresh sign-in cookie
                await signInManager.RefreshSignInAsync(user);
                return View("ChangePasswordConfirmation");
            }

            return View(model);
        }
        [HttpGet] // these Actions add local password in addition to the external password (I don't know why it's needed) 
        public async Task<IActionResult> AddPassword()
        {
            var user = await userManager.GetUserAsync(User);

            var userHasPassword = await userManager.HasPasswordAsync(user);

            if (userHasPassword)
            {
                return RedirectToAction("ChangePassword"); // the user tried to add local password which already exists , so he can change it
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPassword(AddPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);

                var result = await userManager.AddPasswordAsync(user, model.NewPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                await signInManager.RefreshSignInAsync(user);

                return View("AddPasswordConfirmation");
            }

            return View(model);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
