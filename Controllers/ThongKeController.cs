using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Converse_NMCNPM.Data;
using Converse_NMCNPM.Models;

namespace Converse_NMCNPM.Controllers
{
    public class ThongKeController : Controller
    {
        private readonly AppDbContext _context;

        public ThongKeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string filter = "month")
        {
            var role = (HttpContext.Session.GetString("Role") ?? "").Trim();

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "TaiKhoan");

            if (role != "Admin" && role != "QuanLyKho")
                return RedirectToAction("Index", "DanhSachSP");

            var model = new ThongKeViewModel();
            ViewBag.Filter = filter;

            // 1. Tổng giờ làm của từng nhân viên
            var gioLamData = await _context.NhanViens
                .OrderByDescending(x => x.TongGioLam)
                .Select(x => new
                {
                    TenNhanVien = x.Username,
                    TongGio = x.TongGioLam
                })
                .ToListAsync();

            model.NhanVienLabels = gioLamData.Select(x => x.TenNhanVien).ToList();
            model.NhanVienHours = gioLamData.Select(x => x.TongGio).ToList();

            // 2. Doanh thu
            if (filter == "day")
            {
                var doanhThuData = await _context.DonHangs
                    .Join(_context.ThanhToans,
                        dh => dh.MaDH,
                        tt => tt.MaDH,
                        (dh, tt) => new { dh, tt })
                    .Where(x => x.tt.TrangThai == "DaThanhToan")
                    .GroupBy(x => x.dh.NgayTao.Date)
                    .Select(g => new
                    {
                        Label = g.Key,
                        TongDoanhThu = g.Sum(x => x.dh.TongTien)
                    })
                    .OrderBy(x => x.Label)
                    .ToListAsync();

                model.DoanhThuLabels = doanhThuData
                    .Select(x => x.Label.ToString("dd/MM/yyyy"))
                    .ToList();

                model.DoanhThuValues = doanhThuData
                    .Select(x => x.TongDoanhThu)
                    .ToList();
            }
            else if (filter == "week")
            {
                var donHangs = await _context.DonHangs
                    .Join(_context.ThanhToans,
                        dh => dh.MaDH,
                        tt => tt.MaDH,
                        (dh, tt) => new { dh, tt })
                    .Where(x => x.tt.TrangThai == "DaThanhToan")
                    .ToListAsync();

                var doanhThuData = donHangs
                    .GroupBy(x =>
                    {
                        var date = x.dh.NgayTao.Date;
                        var startOfYear = new DateTime(date.Year, 1, 1);
                        int week = ((date - startOfYear).Days / 7) + 1;
                        return new { date.Year, Week = week };
                    })
                    .Select(g => new
                    {
                        Label = $"Tuần {g.Key.Week}/{g.Key.Year}",
                        TongDoanhThu = g.Sum(x => x.dh.TongTien)
                    })
                    .OrderBy(x => x.Label)
                    .ToList();

                model.DoanhThuLabels = doanhThuData.Select(x => x.Label).ToList();
                model.DoanhThuValues = doanhThuData.Select(x => x.TongDoanhThu).ToList();
            }
            else if (filter == "year")
            {
                var doanhThuData = await _context.DonHangs
                    .Join(_context.ThanhToans,
                        dh => dh.MaDH,
                        tt => tt.MaDH,
                        (dh, tt) => new { dh, tt })
                    .Where(x => x.tt.TrangThai == "DaThanhToan")
                    .GroupBy(x => x.dh.NgayTao.Year)
                    .Select(g => new
                    {
                        Year = g.Key,
                        TongDoanhThu = g.Sum(x => x.dh.TongTien)
                    })
                    .OrderBy(x => x.Year)
                    .ToListAsync();

                model.DoanhThuLabels = doanhThuData
                    .Select(x => $"Năm {x.Year}")
                    .ToList();

                model.DoanhThuValues = doanhThuData
                    .Select(x => x.TongDoanhThu)
                    .ToList();
            }
            else // month
            {
                var doanhThuData = await _context.DonHangs
                    .Join(_context.ThanhToans,
                        dh => dh.MaDH,
                        tt => tt.MaDH,
                        (dh, tt) => new { dh, tt })
                    .Where(x => x.tt.TrangThai == "DaThanhToan")
                    .GroupBy(x => new { x.dh.NgayTao.Year, x.dh.NgayTao.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TongDoanhThu = g.Sum(x => x.dh.TongTien)
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToListAsync();

                model.DoanhThuLabels = doanhThuData
                    .Select(x => $"Tháng {x.Month}/{x.Year}")
                    .ToList();

                model.DoanhThuValues = doanhThuData
                    .Select(x => x.TongDoanhThu)
                    .ToList();
            }

            // 3. Số lượng sản phẩm bán được
            if (filter == "day")
            {
                var sanPhamBanData = await _context.ChiTietDonHangs
                    .Include(x => x.ChiTietSP)
                    .ThenInclude(x => x.SanPham)
                    .Include(x => x.DonHang)
                    .GroupBy(x => x.ChiTietSP != null && x.ChiTietSP.SanPham != null
                        ? x.ChiTietSP.SanPham.TenSP
                        : "")
                    .Select(g => new
                    {
                        TenSP = g.Key,
                        TongSoLuong = g.Where(x => x.DonHang.NgayTao.Date == DateTime.Today).Sum(x => x.SoLuong)
                    })
                    .Where(x => x.TongSoLuong > 0)
                    .OrderByDescending(x => x.TongSoLuong)
                    .Take(10)
                    .ToListAsync();

                model.SanPhamLabels = sanPhamBanData.Select(x => x.TenSP).ToList();
                model.SanPhamSoLuong = sanPhamBanData.Select(x => x.TongSoLuong).ToList();
            }
            else
            {
                var sanPhamBanData = await _context.ChiTietDonHangs
                    .Include(x => x.ChiTietSP)
                    .ThenInclude(x => x.SanPham)
                    .GroupBy(x => x.ChiTietSP != null && x.ChiTietSP.SanPham != null
                        ? x.ChiTietSP.SanPham.TenSP
                        : "")
                    .Select(g => new
                    {
                        TenSP = g.Key,
                        TongSoLuong = g.Sum(x => x.SoLuong)
                    })
                    .OrderByDescending(x => x.TongSoLuong)
                    .Take(10)
                    .ToListAsync();

                model.SanPhamLabels = sanPhamBanData.Select(x => x.TenSP).ToList();
                model.SanPhamSoLuong = sanPhamBanData.Select(x => x.TongSoLuong).ToList();
            }

            return View(model);
        }
    }
}