using EmployeeManagement.Models;
using EmployeeManagement.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace EmployeeManagement
{
    public class Startup
    {

        private IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<AppDbContext>(options => // see in appsettings.json there is there "EmployeeDBConnection"
            options.UseSqlServer(_config.GetConnectionString("EmployeeDBConnection")));
            // adding identity services to our app-DataBase in order to give a user possibility to sign and to be recognize
            services.AddIdentity<ApplicationUser, IdentityRole>(options => //ApplicationUser is our extention class of ApplicationUser  
            // services.AddIdentity<IdentityUser, IdentityRole>(options => //this configure the requirements 
            {//about the password structure.
                options.Password.RequiredLength = 10;
                options.Password.RequiredUniqueChars = 3;
                options.Password.RequireNonAlphanumeric = false;
                options.SignIn.RequireConfirmedEmail = true; // this makes requirement for confirming email while registering
                options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation"; // registering the mechanism of our customed email token operation.
                options.Lockout.MaxFailedAccessAttempts = 5; // how many times a user can try to login before the account is locked
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // how long time the account is locked after the max failed attemps 
            })
             .AddEntityFrameworkStores<AppDbContext>()
             .AddDefaultTokenProviders() //a general provider of token which sent in user confirmation link (of email)
             .AddTokenProvider<CustomEmailConfirmationTokenProvider<ApplicationUser>>("CustomEmailConfirmation"); // building the mechanism of our customed email token operation and naming it.

            // Changes token lifespan of all token types
            services.Configure<DataProtectionTokenProviderOptions>(o =>
                    o.TokenLifespan = TimeSpan.FromHours(5));

            // Changes token lifespan of just the Email Confirmation Token type. this overrides the above configuration of the time span and makes specific time span for the email token.
            services.Configure<CustomEmailConfirmationTokenProviderOptions>(o => //
                    o.TokenLifespan = TimeSpan.FromDays(3));

            // this is another way to configure the requirements about the password , you can learn more in GitHub - https://github.com/aspnet/AspNetCore search there for 'PasswordOptions' class
            /*services.Configure<IdentityOptions>(options =>
           {
               options.Password.RequiredLength = 10;
               options.Password.RequiredUniqueChars = 3;
               options.Password.RequireNonAlphanumeric = false;
           });
            */
            services.AddAuthentication().AddGoogle(options =>
            { // we get these from https://console.developers.google.com/ in the credential page (click the edit sign to see these strings)
                options.ClientId = "547226157938-6i15b190cp9c4bjpqvfsiveb31o8ljtl.apps.googleusercontent.com";
                options.ClientSecret = "a85nxyF4ftWB-qUzGtSc1OEW";
                // options.CallbackPath = "";  // if we want to change the default back url after the external sign-in we can do this here
            })
                .AddFacebook(options =>
                {
                    options.AppId = "428628835219895";
                    options.AppSecret = "a2f8024228ea7e6b6b539a52ac63b399";
                });                       
            // creating authorization policy which requires user claims(authority to deal with roles) to make changes on roles
            services.AddAuthorization(options =>
            {
                options.AddPolicy("DeleteRolePolicy", // the name of the policy
                    policy => policy.RequireClaim("Delete Role")//the user need to have "Delete Role" claim if there is [Authorize(Policy = "DeleteRolePolicy")]  attribute near an action of a controller.   
                                    .RequireClaim("Create Role"));   // with adding this line , the user needs to have both claims to be authorizated.                                              //- it is like [Authorize(Roles =  "User")] attribute but there is a difference.
            });
            // this version has the flexibility of boolean combination. here we see "AND" + "OR" conditions 
            /*services.AddAuthorization(options =>
            {                        // .RequireAssertion(..) has a function parameter (Func) that can deal with such claims conditions and it (the parameter function) returns bool value that determines if the user gets the access
                options.AddPolicy("EditRolePolicy", policy => policy.RequireAssertion(context =>
                    context.User.IsInRole("Administrator") &&
                    context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                    context.User.IsInRole("Super Admin")
                ));
            });
              
            // these lines are the same as the above:
            services.AddAuthorization(options =>
            {
                options.AddPolicy("EditRolePolicy", policy =>
                policy.RequireAssertion(context => AuthorizeAccess(context)));
            });  // \/ the called function by the function parameter of RequireAssertion(..)
                 bool AuthorizeAccess(AuthorizationHandlerContext context)
            {
                  return context.User.IsInRole("Administrator") &&
                  context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                  context.User.IsInRole("Super Admin");
                }
            
            *//*services.AddAuthorization(options =>
            {
                options.AddPolicy("EditRolePolicy", policy => policy.RequireClaim("Edit Role"));
            });
            */
            services.AddAuthorization(options =>
            {
               options.AddPolicy("AdminRolePolicy", policy => policy.RequireRole("Administrator"/*, "User"*/));// this is another policy version. role is a kind of claim
            });
            /*
            services.AddAuthorization(options =>
            {
                options.AddPolicy("EditRolePolicy",
                    policy => policy.RequireClaim("Edit Role", "true")); //here is a second parameter needed to be true in order to get autho' (this needed mainly for better orginizing the view)
            });
            */
            services.AddAuthorization(options =>
            {
                options.AddPolicy("EditRolePolicy", policy =>
                    policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));
            });
            // dependency registration for the above policy handler
            services.AddSingleton<IAuthorizationHandler,
                CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();
            //another example of claim policy
            /*services.AddAuthorization(options =>
            {
                options.AddPolicy("AllowedCountryPolicy", 
                   policy => policy.RequireClaim("Country", "USA", "India", "UK")); // here are 2 kinds of parameters: "Country"(Type) and "USA", "India", "UK"(Value) 
            });
            */

            // here is an option to redirect the AccessDenied action/view from one path to another , without needing to seek for all the reffrences and to replace in the code
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            });
            //singlton makes one instance of MockEmployeeRepository-service throught the whole application
            //(like global variable) otherwise, we can get some different repository servises with seperate data (listen to https://www.youtube.com/watch?v=v6Nr7Zman_Y&feature=youtu.be) <//
            // I think , that by this registering the constructor of the MockEmpl' is initialized
            // /services.AddSingleton<IEmployeeRepository, MockEmployeeRepository>();
            //registereing once for scope , a new instanse of MockEm' can be created out of scope in the application
            services.AddScoped<IEmployeeRepository, MockEmployeeRepository>();
            // this register is of kind which makes a new instance of the reposiritory  each time it is occured
            // services.AddTransient<IEmployeeRepository, MockEmployeeRepository>();
            // or , registering the database repository:
           //services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();

            services.AddSingleton<DataProtectionPurposeStrings>(); // this registering is for injection the class within the application controllers (not clear to me yet, because here is only one part of the dependency)

            services.AddMvc(config => { //here I add a global Authorization demand, which means , the user must at least to log-in in order to use any action
                var policy = new AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });
            
            //services.AddMvc();
            // I use this method in case idon't need the Authorization Policy as above. 
            services.AddMvc(options => options.EnableEndpointRouting = false);
            //  MvcOptions.EnableEndpointRouting = false;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {                                             //@ /\ I am not sure if originallay is not IHostingEnvironment 
            if ( env.IsDevelopment()) //see the envinronment variable in launchSettings.json -> profiles -> IIS Express -> ASPNETCORE_ENVIRONMENT 
            {// in case of unmatch route we get the simple system expection page
                app.UseDeveloperExceptionPage();
            }
            else // these are 3 middlewares for error status code: you can replace and try
            {   //this middleware gets the wrong url and pass it on the pipeline till UseMvcWithDefaultRoute() which sends back 404, then from this line-
                // app.UseStatusCodePagesWithReExecute("/Error/{0}");  // - we directed to the controller. here , not as in the case of UseStatusCodePagesWithRedirects , the wrong url is  keeps showing 
                // UseStatusCodePages();              // - instead of /error/404  ; also , in the devtools page ->network-> the status code table there , there is there the reflection of the error (code 404 instead of 302)
                app.UseStatusCodePagesWithRedirects("/Error/{0}"); // this one of 3 middleware componentts of error handling directs to
                                                           //the route which occur in the parnthesis. the place holder {0} get the code number of the error , 404 , for example, then we ridirected
                                                                    // //to the route : controller=Error , action=404
               // app.UseExceptionHandler("/Error"); //this is for handling general error. it direct us to error controller 
            }
            app.UseRouting();
            app.UseStaticFiles();
            app.UseAuthentication(); // It is important to set this line before UseMvc.. method 
            app.UseMvcWithDefaultRoute();// creating mvc including the default route {controller}/{action}/{parameter}// 
           /*app.Run(async context =>
            {
                await context.Response.WriteAsync("Hello, World!");
            }); */
            /*here is extention method of IApplicationBuilder to add mvc include default  route
             it does not work here.
            public static IApplicationBuilder UseMvcWithDefaultRoute(this IApplicationBuilder app)
            {
                if (app == null)
                {
                    throw new ArgumentNullException(nameof(app));
                }

                    return app.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");
                });
            }
            //customized default route template. the '?' is for optional.
            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
            // ControllerActionEndpointConventionBuilder controllerActionEndpointConventionBuilder = app.UseEndpoints.MapDefaultControllerRoute();
            //this is called by the main function (in Program.cs), it is performed if the mvc is not used
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    //   throw new Exception("This is a Middleware expection.");
                    await context.Response
                    .WriteAsync("Tname of the operating envinronment is:   " + System.Diagnostics.Process.GetCurrentProcess().ProcessName);
                    await context.Response
                    .WriteAsync("   \n Hello World");

                });
            });*/
        }
    }
}
