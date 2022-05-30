using Imgeneus.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Imgeneus.Database
{
    public static class ConfigureDatabase
    {
        public static DbContextOptionsBuilder ConfigureCorrectDatabase(this DbContextOptionsBuilder optionsBuilder, DatabaseConfiguration configuration)
        {
            optionsBuilder.UseMySql(
                configuration.ToString(),
                new MySqlServerVersion(new Version(8, 0, 22)),
                builder => {
                    builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
                });
            return optionsBuilder;
        }

        public static IServiceCollection RegisterDatabaseServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddDbContext<IDatabase, DatabaseContext>(options =>
                {
                    var dbConfig = serviceCollection.BuildServiceProvider().GetService<IOptions<DatabaseConfiguration>>();
                    options.ConfigureCorrectDatabase(dbConfig.Value);

#if DEBUG
                    options.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
#endif
                },
                contextLifetime: ServiceLifetime.Transient,
                optionsLifetime: ServiceLifetime.Singleton);
        }
    }
}
