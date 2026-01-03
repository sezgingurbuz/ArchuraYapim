using Microsoft.AspNetCore.Mvc;

namespace basics.Controllers;

public class PagesController : Controller
{
    public IActionResult Kvkk()
    {
        ViewData["Title"] = "KVKK Aydınlatma Metni";
        return View();
    }

    public IActionResult KullanimSartlari()
    {
        ViewData["Title"] = "Kullanım Şartları";
        return View();
    }

    public IActionResult IadeIptal()
    {
        ViewData["Title"] = "İade ve İptal Koşulları";
        return View();
    }

    public IActionResult Iletisim()
    {
        ViewData["Title"] = "İletişim";
        return View();
    }
}
