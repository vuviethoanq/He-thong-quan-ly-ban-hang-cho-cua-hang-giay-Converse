using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Converse_NMCNPM.Data;
using Converse_NMCNPM.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Converse_NMCNPM.Controllers
{
    public class DonHangsController : Controller
    {
        private readonly AppDbContext _context;

        public DonHangsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DonHangs
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.DonHangs.Include(d => d.KhachHang).Include(d => d.NhanVien);
            return View(await appDbContext.ToListAsync());
        }

        // GET: DonHangs/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var donHang = await _context.DonHangs
                .Include(x => x.KhachHang)
                .Include(x => x.NhanVien)
                .FirstOrDefaultAsync(x => x.MaDH == id);

            if (donHang == null)
                return NotFound();

            var chiTietDonHangs = await _context.ChiTietDonHangs
                .Include(x => x.ChiTietSP)
                .ThenInclude(x => x.SanPham)
                .Where(x => x.MaDH == id)
                .ToListAsync();

            ViewBag.ChiTietDonHangs = chiTietDonHangs;

            return View(donHang);
        }

        // GET: DonHangs/Create
        public async Task<IActionResult> Create()
        {
            var role = (HttpContext.Session.GetString("Role") ?? "").Trim();

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "TaiKhoan");

            ViewBag.ChiTietSPs = await _context.ChiTietSPs
                .Select(x => new
                {
                    x.MaCTSP,
                    HienThi = x.MaCTSP + " - " + x.MauSP + " - Size " + x.SizeSP
                })
                .ToListAsync();

            return View(new TaoDonHangViewModel());
        }

        // POST: DonHangs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaoDonHangViewModel model)
        {
            var role = (HttpContext.Session.GetString("Role") ?? "").Trim();
            var maNV = HttpContext.Session.GetString("MaNV");

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "TaiKhoan");

            if (string.IsNullOrEmpty(maNV))
            {
                ModelState.AddModelError("", "Không xác định được nhân viên đang đăng nhập.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ChiTietSPs = await _context.ChiTietSPs
                    .Select(x => new
                    {
                        x.MaCTSP,
                        HienThi = x.MaCTSP + " - " + x.MauSP + " - Size " + x.SizeSP
                    })
                    .ToListAsync();

                return View(model);
            }

            try
            {
                using var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "sp_TaoDonHang";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@TenKH", model.TenKH));
                command.Parameters.Add(new SqlParameter("@SDT", model.SDT));
                command.Parameters.Add(new SqlParameter("@DiaChi", (object?)model.DiaChi ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@MaNV", maNV));
                command.Parameters.Add(new SqlParameter("@MaCTSP", model.MaCTSP));
                command.Parameters.Add(new SqlParameter("@SoLuong", model.SoLuong));

                using var reader = await command.ExecuteReaderAsync();

                TempData["Success"] = "Tạo đơn hàng thành công.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);

                ViewBag.ChiTietSPs = await _context.ChiTietSPs
                    .Select(x => new
                    {
                        x.MaCTSP,
                        HienThi = x.MaCTSP + " - " + x.MauSP + " - Size " + x.SizeSP
                    })
                    .ToListAsync();

                return View(model);
            }
        }
    }
}
