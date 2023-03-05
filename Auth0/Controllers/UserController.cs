using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace API.Controllers
{
    [Route("user")]
    public class UserController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly AppSettings _appSettings;

        public UserController(IHttpClientFactory clientFactory, IOptions<AppSettings> appSettings)
        {
            _clientFactory = clientFactory;
            _appSettings = appSettings.Value;
        }

        [HttpGet]
        [Route("Login")]
        public async Task<IActionResult> Login(string username, string password, CancellationToken cancellationToken)
        {
            var httpClient = _clientFactory.CreateClient();

            var contentDict = new Dictionary<string, string>
            {
                { "client_id", _appSettings.ClientAPI.ClientId },
                { "client_secret", _appSettings.ClientAPI.ClientSecret },
                { "grant_type", "password" },
                { "username", username },
                { "password", password },
                { "audience", _appSettings.ClientAPI.Audience },
                //{ "scope", "read:sample" },
            };

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, $"{_appSettings.ClientAPI.Domain}oauth/token")
            {
                Content = new FormUrlEncodedContent(contentDict),
                Headers = { { "Accept", "application/json" } }
            };

            var result = await httpClient.SendAsync(tokenRequest, cancellationToken);
            var body = JsonConvert.DeserializeObject<TokenResponse>(await result.Content.ReadAsStringAsync(cancellationToken));

            return Json(body);
        }

        [HttpGet]
        [Route("public")]
        public IActionResult Public()
        {
            return Json(new
            {
                Message = "Public!"
            });
        }

        [HttpGet]
        [Route("private")]
        [Authorize]
        public IActionResult Private()
        {
            return Json(new
            {
                Message = "Private!"
            });
        }

        [HttpGet]
        [Route("private-scoped")]
        [Authorize("read:messages")]
        public IActionResult Scoped()
        {
            return Json(new
            {
                Message = "Private with scope \"read:messages\"."
            });
        }


        /// <summary>
        /// This is a helper action. It allows you to easily view all the claims of the token
        /// </summary>
        /// <returns></returns>
        [HttpGet("claims")]
        [Authorize]
        public IActionResult Claims()
        {
            return Json(User.Claims.Select(c =>
                new
                {
                    c.Type,
                    c.Value
                }));
        }
    }
}
