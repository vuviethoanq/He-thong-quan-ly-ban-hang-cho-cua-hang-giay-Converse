using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Converse_NMCNPM.Data;
using Converse_NMCNPM.Models;

namespace Converse_NMCNPM.Controllers
{
    public class GioHangController : Controller
    {
        private readonly AppDbContext _context;
        private const string CartSessionKey = "GIO_HANG";

        public GioHangController(AppDbContext context)
        {
            _context = context;
        }

        private List<GioHangItemViewModel> GetCart()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json))
                return new List<GioHangItemViewModel>();

            return JsonSerializer.Deserialize<List<GioHangItemViewModel>>(json) ?? new List<GioHangItemViewModel>();
        }

        private void SaveCart(List<GioHangItemViewModel> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }

        public IActionResult Index()
        {
            var model = new GioHangViewModel
            {
                Items = GetCart()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ThemVaoGio(string maCTSP, int soLuong)
        {
            if (string.IsNullOrEmpty(maCTSP) || soLuong <= 0)
                return RedirectToAction("Index", "SanPhams");

            var chiTiet = await _context.ChiTietSPs
                .Include(x => x.SanPham)
                .FirstOrDefaultAsync(x => x.MaCTSP == maCTSP);

            if (chiTiet == null || chiTiet.SanPham == null)
                return RedirectToAction("Index", "SanPhams");

            var donGia = chiTiet.SanPham.GiaSP * (100 - chiTiet.Discount) / 100;

            var cart = GetCart();
            var existing = cart.FirstOrDefault(x => x.MaCTSP == maCTSP);

            if (existing != null)
            {
                existing.SoLuong += soLuong;
            }
            else
            {
                cart.Add(new GioHangItemViewModel
                {
                    MaCTSP = chiTiet.MaCTSP,
                    TenSP = chiTiet.SanPham.TenSP,
                    MauSP = chiTiet.MauSP,
                    SizeSP = chiTiet.SizeSP,
                    SoLuong = soLuong,
                    DonGia = donGia,
                    HinhAnh = chiTiet.HinhAnh
                });
            }

            SaveCart(cart);

            TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng.";
            return RedirectToAction("Index", "SanPhams");
        }

        [HttpPost]
        public IActionResult Xoa(string maCTSP)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MaCTSP == maCTSP);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToan(GioHangViewModel model)
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng đang trống.";
                return RedirectToAction("Index");
            }

            var maNV = HttpContext.Session.GetString("MaNV");
            if (string.IsNullOrEmpty(maNV))
                return RedirectToAction("Login", "TaiKhoan");

            if (!ModelState.IsValid)
            {
                model.Items = cart;
                return View("Index", model);
            }

            // Tạo hoặc lấy khách hàng
            var kh = await _context.KhachHangs.FirstOrDefaultAsync(x => x.SdtKH == model.SDT);
            if (kh == null)
            {
                int sttKH = await _context.KhachHangs.CountAsync() + 1;
                string maKH = "KH" + sttKH.ToString("D4");
                while (await _context.KhachHangs.AnyAsync(x => x.MaKH == maKH))
                {
                    sttKH++;
                    maKH = "KH" + sttKH.ToString("D4");
                }

                kh = new KhachHang
                {
                    MaKH = maKH,
                    TenKH = model.TenKH,
                    SdtKH = model.SDT,
                    DiaChiKH = model.DiaChi
                };

                _context.KhachHangs.Add(kh);
                await _context.SaveChangesAsync();
            }

            // Tạo đơn hàng
            int sttDH = await _context.DonHangs.CountAsync() + 1;
            string maDH = "DH" + sttDH.ToString("D4");
            while (await _context.DonHangs.AnyAsync(x => x.MaDH == maDH))
            {
                sttDH++;
                maDH = "DH" + sttDH.ToString("D4");
            }

            var donHang = new DonHang
            {
                MaDH = maDH,
                MaKH = kh.MaKH,
                MaNV = maNV,
                NgayTao = DateTime.Now,
                TongTien = cart.Sum(x => x.ThanhTien)
            };

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();

            // Tạo chi tiết đơn hàng
            int sttCT = await _context.ChiTietDonHangs.CountAsync() + 1;

            foreach (var item in cart)
            {
                string maCTDH = "CTDH" + sttCT.ToString("D4");
                while (await _context.ChiTietDonHangs.AnyAsync(x => x.MaCTDH == maCTDH))
                {
                    sttCT++;
                    maCTDH = "CTDH" + sttCT.ToString("D4");
                }

                _context.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    MaCTDH = maCTDH,
                    MaDH = maDH,
                    MaCTSP = item.MaCTSP,
                    SoLuong = item.SoLuong,
                    Gia = item.DonGia
                });

                sttCT++;
            }

            await _context.SaveChangesAsync();

            // Tạo thanh toán
            int sttTT = await _context.ThanhToans.CountAsync() + 1;
            string maTT = "TT" + sttTT.ToString("D4");
            while (await _context.ThanhToans.AnyAsync(x => x.MaTT == maTT))
            {
                sttTT++;
                maTT = "TT" + sttTT.ToString("D4");
            }

            _context.ThanhToans.Add(new ThanhToan
            {
                MaTT = maTT,
                MaDH = maDH,
                PhuongThuc = model.PhuongThucThanhToan,
                TrangThai = "DaThanhToan",
                ThoiGian = DateTime.Now
            });

            await _context.SaveChangesAsync();

            HttpContext.Session.Remove(CartSessionKey);

            TempData["Success"] = "Thanh toán thành công.";
            return RedirectToAction("Index", "GioHang");
        }
    }
}