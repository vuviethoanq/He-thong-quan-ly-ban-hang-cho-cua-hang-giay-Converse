using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Converse_NMCNPM.Data;
using Converse_NMCNPM.Models;

namespace Converse_NMCNPM.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly AppDbContext _context;

        public TaiKhoanController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // LOGIN / LOGOUT
        // =========================

        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("User") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.NhanViens
                .FirstOrDefault(x => x.Username == username && x.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
                return View();
            }

            user.DangLamViec = true;
            user.GioBatDauLam = DateTime.Now;

            _context.Update(user);
            _context.SaveChanges();

            HttpContext.Session.SetString("User", user.Username);
            HttpContext.Session.SetString("Role", user.Role.Trim());
            HttpContext.Session.SetString("MaNV", user.MaNV);

            var role = user.Role.Trim();

            if (role == "Staff")
                return RedirectToAction("Index", "SanPhams");

            if (role == "Admin" || role == "QuanLyKho")
                return RedirectToAction("Index", "DanhSachSP");

            return RedirectToAction("Login", "TaiKhoan");
        }
        // =========================
        // CHECK LOGIN / ADMIN
        // =========================

        private IActionResult CheckLogin()
        {
            if (HttpContext.Session.GetString("User") == null)
                return RedirectToAction(nameof(Login));

            return null;
        }

        private IActionResult CheckAdmin()
        {
            var login = CheckLogin();
            if (login != null) return login;

            var role = (HttpContext.Session.GetString("Role") ?? "").Trim();

            if (role != "Admin")
                return RedirectToAction("Index", "Home");

            return null;
        }

        // =========================
        // CRUD TÀI KHOẢN (ADMIN ONLY)
        // =========================

        public async Task<IActionResult> Index()
        {
            var auth = CheckLogin();
            if (auth != null) return auth;

            var role = HttpContext.Session.GetString("Role");
            var user = HttpContext.Session.GetString("User");

            if (role == "Admin")
            {
                return View(await _context.NhanViens.ToListAsync());
            }

            var nv = await _context.NhanViens.FirstOrDefaultAsync(x => x.Username == user);
            return View(new List<NhanVien> { nv! });
        }
        public async Task<IActionResult> Details(string id)
        {
            var auth = CheckAdmin();
            if (auth != null) return auth;

            if (id == null) return NotFound();

            var nv = await _context.NhanViens
                .FirstOrDefaultAsync(x => x.MaNV == id);

            if (nv == null) return NotFound();

            return View(nv);
        }

        public IActionResult Create()
        {
            var auth = CheckAdmin();
            if (auth != null) return auth;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhanVien nv)
        {
            var auth = CheckAdmin();
            if (auth != null) return auth;

            if (_context.NhanViens.Any(x => x.Username == nv.Username))
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại";
                return View(nv);
            }

            if (ModelState.IsValid)
            {
                _context.Add(nv);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(nv);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var auth = CheckLogin();
            if (auth != null) return auth;

            if (id == null) return NotFound();

            var nv = await _context.NhanViens.FindAsync(id);
            if (nv == null) return NotFound();

            var role = HttpContext.Session.GetString("Role");
            var user = HttpContext.Session.GetString("User");

            // Staff chỉ sửa chính mình
            if (role != "Admin" && nv.Username != user)
                return RedirectToAction("Index", "Home");

            return View(nv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaNV,Username,Password,Role,DangLamViec,GioBatDauLam,TongGioLam")] NhanVien nhanVien)
        {
            if (id != nhanVien.MaNV)
            {
                return NotFound();
            }

            var existedUsername = await _context.NhanViens
                .FirstOrDefaultAsync(x => x.Username == nhanVien.Username && x.MaNV != nhanVien.MaNV);

            if (existedUsername != null)
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                return View(nhanVien);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nhanVien);
                    await _context.SaveChangesAsync();

                    var currentMaNV = HttpContext.Session.GetString("MaNV");

                    if (currentMaNV == nhanVien.MaNV)
                    {
                        HttpContext.Session.SetString("User", nhanVien.Username);
                        HttpContext.Session.SetString("Role", nhanVien.Role.Trim());
                        HttpContext.Session.SetString("MaNV", nhanVien.MaNV);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.NhanViens.Any(e => e.MaNV == nhanVien.MaNV))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction("Index", "TaiKhoan");
            }

            return View(nhanVien);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var auth = CheckAdmin();
            if (auth != null) return auth;

            if (id == null) return NotFound();

            var nv = await _context.NhanViens
                .FirstOrDefaultAsync(x => x.MaNV == id);

            if (nv == null) return NotFound();

            return View(nv);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var auth = CheckAdmin();
            if (auth != null) return auth;

            var nv = await _context.NhanViens.FindAsync(id);
            if (nv == null) return RedirectToAction(nameof(Index));

            var currentUser = HttpContext.Session.GetString("User");

            // Không xóa chính mình
            if (nv.Username == currentUser)
            {
                TempData["Error"] = "Không thể xóa tài khoản đang đăng nhập";
                return RedirectToAction(nameof(Index));
            }

            // Không xóa admin cuối cùng
            if (nv.Role == "Admin")
            {
                int countAdmin = _context.NhanViens.Count(x => x.Role == "Admin");

                if (countAdmin <= 1)
                {
                    TempData["Error"] = "Phải còn ít nhất 1 Admin";
                    return RedirectToAction(nameof(Index));
                }
            }

            _context.NhanViens.Remove(nv);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa thành công";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // ĐỔI MẬT KHẨU
        // =========================

        public IActionResult DoiMatKhau()
        {
            var auth = CheckLogin();
            if (auth != null) return auth;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(string newUsername, string oldPass, string newPass)
        {
            var auth = CheckLogin();
            if (auth != null) return auth;

            var currentUser = HttpContext.Session.GetString("User");

            var user = _context.NhanViens
                .FirstOrDefault(x => x.Username == currentUser);

            if (user == null)
                return RedirectToAction(nameof(Login));

            if (user.Password != oldPass)
            {
                ViewBag.Error = "Mật khẩu cũ không đúng";
                return View();
            }

            // kiểm tra trùng username
            bool exists = _context.NhanViens.Any(x =>
                x.Username == newUsername && x.MaNV != user.MaNV);

            if (exists)
            {
                ViewBag.Error = "Tên tài khoản đã tồn tại";
                return View();
            }

            user.Username = newUsername;
            user.Password = newPass;

            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("User", user.Username);

            ViewBag.Success = "Cập nhật tài khoản thành công";
            return View();
        }

        public IActionResult Logout()
        {
            var maNV = HttpContext.Session.GetString("MaNV");

            if (!string.IsNullOrEmpty(maNV))
            {
                var user = _context.NhanViens.FirstOrDefault(x => x.MaNV == maNV);

                if (user != null && user.DangLamViec && user.GioBatDauLam != null)
                {
                    var soGioLam = (decimal)(DateTime.Now - user.GioBatDauLam.Value).TotalHours;

                    user.TongGioLam += Math.Round(soGioLam, 2);
                    user.DangLamViec = false;
                    user.GioBatDauLam = null;

                    _context.Update(user);
                    _context.SaveChanges();
                }
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login", "TaiKhoan");
        }
    }
}