using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApplication1.Database;

namespace WebApplication1.Controllers.Users;

[ApiController]
[Route("api/user/[controller]")]
public class BoxController : ControllerBase
{
    public struct CreateBoxProps
    {
        public string name { get; set; }
    }

    public struct UpdateBoxProps
    {
        public string name { get; set; }
    }

    [HttpGet()]
    public string Get(RevoluDbContext db)
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
        var boxes = db.Boxes
            .Where(b => b.UserId == user.Id)
            .Select(b => new
            {
                b.Id,
                b.Name,
                b.Balance,
                b.UserId
            })
            .ToList();

        return JsonConvert.SerializeObject(new
        {
            status = true,
            message = "Done",
            data = boxes
        });
    }

    [HttpGet("{id}")]
    public string GetOne(int id, RevoluDbContext db)
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
        var box = db.Boxes.FirstOrDefault(b => b.Id == id && b.UserId == user.Id);
        if (box == null)
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = "Boite introuvable!"
            });
        }
        var transaction = db.Transactions.Where(t => t.BoxId == box.Id).Select(t => new
        {
            t.Name,
            t.Description,
            t.Amount,
            t.CreatedAt
            
        }).ToList();
        return JsonConvert.SerializeObject(new
        {
            status = true,
            message = "Done",
            data = new
            {
                box.Id,
                box.Name,
                box.Balance,
                box.UserId,
                transaction
            }
        });
    }

    [HttpPost()]
    public string Create([FromBody] CreateBoxProps props, RevoluDbContext db)
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

        if (string.IsNullOrWhiteSpace(props.name))
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = "Le nom de la boîte est requis."
            });
        }

        var box = new Box
        {
            Name = props.name,
            Balance = 0,
            UserId = user.Id
        };

        db.Boxes.Add(box);
        db.SaveChanges();

        return JsonConvert.SerializeObject(new
        {
            status = true,
            message = "Boîte créée avec succès.",
        });
    }

    [HttpPut("{id}")]
    public string Update(int id, [FromBody] UpdateBoxProps props, RevoluDbContext db)
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

        var box = db.Boxes.FirstOrDefault(b => b.Id == id && b.UserId == user.Id);
        if (box == null)
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = "Boîte introuvable."
            });
        }

        if (string.IsNullOrWhiteSpace(props.name))
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = "Le nom de la boîte est requis."
            });
        }

        box.Name = props.name;
        db.Boxes.Update(box);
        db.SaveChanges();

        return JsonConvert.SerializeObject(new
        {
            status = true,
            message = "Boîte mise à jour avec succès."
        });
    }

    [HttpDelete("{id}")]
    public string Delete(int id, RevoluDbContext db)
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

        var box = db.Boxes.FirstOrDefault(b => b.Id == id && b.UserId == user.Id);
        if (box == null)
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = "Boîte introuvable."
            });
        }

        db.Boxes.Remove(box);
        db.SaveChanges();

        return JsonConvert.SerializeObject(new
        {
            status = true,
            message = "Boîte supprimée avec succès."
        });
    }
    
    
    public struct CreateTransactionProps
    {
        public string targetIban { get; set; }
        public decimal amount { get; set; }
        public string description { get; set; }
        public int boxId { get; set; }
    }
    
    
    [HttpPost("/transaction/")]
    public string Create([FromBody] CreateTransactionProps props, RevoluDbContext db)
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

        if (string.IsNullOrWhiteSpace(props.targetIban) || props.amount <= 0 || props.boxId <= 0)
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = "Tous les champs sont requis (IBAN cible, montant, ID de la boîte)."
            });
        }

        var box = db.Boxes.FirstOrDefault(b => b.Id == props.boxId && b.UserId == user.Id);
        if (box == null)
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = "Boîte source introuvable ou non autorisée."
            });
        }

        var targetUser = db.Users.FirstOrDefault(u => u.Iban == props.targetIban);
        if (targetUser == null)
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = "Utilisateur avec cet IBAN introuvable."
            });
        }

        if (box.Balance < props.amount)
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = "AHAH TU ES PAUVRE !"
            });
        }
        // Créer la transaction
        var transaction = new Transaction
        {
            BoxId = box.Id,
            Amount = props.amount,
            Description = props.description,
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            
        };

        db.Transactions.Add(transaction);
        db.SaveChanges();

        return JsonConvert.SerializeObject(new
        {
            status = true,
            message = "Transaction créée avec succès."
        });
    }
    
}