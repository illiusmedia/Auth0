using System.Net.Http.Headers;
using System.Text;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace API.Controllers
{
    [Route("management")]
    public class ManagementController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly AppSettings _appSettings;

        public ManagementController(IHttpClientFactory clientFactory, IOptions<AppSettings> appSettings)
        {
            _clientFactory = clientFactory;
            _appSettings = appSettings.Value;
        }

        [HttpGet]
        [Route("service-token")]
        public async Task<IActionResult> GetServiceToken(CancellationToken cancellationToken)
        {
            var httpClient = _clientFactory.CreateClient("service-token");

            var contentDict = new Dictionary<string, string>
            {
                { "client_id", _appSettings.ClientAPI.ClientId },
                { "client_secret", _appSettings.ClientAPI.ClientSecret },
                { "grant_type", "client_credentials" },
                { "audience", "https://dev-bqyr08coyzznixfx.eu.auth0.com/api/v2/" },
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

        [HttpPost]
        [Route("create-user")]
        [Authorize("create:users")] //TODO: add role?
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
        {
            var bearerToken = HttpContext.Request.Headers.Authorization.FirstOrDefault();

            if (bearerToken == null)
                return null;

            var httpClient = _clientFactory.CreateClient("create-user");
            httpClient.DefaultRequestHeaders.Add("Authorization", bearerToken);

            var contentDict = new Dictionary<string, string>
            {
                { "email", request.email },
                { "blocked", "false" },
                { "email_verified", "false" },
                { "given_name", request.given_name },
                { "family_name", request.family_name },
                { "name", request.name },
                { "nickname",request.nickname},
                { "picture", request.picture },
                { "connection", request.connection },
                { "password", request.password },
                { "verify_email", "true" },
                { "username", request.username },
            };

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, $"{_appSettings.ClientAPI.Domain}api/v2/users")
            {
                Content = new FormUrlEncodedContent(contentDict),
                Headers = { { "Accept", "application/json" } }
            };

            var response = await httpClient.SendAsync(tokenRequest, cancellationToken);

            return Json(response);
        }

    }
}
