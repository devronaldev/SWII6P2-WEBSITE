using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SWII6P2_WEBSITE.Models;
using System.Diagnostics;

namespace SWII6P2_WEBSITE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient client;

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
        {
            _logger = logger;
            client = httpClient;
        }

        [HttpPost]
        public async Task<IActionResult> Login(string nameInput, string passwordInput)
        {
            var loginData = new User
            {
                Id = 0,
                Name = nameInput,
                Password = passwordInput,
                Status = true
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(loginData),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = await client.PostAsync("http://localhost:5133/api/User/login", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, options);
                        TempData["Error"] = $"Erro {response.StatusCode}: {errorResponse?.Message ?? "Erro desconhecido."}";
                    }
                    catch
                    {
                        // Caso a resposta não seja no formato esperado
                        TempData["Error"] = $"Erro {response.StatusCode}: {responseContent}";
                    }

                    return RedirectToAction("Index");
                }

                var successContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<TokenResponse>(successContent);

                if (responseData != null && !string.IsNullOrEmpty(responseData.Token))
                {
                    Response.Cookies.Append("AuthToken", responseData.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        Expires = DateTimeOffset.Now.AddHours(2)
                    });

                    return RedirectToAction("ListaProdutos");
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erro ao processar a requisição: " + ex.Message;
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ListaProdutos()
        {
            List<Product> produtos = new List<Product>();

            try
            {
                var token = Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync("http://localhost:5133/api/Product");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    produtos = JsonSerializer.Deserialize<List<Product>>(content);

                    if (produtos == null || produtos.Count > 0)
                    {
                        TempData["Error"] = "Sem produtos";
                    }
                }
                else
                {
                    TempData["Error"] = "Erro ao carregar produtos.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erro: " + ex.Message;
            }

            return View(produtos);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Credits()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
