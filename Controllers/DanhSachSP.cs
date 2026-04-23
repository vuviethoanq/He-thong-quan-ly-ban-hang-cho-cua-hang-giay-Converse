using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Converse_NMCNPM.Data;

namespace Converse_NMCNPM.Controllers
{
    public class DanhSachSPController : Controller
    {
        private readonly AppDbContext _context;

        public DanhSachSPController(AppDbContext context)
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
            var role = (HttpContext.Session.GetString("Role") ?? "").Trim();

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "TaiKhoan");

            if (role != "Admin" && role != "QuanLyKho")
                return RedirectToAction("Index", "Home");

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
    }
}