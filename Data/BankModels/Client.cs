using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace BankAPI.Data.BankModels{

public partial class Client
{
    public Client()
    {
         Accounts=new HashSet<Account>();
    }
    public int Id { get; set; }

[MaxLength(40, ErrorMessage = "El nombre debe ser menor a 200 craracteres.")]
    public string Name { get; set; } = null!;

[MaxLength(40, ErrorMessage = "El número de teléfono debe ser menor a 40 craracteres.")]
    public string PhoneNumber { get; set; } = null!;
    
[MaxLength(40, ErrorMessage = "El email debe ser menor a 50 craracteres.")]
[EmailAddress(ErrorMessage = "El formato de email es incorrecto")]
    public string? Email { get; set; }

    public DateTime RegDate { get; set; }

[JsonIgnore]
    public virtual ICollection<Account> Accounts { get; } = new List<Account>();
}
}