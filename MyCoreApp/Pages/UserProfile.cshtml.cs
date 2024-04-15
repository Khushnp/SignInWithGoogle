using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyCoreApp.Model;
using Newtonsoft.Json;

namespace MyCoreApp.Pages
{
    public class UserProfileModel : PageModel
    {
        private readonly MyCoreApp.Model.AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        // Inject required services
        public UserProfileModel(MyCoreApp.Model.AppDbContext context, IHttpContextAccessor httpContextAccessor,IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }
        private string ClientId => _configuration["Google:ClientID"];
        private string SecretKey => _configuration["Google:SecretKey"];
        private string RedirectUrl => _configuration["Google:RedirectUrl"];

        public string Picture { get; set; }
        public string Email { get; set; }
        public bool VerifiedEmail { get; set; }
        public string Name { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string Link { get; set; }
        public string Gender { get; set; }
        public string Locale { get; set; }

        //public async Task OnGetAsync()
        //{
        //    // Logic to fetch user profile from session or database if needed
        //    // Example: UserProfile = await _context.UserProfile.FirstOrDefaultAsync();
        //}

        //[HttpGet]
        public async Task OnGetAsync(string code, string state, string session_state)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    //return View("Error");
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
                    //return View("Error");
                }

                var userProfile = await GetuserProfile(token.AccessToken);
                if (userProfile != null)
                {
                    HelperStaticClass.IsUserAuthenticated = true;
                    // Assign retrieved user profile data to properties
                    Picture = userProfile.Picture;
                    Email = userProfile.Email;
                    VerifiedEmail = userProfile.VerifiedEmail;
                    Name = userProfile.Name;
                    GivenName = userProfile.GivenName;
                    FamilyName = userProfile.FamilyName;
                    Link = userProfile.Link;
                    Gender = userProfile.Gender;
                    Locale = userProfile.Locale;
                }
                //return View("UserProfile", obj);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
        }
        // Define method to fetch user profile data
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
