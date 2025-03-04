using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;

namespace QuanLyYTe.Repositorys
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base (options)
        { 
        }
        public DbSet<KSK_DauVao> KSK_DauVao { get; set; }
        public DbSet<GioiTinh> GioiTinh { get; set; }
        public DbSet<KetQuaDauVao> KetQuaDauVao { get; set; }
        public DbSet<LyDoKhongDat> LyDoKhongDat { get; set; }
        public DbSet<ViTriLamViec> ViTriLamViec { get; set; }
        public DbSet<KipLamViec> KipLamViec { get; set; }
        public DbSet<ToLamViec> ToLamViec { get; set; }
        public DbSet<PhanXuong> PhanXuong { get; set; }
        public DbSet<PhongBan> PhongBan { get; set; }
        public DbSet<NhanVien> NhanVien { get; set; }
        public DbSet<ViTriLaoDong> ViTriLaoDong { get; set; }
        public DbSet<DanhSachDocHai> DanhSachDocHai { get; set; }
        public DbSet<ChiTieuNoiDung> ChiTieuNoiDung { get; set; }
        public DbSet<PhanLoaiThoiHan> PhanLoaiThoiHan { get; set; }
        public DbSet<ChiTiet_ChiTieuNoiDung_ViTri> ChiTiet_ChiTieuNoiDung_ViTri { get; set; }
        public DbSet<SoTheoDoi_KSK> SoTheoDoi_KSK { get; set; }
        public DbSet<NhomMau> NhomMau { get; set; }
        public DbSet<KSK_DinhKy> KSK_DinhKy { get; set; }
        public DbSet<PhanLoaiKSK> PhanLoaiKSK { get; set; }
        public DbSet<CapPhatThuoc> CapPhatThuoc { get; set; }
        public DbSet<ChiTiet_CapPhatThuoc> ChiTiet_CapPhatThuoc { get; set; }
        public DbSet<NhomBenh> NhomBenh { get; set; }
        public DbSet<LoaiThuoc> LoaiThuoc { get; set; }
        public DbSet<TaiKhoan> TaiKhoan { get; set; }
        public DbSet<Quyen> Quyen { get; set; }
        public DbSet<SoCapCuu> SoCapCuu { get; set; }
        public DbSet<KSK_ChuyenViTri> KSK_ChuyenViTri { get; set; }
        public DbSet<KSK_BenhNgheNghiep> KSK_BenhNgheNghiep { get; set; }
        public DbSet<CT_KSK_BenhNgheNghiep> CT_KSK_BenhNgheNghiep { get; set; }
        public DbSet<TrinhKy> TrinhKy { get; set; }
        public DbSet<NhomTaiNan> NhomTaiNan { get; set; }
        public DbSet<NhomBenhLy> NhomBenhLy { get; set; }
        public DbSet<TuyenBenhVien> TuyenBenhVien { get; set; }
        public DbSet<TENTER> Tenter { get; set; }

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
            modelBuilder.Entity<ChiTiet_CapPhatThuoc>(entity =>
            {
                entity.ToTable("ChiTiet_CapPhatThuoc");

                entity.HasKey(e => e.ID_CT_CapThuoc); // Đặt khóa chính cho bảng

                entity.Property(e => e.ID_CT_CapThuoc)
                    .HasColumnName("ID_CT_CapThuoc");

                entity.Property(e => e.ID_CapThuoc)
                    .HasColumnName("ID_CapThuoc");

                entity.Property(e => e.ID_LoaiThuoc)
                    .HasColumnName("ID_LoaiThuoc");

                entity.Property(e => e.SoLuong)
                    .HasColumnName("SoLuong");
                // Đặt giới hạn độ dài nếu cần thiết

                // Không ánh xạ thuộc tính không tồn tại trong bảng bằng NotMapped
                entity.Ignore(e => e.TenLoaiThuoc); // Loại bỏ ánh xạ với cơ sở dữ liệu
            });

        }
    }


}
