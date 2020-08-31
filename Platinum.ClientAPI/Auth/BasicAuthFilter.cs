using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.DependencyInjection;
using Platinum.Core.DatabaseIntegration;

namespace Platinum.ClientAPI.Auth
{
public class BasicAuthFilter : IAuthorizationFilter
{
    private readonly string _realm;

    public BasicAuthFilter(string realm)
    {
        _realm = realm;
        if (string.IsNullOrWhiteSpace(_realm))
        {
            throw new ArgumentNullException(nameof(realm), @"Please provide a non-empty realm value.");
        }
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        try
        {
            string authHeader = context.HttpContext.Request.Headers["Authorization"];
            if (authHeader != null && _realm != null)
            {
                var authHeaderValue = AuthenticationHeaderValue.Parse(authHeader);
                if (authHeaderValue.Scheme.Equals(AuthenticationSchemes.Basic.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    var credentials = Encoding.UTF8
                                        .GetString(Convert.FromBase64String(authHeaderValue.Parameter ?? string.Empty))
                                        .Split(':', 2);
                    if (credentials.Length == 2)
                    {
                        if (IsAuthorized(context, credentials[0], credentials[1]))
                        {
                            return;
                        }
                    }
                }
            }

            ReturnUnauthorizedResult(context);
        }
        catch (FormatException)
        {
            ReturnUnauthorizedResult(context);
        }
    }

    public bool IsAuthorized(AuthorizationFilterContext context, string username, string password)
    {
        using (Dal db = new Dal())
        {
            using (var reader = db.ExecuteReader(
                @$"SELECT Realm FROM WebApiUsers with (nolock) where Login like @login and Password like @password;",
                new List<SqlParameter>()
                {
                    new SqlParameter()
                    {
                        ParameterName = "login",
                        SqlDbType = SqlDbType.Text,
                        SqlValue = username
                    },
                    new SqlParameter()
                    {
                        ParameterName = "password",
                        SqlDbType = SqlDbType.Text,
                        SqlValue = password
                    }
                }))
            {
                if (reader.HasRows)
                {
                    reader.Read();
                    if(reader.GetString(0).ToLower().Equals(_realm.ToLower()))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void ReturnUnauthorizedResult(AuthorizationFilterContext context)
    {
        // Return 401 and a basic authentication challenge (causes browser to show login dialog)
        context.HttpContext.Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{_realm}\"";
        context.Result = new UnauthorizedResult();
    }
}
}