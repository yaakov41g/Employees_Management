using EmployeeManagement.Models;
using EmployeeManagement.Security;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;

namespace EmployeeManagement.Controllers
{
    [Authorize]// this demands some authorization in order to operate this controller , expect the actins with '[AllowAnonymous]' 
    public class HomeController : Controller           //- in this case the demands is minimal , that is the user must log-in in order to create or edit.
    {                                               //- the user will be redirected to ' account/Login' route in order to log-in.
        private readonly IEmployeeRepository _employeeRepository;
        [Obsolete]
        private readonly IHostingEnvironment hostingEnvironment;
        // It is through IDataProtector interface Protect and Unprotect methods,
        // we encrypt and decrypt respectively
        private readonly IDataProtector protector;
        // It is the CreateProtector() method of IDataProtectionProvider interface
        // that creates an instance of IDataProtector. CreateProtector() requires
        // a purpose string. So both IDataProtectionProvider and the class that
        // contains our purpose strings are injected using the contructor
        [Obsolete]
        public HomeController(IEmployeeRepository employeeRepository, IDataProtectionProvider dataProtectionProvider,
                              DataProtectionPurposeStrings dataProtectionPurposeStrings, IHostingEnvironment hostingEnvironment)
        {// injection of the repository into the controller. The registration of 'employeeRepository' dependency is created in Startup.cs by 'AddScoped' method.
            _employeeRepository = employeeRepository; //@= new MockEmployeeRepository();
           // Pass the purpose string as a parameter
            this.protector = dataProtectionProvider.CreateProtector(
                dataProtectionPurposeStrings.EmployeeIdRouteValue);
            this.hostingEnvironment = hostingEnvironment;
        }
        [AllowAnonymous] // this is exemption from authorization for this action , so the user can get to this place by the url route.
        public ViewResult Index()
        {
            // retrieve all the employees
            var model = _employeeRepository.GetAllEmployees()
                 .Select(e =>  // I think  that 'select' takes all the EmployeeS in the List , transforms the 'EncryptedId' of each and "returns"  the transformed Employee List object
                 {
                     // Encrypt the ID value and store in EncryptedId property
                     e.EncryptedId = protector.Protect(e.Id.ToString());
                     return e;
                 }); 
            // Pass the list of employees to the view
            return View(model);
        }
        /*        public ViewResult Details(int id)
                {
                    Employee model = _employeeRepository.GetEmployee(id);
                    return View(model);
                } */
        /*[AllowAnonymous]
        public ViewResult Details(int id) // The non encripted Id version
        {
            //throw new Exception("Error in Details View"); //@ temp line in order to see the log  file in 'C:\DemoLogs'
            Employee employee = _employeeRepository.GetEmployee(id);
            if (employee == null)
            {
                Response.StatusCode = 404;  // I think this return the error message to the client's page's developemt (F12)  
                return View("EmployeeNotFound", id);
            } 
            // Instantiate HomeDetailsViewModel and store Employee details and PageTitle
            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {// constructor
                Employee = employee, //  _employeeRepository.GetEmployee(id),
                PageTitle = "Employee Details" // id.ToString()
            };
           
            // Pass the ViewModel object to the View() helper method
            return View(homeDetailsViewModel);
        }
        *///throw new Exception("Error in Details View"); //@ temp line in order to see the log  file in 'C:\DemoLogs'


        // Details view receives the encrypted employee ID
        [AllowAnonymous]
            public ViewResult Details(string id)
            {
                // Decrypt the employee id using Unprotect method
                string decryptedId = protector.Unprotect(id);
                int decryptedIntId = Convert.ToInt32(decryptedId); // converting string to int

                Employee employee = _employeeRepository.GetEmployee(decryptedIntId);
                if (employee == null)
            {
                Response.StatusCode = 404;  // I think this return the error message to the client's page's developemt (F12)  
                return View("EmployeeNotFound", id);
            }
            // Instantiate HomeDetailsViewModel and store Employee details and PageTitle
            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {// constructor
                Employee = employee, //  _employeeRepository.GetEmployee(id),
                PageTitle = "Employee Details" // id.ToString()
            };

            // Pass the ViewModel object to the View() helper method
            return View(homeDetailsViewModel);
        }
        [Obsolete]// this method adds the loaded picture to wwwroot\images folder by creating the path
        private string ProcessUploadedFile(EmployeeCreateViewModel model) // this is not action method,it is only a helper , called by other function here.
        {
            string uniqueFileName = null;

            if (model.Photo != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                //@ to prevent error occured as the FileStream remains not lockeded we need 'using' .
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Photo.CopyTo(fileStream);
                }
            }

            return uniqueFileName;
        }
        [HttpGet] // attribute to differ this 'create' function from the next 'create' ,  and to say 
                  // it will called through Get method (the url)
                  // this create function is called by the Menu Create and display the Create form\
        public ViewResult Create()
        {
            return View();
        }

