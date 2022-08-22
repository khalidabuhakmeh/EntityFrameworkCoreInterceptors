using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using var db = new CustomerContext();
db.Database.Migrate();

var entity = db.Customers.Add(new Customer
{
    Name = "Khalid",
    PhoneNumber = "(555) 867 5309"
});

db.SaveChanges();
db.ChangeTracker.Clear();

var customer = db.Customers.Find(entity.Entity.Id)!;
Console.WriteLine($"{customer.Id}: {customer.Name} @ {customer.PhoneNumber} [{customer.Retrieved}]");

public interface IHasRetrieved
{
    DateTime Retrieved { get; set; }
}

public class Customer : IHasRetrieved
{
    public int Id { get; set; } = 0;
    public string Name { get; set; } = "";
    public string? PhoneNumber { get; set; }

    [NotMapped] public DateTime Retrieved { get; set; }
}

public class SetRetrievedInterceptor : IMaterializationInterceptor
{
    public object InitializedInstance(MaterializationInterceptionData materializationData, object instance)
    {
        if (instance is IHasRetrieved hasRetrieved)
        {
            hasRetrieved.Retrieved = DateTime.UtcNow;
        }

        return instance;
    }
}

public class CustomerContext : DbContext
{
    private static readonly SetRetrievedInterceptor SetRetrievedInterceptor
        = new();

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .AddInterceptors(SetRetrievedInterceptor)
            .UseSqlite("Data Source = customers.db");
}