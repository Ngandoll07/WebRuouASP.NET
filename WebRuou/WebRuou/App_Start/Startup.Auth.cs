using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Facebook;
using Owin;
using System;
using System.Security.Claims;
using WebRuou.Models;

public partial class Startup
{
    public void ConfigureAuth(IAppBuilder app)
    {
        // Cấu hình đăng nhập qua Google
        app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
        {
            ClientId = "",
            ClientSecret = "",
            Provider = new GoogleOAuth2AuthenticationProvider
            {
                OnAuthenticated = async context =>
                {
                    context.Identity.AddClaim(new Claim(ClaimTypes.Role, "User")); // Mặc định là User
                }
            }
        });

        // Cấu hình đăng nhập qua Facebook
        app.UseFacebookAuthentication(new FacebookAuthenticationOptions()
        {
            AppId = "YOUR_FACEBOOK_APP_ID",
            AppSecret = "YOUR_FACEBOOK_APP_SECRET",
            Provider = new FacebookAuthenticationProvider
            {
                OnAuthenticated = async context =>
                {
                    context.Identity.AddClaim(new Claim(ClaimTypes.Role, "User")); // Mặc định là User
                }
            }
        });

        // Cấu hình Cookie Authentication
        app.UseCookieAuthentication(new CookieAuthenticationOptions()
        {
            AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
            LoginPath = new PathString("/Account/Login")
        });
    }
}
