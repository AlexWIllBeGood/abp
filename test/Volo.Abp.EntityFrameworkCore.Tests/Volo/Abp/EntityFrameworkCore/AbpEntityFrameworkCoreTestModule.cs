﻿using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Autofac;
using Volo.Abp.EntityFrameworkCore.TestApp.SecondContext;
using Volo.Abp.EntityFrameworkCore.TestApp.ThirdDbContext;
using Volo.Abp.Modularity;
using Volo.Abp.TestApp;
using Volo.Abp.TestApp.EntityFrameworkCore;

namespace Volo.Abp.EntityFrameworkCore
{
    [DependsOn(typeof(AbpEntityFrameworkCoreModule))]
    [DependsOn(typeof(TestAppModule))]
    [DependsOn(typeof(AbpAutofacModule))]
    [DependsOn(typeof(AbpEfCoreTestSecondContextModule))]
    public class AbpEntityFrameworkCoreTestModule : AbpModule
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddAssemblyOf<AbpEntityFrameworkCoreTestModule>();

            services.AddAbpDbContext<TestAppDbContext>(options =>
            {
                options.AddDefaultRepositories(true);
                options.ReplaceDbContext<IThirdDbContext>();
            });

            var sqliteConnection = CreateDatabaseAndGetConnection();

            services.Configure<AbpDbContextOptions>(options =>
            {
                options.Configure(context =>
                {
                    context.DbContextOptions.UseSqlite(sqliteConnection);
                });
            });
        }

        public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
        {
            context.ServiceProvider.GetRequiredService<SecondDbContext>().Database.Migrate();
        }

        private static SqliteConnection CreateDatabaseAndGetConnection()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            using (var context = new TestMigrationsDbContext(new DbContextOptionsBuilder<TestMigrationsDbContext>().UseSqlite(connection).Options))
            {
                context.GetService<IRelationalDatabaseCreator>().CreateTables();
            }
            
            return connection;
        }
    }
}
