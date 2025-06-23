using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography.Xml;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CorpNumber.Models
{
    public class CorpNumberDbContext : DbContext
    {
        public CorpNumberDbContext(DbContextOptions<CorpNumberDbContext> options)
            : base(options)
        {
        }

        // Пример таблиц
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Phone> Phones { get; set; }
        public DbSet<Operator> Operators { get; set; }
        public DbSet<Quota> Quotas { get; set; }
        public DbSet<Nationality> Nationalities { get; set; }
        public DbSet<Citizenships> Citizenships { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<OtherOwner> OtherOwners { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<Tariff> Tariffs { get; set; }
        public DbSet<SimCard> SimCards { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostCategory> PostCategories { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
    }
}
