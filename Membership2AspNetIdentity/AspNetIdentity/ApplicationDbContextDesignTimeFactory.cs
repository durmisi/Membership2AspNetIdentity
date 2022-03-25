using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Membership2AspNetIdentity.AspNetIdentity
{
    public class ApplicationDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer("Data Source=(local);Initial Catalog=AspNetIdentity;Integrated Security=True;");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }

}
