using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListController: ControllerBase
{
    public struct CreateProps
    {
        
        public int Lat { get; }
        public int Lon { get; }
        
     
        
    }
    [HttpPost("create")]
    public string Create([FromBody] CreateProps createProps)
    {
        // Utilisation d'un StringBuilder
        var builder = new StringBuilder();
        int totalRectangles = 0;

        for (int i = 0; i < createProps.Lat; i++)
        {
            for (int j = 0; j < createProps.Lon; j++)
            {
                builder.AppendLine($"Un rectangle de {j} par {i} donne {j * i}");
                totalRectangles++;
            }
        }

        builder.AppendLine($"Fin totale de {totalRectangles} rectangles");
        
        return builder.ToString();
    }
}