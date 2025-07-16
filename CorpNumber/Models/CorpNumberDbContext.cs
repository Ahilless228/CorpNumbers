using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ParsingPDF;
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

        // DbSet-свойства для таблиц

        public DbSet<ReportEntry> ReportEntries { get; set; }
        public DbSet<ServiceCharge> ServiceCharges { get; set; }
        public DbSet<ServiceName> ServiceNames { get; set; }
        public DbSet<Accounts> Accounts { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Districts> Districts { get; set; }
        public DbSet<Phone> Phones { get; set; }
        public DbSet<Operator> Operators { get; set; }
        public DbSet<Quota> Quotas { get; set; }
        public DbSet<Nationalities> Nationalities { get; set; }
        public DbSet<Citizenships> Citizenships { get; set; }
        public DbSet<CompanyDocs> CompanyDocs { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<OtherOwner> OtherOwners { get; set; }
        public DbSet<Operations> Operations { get; set; }
        public DbSet<Tariff> Tariffs { get; set; }
        public DbSet<SimCard> SimCards { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostCategory> PostCategories { get; set; }
        public DbSet<Sections> Sections { get; set; }
        public DbSet<Sexes> Sexes { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<TempOwners> TempOwners { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<InternetService> InternetServices { get; set; }
        public DbSet<OperationTypes> OperationTypes { get; set; }
        public DbSet<OwnerCategory> OwnerCategories { get; set; }
      



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Связь между Phone и Operator
            modelBuilder.Entity<Phone>()
                .HasOne(p => p.OperatorNavigation)
                .WithMany(o => o.Phones)
                .HasForeignKey(p => p.Operator)
                .HasPrincipalKey(o => o.CodeOperator)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь между Phone и Tariff
            modelBuilder.Entity<Phone>()
                .HasOne(p => p.TariffNavigation)
                .WithMany(t => t.Phones)
                .HasForeignKey(p => p.Tariff)
                .HasPrincipalKey(t => t.CodeTariff)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь между Phone и Status
            modelBuilder.Entity<Phone>()
                .HasOne(p => p.StatusNavigation)
                .WithMany(s => s.Phones)
                .HasForeignKey(p => p.Status)
                .HasPrincipalKey(s => s.Code)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь между Phone и InternetService
            modelBuilder.Entity<Phone>()
                .HasOne(p => p.InternetNavigation)
                .WithMany(i => i.Phones)
                .HasForeignKey(p => p.Internet)
                .HasPrincipalKey(i => i.CodeServ)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Owner>()
                .HasOne(o => o.CategoryNavigation)
                .WithMany(c => c.Owners)
                .HasForeignKey(o => o.CodeCategory)
                .HasPrincipalKey(c => c.CodeCategory)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Phone>()
                .HasOne(p => p.CodeOwnerNavigation)
                .WithMany(o => o.Phones)  // В модели Owner должно быть ICollection<Phone> Phones { get; set; }
                .HasForeignKey(p => p.CodeOwner)
                .HasPrincipalKey(o => o.CodeOwner)  // Код владельца в Owner
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.PostNavigation)
                .WithMany()
                .HasForeignKey(e => e.Post);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.DepartmentNavigation)
                .WithMany()
                .HasForeignKey(e => e.Department);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.SectionNavigation)
                .WithMany()
                .HasForeignKey(e => e.Section);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.CodeQuotaNavigation)
                .WithMany()
                .HasForeignKey(e => e.CodeQuota);
            

            modelBuilder.Entity<Operations>()
                .HasOne(o => o.OperationTypes)
                .WithMany(t => t.Operations)
                .HasForeignKey(o => o.CodeOperType)
                .HasPrincipalKey(t => t.CodeOperType);

            modelBuilder.Entity<Operations>()
                .HasOne(o => o.Phone)
                .WithMany(p => p.Operations)
                .HasForeignKey(o => o.Number)
                .HasPrincipalKey(p => p.CodePhone);


            modelBuilder.Entity<ServiceCharge>()
                .Property(s => s.Amount)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ReportEntry>().Ignore(r => r.Phone);

            modelBuilder.Entity<ReportEntry>()
                .HasMany(r => r.Services)
                .WithOne(s => s.ReportEntry)
                .HasForeignKey(s => s.ReportEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReportEntry>()
                .HasIndex(r => new { r.PhoneId, r.ReportDate });
        }

        public void EnsureServiceNameExists(string serviceName)
        {
            // Загрузить все ServiceNames в память
            var serviceNamesDict = ServiceNames.ToDictionary(s => s.Name, s => s);

            // ... в цикле:
            if (!serviceNamesDict.TryGetValue(serviceName, out var service))
            {
                service = new ServiceName { Name = serviceName };
                ServiceNames.Add(service);
                SaveChanges();
                serviceNamesDict[serviceName] = service;
            }
        }

        // Если появятся модели Status и InternetService:
        // modelBuilder.Entity<Phone>()
        //     .HasOne(p => p.StatusNavigation)
        //     .WithMany()
        //     .HasForeignKey(p => p.Status)
        //     .OnDelete(DeleteBehavior.Restrict);

        // modelBuilder.Entity<Phone>()
        //     .HasOne(p => p.InternetNavigation)
        //     .WithMany()
        //     .HasForeignKey(p => p.Internet)
        //     .OnDelete(DeleteBehavior.Restrict);
    }
}


