using Identity.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
        
    }

    public DbSet<User> Users { get; set; }  
    
}