﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using NewProjectEpam.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Security.Claims;

namespace NewProjectEpam.Controllers
{
    public class AccountController : Controller
    {
        //для регистрации пользователей
        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }
        /*
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                 //   await UserManager.AddToRoleAsync(user.Id, "user");
                    
                  //  await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    Login("Index");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }
            return View(model);
        }
        */

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser { UserName = model.Email, Email = model.Email };
               IdentityResult result = await UserManager.CreateAsync(user, model.Password);
               //await UserManager.AddToRoleAsync(user.Id, "Пользователь");
                if (result.Succeeded)
                {
                    
                    ClaimsIdentity claim = await UserManager.CreateIdentityAsync(user,
                                           DefaultAuthenticationTypes.ApplicationCookie);
                 
                    AuthenticationManager.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = true
                    }, claim);
                    
                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }
            return View(model);
        }

        //для входа на сайт
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public ActionResult Login(string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await UserManager.FindAsync(model.Email, model.Password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Неверный логин или пароль.");
                }
                else
                {
                    ClaimsIdentity claim = await UserManager.CreateIdentityAsync(user,
                                            DefaultAuthenticationTypes.ApplicationCookie);
                    AuthenticationManager.SignOut();
                    AuthenticationManager.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = true
                    }, claim);
                    if (String.IsNullOrEmpty(returnUrl))
                        return RedirectToAction("Index", "Home");
                    return Redirect(returnUrl);
                }
            }
            ViewBag.returnUrl = returnUrl;
            return View(model);
        }

        //Редоктирование пользователей
        [HttpGet]
        public ActionResult Delete()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed()
        {
            ApplicationUser user = await UserManager.FindByEmailAsync(User.Identity.Name);
            if (user != null)
            {
                IdentityResult result = await UserManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    
                    LogOff();
                    return RedirectToAction("Index", "Home");
                }
            }
            return RedirectToAction("Index", "Home");
        }


        // POST: /Account/LogOff
         [HttpPost]
         [ValidateAntiForgeryToken]
         public ActionResult LogOff()
         {
             AuthenticationManager.SignOut();
             return RedirectToAction("Index", "Home");
         }

         [HttpGet]
         public ActionResult EditUsers()
         {

             return View();
         }

        


         //изменение  пользователей
         public async Task<ActionResult> Edit()
         {
             ApplicationUser user = await UserManager.FindByEmailAsync(User.Identity.Name);
             if (user != null)
             {
                 EditModel model = new EditModel { UserName = user.UserName, Email = user.Email };
                 return View(model);
             }
             return RedirectToAction("Login", "Account");
         }

         [HttpPost]
         public async Task<ActionResult> Edit(EditModel model)
         {
             ApplicationUser user = await UserManager.FindByEmailAsync(User.Identity.Name);
             if (user != null)
             {
                 user.Email = model.Email;
                 user.UserName = model.UserName;
                 IdentityResult result = await UserManager.UpdateAsync(user);
                 if (result.Succeeded)
                 {
                     return RedirectToAction("Index", "Home");
                 }
                 else
                 {
                     ModelState.AddModelError("", "Что-то пошло не так");
                 }
             }
             else
             {
                 ModelState.AddModelError("", "Пользователь не найден");
             }

             return View(model);
         }

        
 
         }
   
    
    }


       
   
 
    


