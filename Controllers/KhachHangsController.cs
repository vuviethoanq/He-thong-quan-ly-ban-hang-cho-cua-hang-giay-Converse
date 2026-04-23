using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Converse_NMCNPM.Data;
using Converse_NMCNPM.Models;

namespace Converse_NMCNPM.Controllers
{
    public class KhachHangsController : Controller
    {
        private readonly AppDbContext _context;

        public KhachHangsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: KhachHangs
        public async Task<IActionResult> Index()
        {
            return View(await _context.KhachHangs.ToListAsync());
        }

        // GET: KhachHangs/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(x => x.MaKH == id);

            if (khachHang == null)
                return NotFound();

            var donHangs = await _context.DonHangs
                .Where(x => x.MaKH == id)
                .ToListAsync();

            var maDonHangs = donHangs.Select(x => x.MaDH).ToList();

            var chiTietDonHangs = await _context.ChiTietDonHangs
                .Include(x => x.DonHang)
                .Include(x => x.ChiTietSP)
                .ThenInclude(x => x.SanPham)
                .Where(x => maDonHangs.Contains(x.MaDH))
                .ToListAsync();

            ViewBag.DonHangs = donHangs;
            ViewBag.ChiTietDonHangs = chiTietDonHangs;

            return View(khachHang);
        }

        // GET: KhachHangs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: KhachHangs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaKH,TenKH,SdtKH,DiaChiKH")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                _context.Add(khachHang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(khachHang);
        }
    }
}
