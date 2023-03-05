// ReSharper disable InconsistentNaming
namespace API.Models;

public class CreateUserRequest
{
    public string email { get; set; }
    public bool blocked { get; set; } = false;
    public bool email_verified { get; set; } = false;
    public string given_name { get; set; }
    public string family_name { get; set; }
    public string name { get; set; }
    public string nickname { get; set; }
    public string picture { get; set; } // URL to image
    public string connection { get; set; } // Auth0 Database name
    public string password { get; set; }
    public bool verify_email { get; set; } = true;
    public string username { get; set; } // Only valid if the connection requires a username.
    //public string user_id { get; set; }
}