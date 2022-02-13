using Refactored.Domain.V1;
using Microsoft.EntityFrameworkCore;

class RefactoredDB : DbContext
{
    public RefactoredDB(DbContextOptions<RefactoredDB> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductOption> ProductOptions => Set<ProductOption>();
}
