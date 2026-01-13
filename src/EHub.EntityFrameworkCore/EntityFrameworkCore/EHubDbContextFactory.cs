using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EHub.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class EHubDbContextFactory : IDesignTimeDbContextFactory<EHubDbContext>
{
    public EHubDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        EHubEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<EHubDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new EHubDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../EHub.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
