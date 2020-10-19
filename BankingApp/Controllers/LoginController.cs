﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankingApp.Models;
using BankingApp.Entities.Models;
using BankingApp.Entities.Services.Interfaces;

namespace BankingApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUsersService _usersService;
        public LoginController(IUsersService usersService)
        {
            _usersService = usersService;
        }

        /// <summary>
        /// The View containing the LoginStageOne form with a modal for signing up with a new account
        /// </summary>
        /// <returns>The View containing the Login form</returns>
        [AllowAnonymous]
        [HttpGet]
        public ActionResult LoginStageOne()
        {
            //If user is already logged in then redirect them to the Home page
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { Area = "Home" });
            }
            return View(new LoginStageOneViewModel());
        }

        /// <summary>
        /// Receives User details, validates them and logs the user in with those details if successful
        /// </summary>
        /// <param name="viewModel">The viewModel containing the user details to validate</param>
        /// <returns>Redirects to the Home View in the Home Area if successful, otherwise returns the LoginStageOne View</returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> LoginStageOne(LoginStageOneViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(new LoginStageOneViewModel());
            }

            User currentUser = _usersService.CheckUserDetails(viewModel.Email, viewModel.Password);
            if (currentUser == null)
            {
                ModelState.AddModelError("Incorrect", "Email or Password are incorrect");
                return View(new LoginStageOneViewModel());
            }

            var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, currentUser.Username),
                        new Claim("UserId", currentUser.UserId.ToString()),
                        new Claim(ClaimTypes.Role, currentUser.Role)
                    };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = true,
                IssuedUtc = DateTime.Now
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("Index", "Home", new { Area = "Home" });
        }

        [AllowAnonymous]
        [HttpGet]
        public ViewResult SignUp()
        {
            return View();
        }

        /// <summary>
        /// Receives User details for a new account and passes them to a service which 
        /// checks them to make sure they don't match the details of an existing account. 
        /// If they don't, create a new account.
        /// </summary>
        /// <param name="viewModel">The viewModel containing the User details for a new account</param>
        /// <returns>The LoginStageOne View</returns>
        [AllowAnonymous]
        [HttpPost]
        public ViewResult SignUp(SignUpViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (!_usersService.CreateNewAccount(viewModel.Username, viewModel.Email, viewModel.Password))
                {
                    ModelState.AddModelError("AccountAlreadyExists", "Email or Username already in use.");
                }
                else
                {
                    return View("LoginStageOne", new LoginStageOneViewModel
                    {
                        AccountCreated = true
                    });
                }
            }

            viewModel.Password = "";
            return View(viewModel);
        }

        /// <summary>
        /// Logs the user out
        /// </summary>
        /// <returns>The LoginStageOne View</returns>
        [Authorize]
        [HttpGet]
        public RedirectToActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("LoginStageOne");
        }
    }
}