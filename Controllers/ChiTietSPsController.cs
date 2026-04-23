using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Converse_NMCNPM.Data;
using Converse_NMCNPM.Models;

namespace Converse_NMCNPM.Controllers
{
    public class ChiTietSPsController : Controller
    {
        private readonly AppDbContext _context;

        public ChiTietSPsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var sanPham = _context.SanPhams
                .Include(x => x.DanhMuc)
                .FirstOrDefault(x => x.MaSP == id);

            if (sanPham == null)
                return NotFound();

            var variants = _context.ChiTietSPs
                .Where(x => x.MaSP == id)
                .OrderBy(x => x.MauSP)
                .ThenBy(x => x.SizeSP)
                .ToList();

            ViewBag.SanPham = sanPham;
            ViewBag.Variants = variants;
            ViewBag.Colors = variants.Select(x => x.MauSP).Distinct().ToList();
            ViewBag.DefaultVariant = variants.FirstOrDefault();

            return View(sanPham);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var chiTietSP = await _context.ChiTietSPs.FindAsync(id);
            if (chiTietSP == null)
                return NotFound();

            return View(chiTietSP);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ChiTietSP chiTietSP, IFormFile? imageFile)
        {
            if (id != chiTietSP.MaCTSP)
                return NotFound();

            var oldData = await _context.ChiTietSPs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaCTSP == id);

            if (oldData == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var extension = Path.GetExtension(imageFile.FileName).ToLower();
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Chỉ cho phép file ảnh .jpg, .jpeg, .png, .webp");
                        return View(chiTietSP);
                    }

                    var fileName = $"{chiTietSP.MaCTSP}_{Guid.NewGuid():N}{extension}";
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    chiTietSP.HinhAnh = fileName;
                }
                else
                {
                    chiTietSP.HinhAnh = oldData.HinhAnh;
                }

                _context.Update(chiTietSP);
                await _context.SaveChangesAsync();

                return RedirectToAction("ChiTiet", "DanhSachSP", new { id = chiTietSP.MaSP });
            }

            return View(chiTietSP);
        }
    }
}