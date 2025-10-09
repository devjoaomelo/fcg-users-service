using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FCG.Users.Infra.Data
{
    public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
    {
        public UsersDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<UsersDbContext>();
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 42));
            var cs = "Server=127.0.0.1;Port=3306;Database=fcg_users;User=fcg;Password=fcgpwd12;SslMode=None";

            options.UseMySql(cs, serverVersion, my =>
            {
                my.MigrationsAssembly(typeof(UsersDbContext).Assembly.FullName);
            });

            return new UsersDbContext(options.Options);
        }
    }
}
