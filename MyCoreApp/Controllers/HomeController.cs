using Microsoft.AspNetCore.Mvc;
using MyCoreApp.Model;
using Newtonsoft.Json;

namespace MyCoreApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        private string ClientId => _configuration["Google:ClientID"];
        private string SecretKey => _configuration["Google:SecretKey"];
        private string RedirectUrl => _configuration["Google:RedirectUrl"];

        public async Task<ActionResult> Index()
        {
            if (_httpContextAccessor?.HttpContext?.Session != null)
            {
                string token = _httpContextAccessor.HttpContext.Session.GetString("user");
                if (string.IsNullOrEmpty(token))
                {
                    return View();
                }
                else
                {
                    return View("UserProfile", await GetuserProfile(token));
                }
            }
            else
            {
                // Handle the case where HttpContext or Session is null
                // For example, you can return an error view
                return View("Error");
            }
        }

        public void LoginUsingGoogle()
        {
            Response.Redirect($"https://accounts.google.com/o/oauth2/v2/auth?client_id={ClientId}&response_type=code&scope=openid%20email%20profile&redirect_uri={RedirectUrl}&state=abcdef");
        }

        [HttpGet]
        public ActionResult SignOut()
        {
            if (_httpContextAccessor?.HttpContext?.Session != null)
            {
                _httpContextAccessor.HttpContext.Session.SetString("user", string.Empty);
                return View("Index");
            }
            else
            {
                // Handle the case where HttpContext or Session is null
                // For example, you can return an error view
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<ActionResult> SaveGoogleUser(string code, string state, string session_state)
        {
            if (string.IsNullOrEmpty(code))
            {
                return View("Error");
            }

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://www.googleapis.com")
            };
            var requestUrl = $"oauth2/v4/token?code={code}&client_id={ClientId}&client_secret={SecretKey}&redirect_uri={RedirectUrl}&grant_type=authorization_code";

            var dict = new Dictionary<string, string>
            {
                { "Content-Type", "application/x-www-form-urlencoded" }
            };
            var req = new HttpRequestMessage(HttpMethod.Post, requestUrl) { Content = new FormUrlEncodedContent(dict) };
            var response = await httpClient.SendAsync(req);
            var content = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<GmailToken>(content);

            if (_httpContextAccessor?.HttpContext?.Session != null)
            {
                _httpContextAccessor.HttpContext.Session.SetString("user", token?.AccessToken);
            }

            if (token == null || string.IsNullOrEmpty(token.AccessToken))
            {
                return View("Error");
            }

            var obj = await GetuserProfile(token.AccessToken);
            return View("UserProfile", obj);
        }

        public async Task<UserProfile> GetuserProfile(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return null;
            }

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://www.googleapis.com")
            };
            string url = $"https://www.googleapis.com/oauth2/v1/userinfo?alt=json&access_token={accessToken}";
            var response = await httpClient.GetAsync(url);
            return JsonConvert.DeserializeObject<UserProfile>(await response.Content.ReadAsStringAsync());
        }
    }
}
