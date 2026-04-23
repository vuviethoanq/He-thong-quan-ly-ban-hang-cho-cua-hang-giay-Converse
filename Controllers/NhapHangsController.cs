using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Converse_NMCNPM.Data;
using Converse_NMCNPM.Models;

namespace Converse_NMCNPM.Controllers
{
    public class NhapHangsController : Controller
    {
        private readonly AppDbContext _context;

        public NhapHangsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var role = (HttpContext.Session.GetString("Role") ?? "").Trim();

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "TaiKhoan");

            if (role != "Admin" && role != "QuanLyKho")
                return RedirectToAction("Index", "Home");

            var sanPhams = await _context.SanPhams
                .Include(x => x.DanhMuc)
                .ToListAsync();

            return View(sanPhams);
        }

        public async Task<IActionResult> ChiTiet(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var sanPham = await _context.SanPhams
                .Include(x => x.DanhMuc)
                .FirstOrDefaultAsync(x => x.MaSP == id);

            if (sanPham == null)
                return NotFound();

            var chiTietSPs = await _context.ChiTietSPs
                .Where(x => x.MaSP == id)
                .OrderBy(x => x.MauSP)
                .ThenBy(x => x.SizeSP)
                .ToListAsync();

            ViewBag.SanPham = sanPham;
            return View(chiTietSPs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiYeuCauNhap(string MaCTSP, string MaSP, int SoLuongNhap, decimal GiaNhap, string? GhiChu)
        {
            var role = (HttpContext.Session.GetString("Role") ?? "").Trim();

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "TaiKhoan");

            if (role != "Admin" && role != "QuanLyKho")
                return RedirectToAction("Index", "Home");

            try
            {
                var maNV = HttpContext.Session.GetString("MaNV");

                if (string.IsNullOrEmpty(maNV))
                {
                    TempData["Error"] = "Không xác định được nhân viên đang đăng nhập.";
                    return RedirectToAction("ChiTiet", new { id = MaSP });
                }

                // Tự sinh mã nhập hàng
                string maNH = "NH" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

                var nhapHang = new NhapHang
                {
                    MaNH = maNH,
                    MaNV = maNV,
                    NgayNhap = DateTime.Now,
                    GhiChu = GhiChu
                };

                _context.NhapHangs.Add(nhapHang);

                // Tự sinh mã chi tiết nhập hàng
                string maCTNH = "CTNH" + DateTime.Now.Ticks.ToString().Substring(8);

                var chiTietNhap = new ChiTietNhapHang
                {
                    MaCTNH = maCTNH,
                    MaNH = maNH,
                    MaCTSP = MaCTSP,
                    SoLuongNhap = SoLuongNhap,
                    GiaNhap = GiaNhap
                };

                _context.ChiTietNhapHangs.Add(chiTietNhap);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Gửi yêu cầu nhập hàng thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("ChiTiet", new { id = MaSP });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanDatHang(XacNhanNhapHangViewModel model)
        {
            var role = (HttpContext.Session.GetString("Role") ?? "").Trim();

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "TaiKhoan");

            if (role != "Admin" && role != "QuanLyKho")
                return RedirectToAction("Index", "Home");

            if (model.Items == null || !model.Items.Any())
            {
                TempData["Error"] = "Phiếu nhập chưa có sản phẩm nào.";
                return RedirectToAction("ChiTiet", new { id = model.MaSP });
            }

            try
            {
                var maNV = HttpContext.Session.GetString("MaNV");
                if (string.IsNullOrEmpty(maNV))
                {
                    TempData["Error"] = "Không xác định được nhân viên đang đăng nhập.";
                    return RedirectToAction("ChiTiet", new { id = model.MaSP });
                }

                // Tự sinh MaNH dạng NH0001, NH0002...
                int soNhapHang = await _context.NhapHangs.CountAsync() + 1;
                string maNH = "NH" + soNhapHang.ToString("D4");

                while (await _context.NhapHangs.AnyAsync(x => x.MaNH == maNH))
                {
                    soNhapHang++;
                    maNH = "NH" + soNhapHang.ToString("D4");
                }

                var nhapHang = new NhapHang
                {
                    MaNH = maNH,
                    MaNV = maNV,
                    NgayNhap = DateTime.Now,
                    GhiChu = model.GhiChuChung
                };

                _context.NhapHangs.Add(nhapHang);

                int soChiTiet = await _context.ChiTietNhapHangs.CountAsync() + 1;

                foreach (var item in model.Items)
                {
                    if (string.IsNullOrWhiteSpace(item.MaCTSP) || item.SoLuongNhap <= 0 || item.GiaNhap < 0)
                        continue;

                    string maCTNH = "CTNH" + soChiTiet.ToString("D4");
                    while (await _context.ChiTietNhapHangs.AnyAsync(x => x.MaCTNH == maCTNH))
                    {
                        soChiTiet++;
                        maCTNH = "CTNH" + soChiTiet.ToString("D4");
                    }

                    var chiTietNhap = new ChiTietNhapHang
                    {
                        MaCTNH = maCTNH,
                        MaNH = maNH,
                        MaCTSP = item.MaCTSP,
                        SoLuongNhap = item.SoLuongNhap,
                        GiaNhap = item.GiaNhap
                    };

                    _context.ChiTietNhapHangs.Add(chiTietNhap);
                    soChiTiet++;
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Tạo phiếu nhập {maNH} thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("ChiTiet", new { id = model.MaSP });
        }

        public async Task<IActionResult> LichSu()
        {
            var role = (HttpContext.Session.GetString("Role") ?? "").Trim();

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "TaiKhoan");

            if (role != "Admin" && role != "QuanLyKho")
                return RedirectToAction("Index", "Home");

            var dsNhapHang = await _context.NhapHangs
                .Include(x => x.NhanVien)
                .OrderByDescending(x => x.NgayNhap)
                .ToListAsync();

            return View(dsNhapHang);
        }

        public async Task<IActionResult> ChiTietLichSu(string id)
        {
            var role = (HttpContext.Session.GetString("Role") ?? "").Trim();

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "TaiKhoan");

            if (role != "Admin" && role != "QuanLyKho")
                return RedirectToAction("Index", "Home");

            if (string.IsNullOrEmpty(id))
                return NotFound();

            var nhapHang = await _context.NhapHangs
                .Include(x => x.NhanVien)
                .FirstOrDefaultAsync(x => x.MaNH == id);

            if (nhapHang == null)
                return NotFound();

            var chiTietNhap = await _context.ChiTietNhapHangs
                .Include(x => x.ChiTietSP)
                .ThenInclude(x => x.SanPham)
                .Where(x => x.MaNH == id)
                .ToListAsync();

            ViewBag.NhapHang = nhapHang;
            return View(chiTietNhap);
        }

    }
}