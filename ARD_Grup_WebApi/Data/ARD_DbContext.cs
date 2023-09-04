using ARD_Grup_WebApi.Data;
using Microsoft.EntityFrameworkCore;

public class ARD_DbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public ARD_DbContext(DbContextOptions<ARD_DbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Role> Roles { get; set; }




    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
         optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<Team>()
            .HasMany(t => t.TeamMembers)
            .WithOne(u => u.Teams);

            

    }


}
