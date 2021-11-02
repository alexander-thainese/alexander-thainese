using System.Data.Entity;

namespace CMT.DL
{
    public interface ICmtEntities
    {
        DbSet<User> Users { get; }
        DbSet<Country> Countries { get; }
        DbSet<UserCountry> UserCountries { get; }
    }
}