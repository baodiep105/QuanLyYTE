
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using static QuanLyYTe.Models.Employees_API;

namespace QuanLyYTe.Repositorys
{
    public class ORCcontext: DbContext
    {
        private readonly IConfiguration _config;
        public ORCcontext()
        {
        }

        public ORCcontext(DbContextOptions<ORCcontext> options, IConfiguration config)
            : base(options)
        {
            _config = config;
        }
        public DbSet<TENTER> Tenter { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                //optionsBuilder.UseSqlServer("Data Source=DESKTOP-R4TO9PD\\MSSQLSERVER_1;Initial Catalog=NMCD1;Persist Security Info=True;User ID=sa;Password=123123;Encrypt=True;Trust Server Certificate=True");
                // optionsBuilder.UseSqlServer("Data Source=192.168.240.3;Initial Catalog=NMCD1;User ID=sa;Password=HPDQ@1234");
               // string connectionString = _config.GetConnectionString("ConnectionStringORC");
              //  optionsBuilder.UseOracle("Data Source=192.168.120.116:1521/hoaphat;User Id=UNISUSER;Password=unisamho",p=>p.UseOracleSQLCompatibility("11"));
                optionsBuilder.UseOracle(AppSettings.ConnectionStringORC);
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TENTER>(entity =>
            {
                entity.HasNoKey();
                entity.ToTable("TENTER");
                entity.Property(e => e.L_UID).HasColumnName("L_UID");
                entity.Property(e => e.L_TID).HasColumnName("L_TID");
                entity.Property(e => e.C_DATE)
                    .HasMaxLength(6)
                    .HasColumnName("C_DATE");
                entity.Property(e => e.C_TIME)
                    .HasMaxLength(6)
                    .HasColumnName("C_TIME");
                entity.Property(e => e.C_NAME)
                    .HasMaxLength(30)
                    .HasColumnName("C_NAME");
            });  
            
        }

    }
  
}
