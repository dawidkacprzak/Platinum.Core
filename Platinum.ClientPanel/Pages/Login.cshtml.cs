using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Platinum.Core.DatabaseIntegration;

namespace Platinum.ClientPanel.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        public string ReturnUrl { get; set; }
        public static string Error { get; set; }

        public async Task<IActionResult>
            OnGetAsync(string paramUsername, string paramPassword)
        {
            string returnUrl = Url.Content("~/");
            try
            {
                // Clear the existing external cookie
                await HttpContext
                    .SignOutAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch
            {
            }

            // *** !!! This is where you would validate the user !!! ***
            // In this example we just log the user in
            // (Always log the user in for this demo)
            int userId = 0;
            try
            {
                if (paramPassword == null || paramUsername == null)
                {
                    throw new AuthenticationException("Nie znaleziono użytkownika o danym loginie i haśle");
                }

                using (Dal db = new Dal())
                {
                    if (paramUsername.Contains('-') || paramUsername.Contains('\'') || paramUsername.Contains('\"') ||
                        paramPassword.Contains('-') || paramPassword.Contains('\'') || paramPassword.Contains('\"'))
                    {
                        throw new AuthenticationException("Nie znaleziono użytkownika o danym loginie i haśle");
                    }

                    int fetchedUsers = (int) db.ExecuteScalar(
                        $"SELECT COUNT(*) From WebApiUsers WHERE Login = '{paramUsername}' and Password = '{paramPassword}'");
                    if (fetchedUsers != 1)
                    {
                        throw new AuthenticationException("Nie znaleziono użytkownika o danym loginie i haśle");
                    }
                    else
                    {
                        userId = (int) db.ExecuteScalar(
                            $"SELECT Id From WebApiUsers WHERE Login = '{paramUsername}' and Password = '{paramPassword}'");
                        if (userId == 0)
                        {
                            throw new AuthenticationException(
                                "Błąd serwera - skontaktuj się z administratorem serwisu");
                        }
                    }
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, paramUsername),
                    new Claim(ClaimTypes.Role, "User"),
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                };
                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    RedirectUri = this.Request.Host.Value
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
            }
            catch (AuthenticationException ex)
            {
                Error = ex.Message;
            }
            catch (Exception ex)
            {
                Error = "Błąd po stronie serwera - spróbuj ponownie później";
            }

            return LocalRedirect(returnUrl);
        }
    }
}