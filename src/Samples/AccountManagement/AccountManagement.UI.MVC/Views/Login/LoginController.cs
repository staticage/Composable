﻿using AccountManagement.API;
using Composable.Messaging;
using Composable.Messaging.Buses;
using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.UI.MVC.Views.Login
{
    public class LoginController : ControllerBase
    {
        readonly IRemoteApiNavigatorSession _bus;
        public LoginController(IRemoteApiNavigatorSession remoteApiNavigator) => _bus = remoteApiNavigator;

        public IActionResult Login(AccountResource.Command.LogIn loginCommand)
        {
            if(!ModelState.IsValid) return View("LoginForm");

            var result = loginCommand.PostOn(_bus);
            if(result.Succeeded)
            {
                return View("LoggedIn");
            }

            ModelState.AddModelError("Something", "Login Failed");
            return RedirectToAction("LoginForm", new { email = loginCommand.Email });
        }

        public IActionResult LoginForm(string email = "") => View("LoginForm", _bus.Navigate(Api.Accounts.Command.Login()).WithValues(email, ""));
    }
}
