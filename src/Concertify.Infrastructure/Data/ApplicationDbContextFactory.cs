using Microsoft.EntityFrameworkCore;

namespace Concertify.Infrastructure.Data;

public class ApplicationDbContextFactory : IDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        return new ApplicationDbContext(optionsBuilder.Options);
        
    }
}
