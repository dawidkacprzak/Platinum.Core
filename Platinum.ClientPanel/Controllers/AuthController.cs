using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace Platinum.ClientPanel.Controllers
{
    public static class AuthController
    {
        
        public static bool IsUserAuthenticated(AuthenticationStateProvider provider)
        {
            
            var authState = provider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
            var user = authState.User;
            if (user.Identity.IsAuthenticated)
            {
                Claim userIdClaim = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    int userId = int.Parse(userIdClaim.Value);
                    if (userId == 0)
                    {
                        throw new Exception("Wystąpił błąd podczas odczytu danych użytkownika");
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        public static int GetAuthenticatedUserId(AuthenticationStateProvider provider)
        {
            var authState = provider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
            var user = authState.User;
            if (user.Identity.IsAuthenticated)
            {
                Claim userIdClaim = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    int userId = int.Parse(userIdClaim.Value);
                    if (userId == 0)
                    {
                        throw new Exception("Wystąpił błąd podczas odczytu danych użytkownika");
                    }
                    else
                    {
                        return userId;
                    }
                }
            }

            throw new Exception("Uzytkownik nie jest zalogowany.");
        }
    }
}