           //@@@@@@@@@@@@@@@@@ not to erase the comments on this page  @@@@@@@@@@@@@@@
        [HttpPost]// attribute to differ this 'create' function from the next 'create' ,  and to say 
                  // it will called through Post method 
        [Obsolete]
        //@ this is called by the form  Create button// TREATS form with 2 photoes upload control
        //@ViewResult and RedirectToAction types both are inherited from IActionResult Interface
        //@therefore the return type of this action is IActionResult
        /*public IActionResult Create(EmployeeCreateViewModel model)
       {// check if there is validation between the form details and the model properies  
          if (ModelState.IsValid)
           {
               string uniqueFileName = null;
               // If the Photos property on the incoming model object is not null and if count > 0,
               // then the user has selected at least one file to upload

               if (model.Photos != null && model.Photos.Count > 0)
               {
                   // Loop thru each selected file
                   foreach (IFormFile photo in model.Photos)
                   {

                       // The image must be uploaded to the images folder in wwwroot
                       // To get the path of the wwwroot folder we are using the inject
                       // HostingEnvironment service provided by ASP.NET Core //@ maybe the registering of the HostingEnvironment dependency is done
                       //in the HostingEnvironment implementing class //@ .WebRootPath returns the path to wwwroot folder , Path,Combine returns the
                       string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images"); // full path include Images folder
                                                                                                      //# - restore back after end with the example of loadind 2 files       // To make sure the file name is unique we are appending a new
                                                                                                      // GUID value and and an underscore to the file name
                                                                                                      //# uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photos.FileName;
                       uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                       string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                       // Use CopyTo() method provided by IFormFile interface to
                       // copy the file to wwwroot/images folder
                       photo.CopyTo(new FileStream(filePath, FileMode.Create));
                   }
               }

               Employee newEmployee = new Employee
               {
                   Name = model.Name,
                   Email = model.Email,
                   Department = model.Department,
                   // Store the file name in PhotoPath property of the employee object
                   // which gets saved to the Employees database table
                   PhotoPath = uniqueFileName
               };

               _employeeRepository.Add(newEmployee);
               return RedirectToAction("details", new { id = newEmployee.Id });
               //@ maybe the Emplioyee Id is getting automaticaly from  the binded database 
               // Id , incrementaly
           }
           return View();*/
        [HttpPost]//this attribute says the function will be called through Post method
        //@ this is called by the form  Create button//
        //@ViewResult and RedirectToAction types both are inherited from IActionResult Interface
        //@therefore the return type of this action is IActionResult
        public IActionResult Create(EmployeeCreateViewModel model)
        {// check if there (is validation between the form details and the model properies) are no validation errors
            if (ModelState.IsValid)
            {
                string uniqueFileName = ProcessUploadedFile(model); 
                /*
                 // If the Photo property on the incoming model object is not null, then the user
                 // has selected an image to upload.
                 if (model.Photo != null)
                 {
                     // The image must be uploaded to the images folder in wwwroot
                     // To get the path of the wwwroot folder we are using the inject
                     // HostingEnvironment service provided by ASP.NET Core
                     string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                     // To make sure the file name is unique we are appending a new
                     // GUID value and and an underscore to the file name
                     uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                     string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                     // Use CopyTo() method provided by IFormFile interface to
                     // copy the file to wwwroot/images folder
                     model.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
                 }
                */
                Employee newEmployee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    // Store the file name in PhotoPath property of the employee object
                    // which gets saved to the Employees database table
                    PhotoPath = uniqueFileName
                };

                _employeeRepository.Add(newEmployee);
                return RedirectToAction("details", new { id = newEmployee.Id });
            }
            // if there are validation errors then this view is returned to refulfill the form by the user
            return View();
        }
        [HttpGet]
        public ViewResult Edit(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            EmployeeEditViewModel employeeEditViewModel = new EmployeeEditViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotoPath = employee.PhotoPath
            };
            return View(employeeEditViewModel);
        }
        // Through model binding, the action method parameter
        // EmployeeEditViewModel receives the posted edit form data
        [HttpPost]
        [Obsolete]
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            // Check if the provided data is valid, if not rerender the edit view
            // so the user can correct and resubmit the edit form
            if (ModelState.IsValid)
            {
                // Retrieve the employee being edited from the database
                Employee employee = _employeeRepository.GetEmployee(model.Id);


                // Update the employee object with the data in the model object
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;

                // If the user wants to change the photo, a new photo will be
                // uploaded and the Photo property on the model object receives
                // the uploaded photo. If the Photo property is null, user did
                // not upload a new photo and keeps his existing photo
                if (model.Photo != null)
                {
                    // If a new photo is uploaded, the existing photo must be
                    // deleted. So check if there is an existing photo and delete
                    if (model.ExistingPhotoPath != null)
                    {
                        string filePath = Path.Combine(hostingEnvironment.WebRootPath,
                            "images", model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath);
                    }
                    // Save the new photo in wwwroot/images folder and update
                    // PhotoPath property of the employee object which will be
                    // eventually saved in the database
                    employee.PhotoPath = ProcessUploadedFile(model);
                }

                // Call update method on the repository service passing it the
                // employee object to update the data in the database table
                Employee updatedEmployee = _employeeRepository.Update(employee);

                return RedirectToAction("index");
            }

            return View(model);
        }
    }
}







