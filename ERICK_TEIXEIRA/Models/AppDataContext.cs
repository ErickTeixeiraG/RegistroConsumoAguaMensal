using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace ERICK_TEIXEIRA.Models
{
    public class AppDataContext : DbContext
    {
        public DbSet<RegistroConsumo> RegistroConsumos { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Luiz_Erick.db");
        }
    }
}