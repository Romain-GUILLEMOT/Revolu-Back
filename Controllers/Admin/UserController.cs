using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebApplication1.Database;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController: ControllerBase
{
    
    public struct CreateProps
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        
        public string token { get; set; }
        
        
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
    [HttpPost("create")]
    public string create([FromBody] CreateProps props, RevoluDbContext db)
    {
        Console.WriteLine("AA" + props.firstName);
        if(props.token != "a7B9c1D3eF5G7h9I2J4kL6M8nO0PqRsTuVwXyZ1a3b5c7d9EfGhIjKlMnOpQrStUvWxYz0123456789ABCDEF")
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = "Token incorrect"
            });
        }
        try
        {
            if (string.IsNullOrWhiteSpace(props.firstName) || string.IsNullOrWhiteSpace(props.lastName) ||
                string.IsNullOrWhiteSpace(props.email) || string.IsNullOrWhiteSpace(props.password))
            {
                return JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = "Tous les champs sont requis."
                });
            }

            var existingUser = db.Users.FirstOrDefault(u => u.Email == props.email);
            if (existingUser != null)
            {
                return JsonConvert.SerializeObject(new
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

            return JsonConvert.SerializeObject(new
            {
                status = true,
                message = "Utilisateur créé avec succès."
            });
        }
        catch (Exception ex)
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = $"Erreur lors de la création de l'utilisateur : {ex.Message}"
            });
        }
    }
    
    public struct EditProps
    {
        public int userId { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
        public string token { get; set; }
    }

    [HttpPost("edit")]
    public string Edit([FromBody] EditProps props, RevoluDbContext db)
    {
        if(props.token != "a7B9c1D3eF5G7h9I2J4kL6M8nO0PqRsTuVwXyZ1a3b5c7d9EfGhIjKlMnOpQrStUvWxYz0123456789ABCDEF")
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = "Token incorrect"
            });
        }
        try
        {
            if (props.userId <= 0)
            {
                return JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = "L'ID de l'utilisateur est requis."
                });
            }

            var user = db.Users.FirstOrDefault(u => u.Id == props.userId);
            if (user == null)
            {
                return JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = "Utilisateur introuvable."
                });
            }

            if (!string.IsNullOrWhiteSpace(props.email))
            {
                var existingUser = db.Users.FirstOrDefault(u => u.Email == props.email && u.Id != props.userId);
                if (existingUser != null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        status = false,
                        message = "Un utilisateur avec cet email existe déjà."
                    });
                }
                user.Email = props.email;
            }

            if (!string.IsNullOrWhiteSpace(props.firstName))
            {
                user.FirstName = props.firstName;
            }

            if (!string.IsNullOrWhiteSpace(props.lastName))
            {
                user.LastName = props.lastName;
            }

            if (!string.IsNullOrWhiteSpace(props.password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(props.password);
            }

            db.Users.Update(user);
            db.SaveChanges();

            return JsonConvert.SerializeObject(new
            {
                status = true,
                message = "Utilisateur mis à jour avec succès."
            });
        }
        catch (Exception ex)
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = $"Erreur lors de la mise à jour de l'utilisateur : {ex.Message}"
            });
        }
    }

    public struct RemoveProps
    {
        public int userId { get; set; }
        public string token { get; set; }
    }

    [HttpPost("remove")]
    public string Remove([FromBody] RemoveProps props, RevoluDbContext db)
    {
        if(props.token != "a7B9c1D3eF5G7h9I2J4kL6M8nO0PqRsTuVwXyZ1a3b5c7d9EfGhIjKlMnOpQrStUvWxYz0123456789ABCDEF")
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = "Token incorrect"
            });
        }
        try
        {
            if (props.userId <= 0)
            {
                return JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = "L'ID de l'utilisateur est requis."
                });
            }

            var user = db.Users.FirstOrDefault(u => u.Id == props.userId);
            if (user == null)
            {
                return JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = "Utilisateur introuvable."
                });
            }

            db.Users.Remove(user);
            db.SaveChanges();

            return JsonConvert.SerializeObject(new
            {
                status = true,
                message = "Utilisateur supprimé avec succès."
            });
        }
        catch (Exception ex)
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = $"Erreur lors de la suppression de l'utilisateur : {ex.Message}"
            });
        }
    }
}
