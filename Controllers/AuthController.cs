using System.Numerics;
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
           

            var user = db.Users.FirstOrDefault(u => u.Email == loginProps.email);

            if (user == null)
            {
                return JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = "Utilisateur introuvable."
                });
            }
            var testPassword = BCrypt.Net.BCrypt.Verify(loginProps.password, user.Password);
            if (!testPassword)
            {
                return JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = "ID incorrect"
                });
            }      

            string token = new string(Enumerable.Range(0, 128).Select(num => Alphabet[new Random().Next(Alphabet.Length)]).ToArray());
            user.Token = token;
            db.SaveChanges();

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
    public struct CreateProps
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        
        
        
    }
    
    private static string GenerateIBAN()
    {
        string bankCode = "38395";
        string countryCode = "FR";
        string branchCode = "67930";
        var accountNumber = new string(Enumerable.Range(0, 11).Select(_ => (char)('0' + new Random().Next(0, 10))).ToArray());

        string ibanBase = $"{bankCode}{branchCode}{accountNumber}";

        string numericCountryCode = $"{countryCode[0] - 'A' + 10}{countryCode[1] - 'A' + 10}";
        string checksumBase = ibanBase + numericCountryCode + "00";

        int checksum = 98 - (int)(BigInteger.Parse(checksumBase) % 97);

        return $"{countryCode}{checksum:D2}{ibanBase}";
    }
    private static string GenerateBIC()
    {
        Random random = new();

        string bankCode = "REVU";
        string countryCode = "FR";
        string locationCode = "BR";
        string branchCode = new string(Enumerable.Range(0, 3).Select(_ => (char)(random.Next(0, 10) + '0')).ToArray());

        return $"{bankCode}{countryCode}{locationCode}{branchCode}";
    }
    
    [HttpPost("register")]
    public IActionResult register([FromBody] CreateProps props, RevoluDbContext db)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(props.firstName) || string.IsNullOrWhiteSpace(props.lastName) ||
                string.IsNullOrWhiteSpace(props.email) || string.IsNullOrWhiteSpace(props.password))
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Tous les champs sont requis."
                });
            }

            var existingUser = db.Users.FirstOrDefault(u => u.Email == props.email);
            if (existingUser != null)
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Un utilisateur avec cet email existe déjà."
                });
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(props.password);

            var user = new User
            {
                FirstName = props.firstName,
                LastName = props.lastName,
                Email = props.email,
                Password = hashedPassword,
                Iban = GenerateIBAN(),
                Bic = GenerateBIC()
            };

            db.Users.Add(user);
            db.SaveChanges();

            return Ok(new
            {
                status = true,
                message = "Utilisateur créé avec succès."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                status = false,
                message = $"Erreur lors de la création de l'utilisateur : {ex.Message}"
            });
        }
    }
}