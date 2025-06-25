using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

public class AuthController : Controller
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(string login, string password)
    {
        var expectedLogin = _config["AdminCredentials:Login"];
        var expectedPassword = _config["AdminCredentials:Password"];

        if (login == expectedLogin && password == expectedPassword)
        {
            HttpContext.Session.SetString("IsAdmin", "true");
            return RedirectToAction("EditPanel", "Phones");
        }

        ViewBag.Error = "Неверный логин или пароль";
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
