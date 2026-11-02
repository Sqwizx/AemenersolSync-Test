using System.ComponentModel.DataAnnotations.Schema;

namespace AemenersolSync.Api.Models;

[NotMapped]
public abstract class AbstractModel
{
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
