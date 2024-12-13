using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApplication1.Database;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController: ControllerBase
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public struct LoginProps
    {
        public string email { get; set; }
        public string password { get; set; }
    }
    [HttpPost("login")]
    public string login([FromBody] LoginProps loginProps, RevoluDbContext db)
    {
        try
        {
           
            Console.WriteLine("test0");

            var user = db.Users.FirstOrDefault(u => u.Email == loginProps.email);
            Console.WriteLine("test022");

            if (user == null)
            {
                return JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = "Utilisateur introuvable."
                });
            }
            Console.WriteLine("test1");
            var testPassword = BCrypt.Net.BCrypt.Verify(loginProps.password, user.Password);
            if (!testPassword)
            {
                return JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = "ID incorrect"
                });
            }      
            Console.WriteLine("test2");

            string token = new string(Enumerable.Range(0, 128).Select(num => Alphabet[new Random().Next(Alphabet.Length)]).ToArray());
            user.Token = token;
            db.SaveChanges();
            Console.WriteLine("test3");

            return JsonConvert.SerializeObject(new
            {
                status = true,
                message = "Connected",
                token
            });
        }
        catch (Exception ex)
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = $"Erreur lors de la connexion : {ex.Message}"
            });
        }
       
        
        
    }

  
    [HttpGet("infos")]
    public string getInfos(RevoluDbContext db)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = "Non authentifié."
            });
        }

        return JsonConvert.SerializeObject(new
        {
            status = true,
            message = "Done",
            data = new
            {
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                iban = user.Iban,
                bic = user.Bic,
                box = user.Boxes
            }
        });
    }
}