using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApplication1.Database;

namespace WebApplication1.Controllers.Users;

[ApiController]
[Route("api/user/[controller]")]
public class TransactionController : ControllerBase
{
    public struct TransactionProps
    {
        public int SourceBoxId { get; set; }
        public int? TargetBoxId { get; set; }
        public string? TargetIban { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }

    [HttpPost("transfer")]
    public string Transfer([FromBody] TransactionProps props, RevoluDbContext db)
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

        if (props.Amount <= 0 || string.IsNullOrWhiteSpace(props.Description))
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = "Montant invalide ou description manquante."
            });
        }

        var sourceBox = db.Boxes.FirstOrDefault(b => b.Id == props.SourceBoxId && b.UserId == user.Id);
        if (sourceBox == null || sourceBox.Balance < props.Amount)
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = sourceBox == null ? "Boîte source introuvable." : "Fonds insuffisants."
            });
        }

        if (props.TargetBoxId.HasValue)
        {
            // Transfert vers une autre boîte
            var targetBox = db.Boxes.FirstOrDefault(b => b.Id == props.TargetBoxId.Value);
            if (targetBox == null)
            {
                return JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = "Boîte cible introuvable."
                });
            }

            sourceBox.Balance -= props.Amount;
            targetBox.Balance += props.Amount;

            // Créer une transaction pour la boîte source (perte d'argent)
            var sourceTransaction = new Transaction
            {
                BoxId = props.SourceBoxId,
                Amount = -props.Amount,
                Name = props.Description,
                Description = $"Transfert vers boîte ID {props.TargetBoxId.Value}",
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            };

// Créer une transaction pour la boîte cible (gain d'argent)
            var targetTransaction = new Transaction
            {
                BoxId = props.TargetBoxId.Value,
                Amount = props.Amount,
                Name = props.Description,
                Description = $"Reçu de la boîte ID {props.SourceBoxId}",
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            };

// Ajouter les deux transactions à la base de données
            db.Transactions.Add(sourceTransaction);
            db.Transactions.Add(targetTransaction);

            db.SaveChanges();

            return JsonConvert.SerializeObject(new
            {
                status = true,
                message = "Transfert effectué avec succès."
            });
        }
        else if (!string.IsNullOrWhiteSpace(props.TargetIban))
        {
            // Transfert vers un autre utilisateur via IBAN
            var targetUser = db.Users.FirstOrDefault(u => u.Iban == props.TargetIban);
            if (targetUser == null)
            {
                return JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = "Utilisateur cible introuvable."
                });
            }

            sourceBox.Balance -= props.Amount;

            var transaction = new Transaction
            {
                BoxId = props.SourceBoxId,
                Amount = -props.Amount,
                Name = props.Description,
                Description = $"Transfert vers {props.TargetIban}",
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            };
            db.Transactions.Add(transaction);

            db.SaveChanges();

            return JsonConvert.SerializeObject(new
            {
                status = true,
                message = "Transfert effectué avec succès."
            });
        }
        else
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = "Boîte cible ou IBAN requis pour le transfert."
            });
        }
    }

    [HttpPost("earn")]
    public string Earn([FromBody] TransactionProps props, RevoluDbContext db)
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

        if (props.Amount <= 0 || string.IsNullOrWhiteSpace(props.Description))
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = "Montant invalide ou description manquante."
            });
        }

        var sourceBox = db.Boxes.FirstOrDefault(b => b.Id == props.SourceBoxId && b.UserId == user.Id);
        if (sourceBox == null)
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = "Boîte introuvable."
            });
        }

        sourceBox.Balance += props.Amount;

        var transaction = new Transaction
        {
            BoxId = props.SourceBoxId,
            Amount = props.Amount,
            Description = props.Description,
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
        };

        db.Transactions.Add(transaction);
        db.SaveChanges();

        return JsonConvert.SerializeObject(new
        {
            status = true,
            message = "Montant ajouté avec succès."
        });
    }

    [HttpPost("spend")]
    public string Spend([FromBody] TransactionProps props, RevoluDbContext db)
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

        if (props.Amount <= 0 || string.IsNullOrWhiteSpace(props.Description))
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = "Montant invalide ou description manquante."
            });
        }

        var sourceBox = db.Boxes.FirstOrDefault(b => b.Id == props.SourceBoxId && b.UserId == user.Id);
        if (sourceBox == null || sourceBox.Balance < props.Amount)
        {
            return JsonConvert.SerializeObject(new
            {
                status = false,
                message = sourceBox == null ? "Boîte introuvable." : "Fonds insuffisants."
            });
        }

        sourceBox.Balance -= props.Amount;

        var transaction = new Transaction
        {
            BoxId = props.SourceBoxId,
            Amount = -props.Amount,
            Description = props.Description,
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
        };

        db.Transactions.Add(transaction);
        db.SaveChanges();

        return JsonConvert.SerializeObject(new
        {
            status = true,
            message = "Montant dépensé avec succès."
        });
    }
}