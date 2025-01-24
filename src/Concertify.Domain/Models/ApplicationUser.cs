using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
namespace Concertify.Domain.Models;

public class ApplicationUser : IdentityUser
{
    [ProtectedPersonalData]
    public override string? UserName { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public List<Concert> RatedConcerts { get; set; } = [];
}