using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[assembly: OwinStartup(typeof(WebRuou.Startup))]
namespace WebRuou
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
        private void ConfigureAuth(IAppBuilder app)
        {
            // 1️⃣ Cấu hình Cookie Authentication
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Login"),
                ExpireTimeSpan = TimeSpan.FromMinutes(60),
                SlidingExpiration = true
            });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
            // 2️⃣ Đăng nhập Google
            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "",
                ClientSecret = "",
                CallbackPath = new PathString("/signin-google"),
                SignInAsAuthenticationType = DefaultAuthenticationTypes.ExternalCookie
            });

            // Đăng nhập bằng Facebook
            app.UseFacebookAuthentication(new FacebookAuthenticationOptions()
            {
                AppId = "1329580451589504",
                AppSecret = "1bcee80af95578ccc5758a2f9ed0d4ac",
                CallbackPath = new PathString("/signin-facebook"),
                SignInAsAuthenticationType = DefaultAuthenticationTypes.ExternalCookie
            });
        }
    }
}