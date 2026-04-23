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
    public class SanPhamsController : Controller
    {
        private readonly AppDbContext _context;

        public SanPhamsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: SanPhams
        public async Task<IActionResult> Index(string keyword = "", string sortOrder = "", int page = 1)
        {
            int pageSize = 9;

            var sanPhams = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .ToListAsync();

            var chiTietSPs = await _context.ChiTietSPs.ToListAsync();

            var data = sanPhams.Select(s =>
            {
                var chiTietCuaSP = chiTietSPs.Where(ct => ct.MaSP == s.MaSP).ToList();

                var discountLonNhat = chiTietCuaSP.Any()
                    ? chiTietCuaSP.Max(ct => ct.Discount)
                    : 0;

                var hinhAnh = chiTietCuaSP
                    .FirstOrDefault(ct => !string.IsNullOrEmpty(ct.HinhAnh))
                    ?.HinhAnh;

                return new SanPhamCardViewModel
                {
                    MaSP = s.MaSP,
                    TenSP = s.TenSP,
                    TenDM = s.DanhMuc != null ? s.DanhMuc.TenDM : "",
                    GiaGoc = s.GiaSP,
                    DiscountLonNhat = discountLonNhat,
                    GiaHienThi = s.GiaSP * (100 - discountLonNhat) / 100,
                    HinhAnh = hinhAnh
                };
            }).ToList();

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();
                data = data.Where(x =>
                    x.TenSP.ToLower().Contains(keyword) ||
                    x.TenDM.ToLower().Contains(keyword)
                ).ToList();
            }

            // Sắp xếp
            data = sortOrder switch
            {
                "name_asc" => data.OrderBy(x => x.TenSP).ToList(),
                "name_desc" => data.OrderByDescending(x => x.TenSP).ToList(),
                "price_asc" => data.OrderBy(x => x.GiaHienThi).ToList(),
                "price_desc" => data.OrderByDescending(x => x.GiaHienThi).ToList(),
                _ => data.OrderBy(x => x.MaSP).ToList()
            };

            int totalItems = data.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var pagedData = data
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Keyword = keyword;
            ViewBag.SortOrder = sortOrder;

            return View(pagedData);
        }

        // GET: SanPhams/Details/5
        public IActionResult Details(string id)
        {
            return RedirectToAction("Index", "ChiTietSPs", new { id });
        }

        // GET: SanPhams/Create
        public IActionResult Create()
        {
            ViewData["MaDM"] = new SelectList(_context.DanhMucs, "MaDM", "MaDM");
            return View();
        }

        // POST: SanPhams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaSP,TenSP,GiaSP,MaDM")] SanPham sanPham)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sanPham);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaDM"] = new SelectList(_context.DanhMucs, "MaDM", "MaDM", sanPham.MaDM);
            return View(sanPham);
        }

        // GET: SanPhams/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham == null)
            {
                return NotFound();
            }
            ViewData["MaDM"] = new SelectList(_context.DanhMucs, "MaDM", "MaDM", sanPham.MaDM);
            return View(sanPham);
        }

        // POST: SanPhams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaSP,TenSP,GiaSP,MaDM")] SanPham sanPham)
        {
            if (id != sanPham.MaSP)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sanPham);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SanPhamExists(sanPham.MaSP))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaDM"] = new SelectList(_context.DanhMucs, "MaDM", "MaDM", sanPham.MaDM);
            return View(sanPham);
        }

        // GET: SanPhams/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .FirstOrDefaultAsync(m => m.MaSP == id);
            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }

        // POST: SanPhams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham != null)
            {
                _context.SanPhams.Remove(sanPham);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SanPhamExists(string id)
        {
            return _context.SanPhams.Any(e => e.MaSP == id);
        }
    }
}