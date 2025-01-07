using Microsoft.AspNetCore.Mvc;
using SWII6P2_WEBSITE.Models;
using System.Text;
using System.Text.Json;

namespace SWII6P2_WEBSITE.Controllers
{
    public class UserAuthController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Login(string name, string password)
        {
            // Verifica se os dados do usuário estão completos
            if (!SWII6P2Verifications.Verifications.IsUserFull(name, password))
            {
                TempData["Error"] = "Dados incompletos ou inválidos.";
                return View();
            }

            string url = "http://localhost:5133/api/User/login";

            // Criação do objeto usuário
            User user = new User
            {
                Id = 0,
                Name = name,
                Password = password,
                Status = true
            };

            string json = JsonSerializer.Serialize(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Lê o token da resposta
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var responseData = JsonSerializer.Deserialize<TokenResponse>(responseContent);

                        if (responseData?.Token != null)
                        {
                            // Armazenar o token no cookie
                            var cookieOptions = new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTime.UtcNow.AddHours(2)
                            };

                            Response.Cookies.Append("jwtToken", responseData.Token, cookieOptions);

                            // Redirecionar para a view correta
                            return RedirectToAction("Index", "Products");
                        }
                    }
                    else
                    {
                        TempData["Error"] = "Login falhou. Verifique suas credenciais.";
                    }
                }
                catch (Exception ex)
                {
                    // Exibir mensagem de erro na View
                    TempData["Error"] = $"Erro ao realizar login: {ex.Message}";
                }
            }

            return View();
        }
    }

}
