using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
namespace Concertify.Domain.Models;

public class ApplicationUser : IdentityUser
{
    //private string _username = default!;
    [ProtectedPersonalData]
    //public new string UserName { get => _username; set => _username = value; }
    public override string? UserName { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}