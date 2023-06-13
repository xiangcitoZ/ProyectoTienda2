using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MvcAWSApiConciertosMySql.Helpers;
using MvcAWSApiConciertosMySql.Models;
using Newtonsoft.Json;
using ProyectoTienda2.Services;
using PyoyectoNugetTienda;
using System.Security.Claims;

namespace ProyectoTienda2.Controllers
{
    public class ManagedController : Controller
    {
        private ServiceApi service;
        private ServiceStorageS3 serviceS3;
        private string BucketUrl;
        string miSecreto = HelperSecretManager.GetSecretAsync().Result;

        public ManagedController(ServiceApi service, ServiceStorageS3 serviceS3, IConfiguration configuration)
        {
            KeysModel model = JsonConvert.DeserializeObject<KeysModel>(miSecreto);
            this.service = service;
            this.serviceS3 = serviceS3;
            this.BucketUrl = model.BucketUrl;
        }

        #region CLIENTE

        public IActionResult LoginCliente()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginCliente(string email
            , string password)
        {
            Cliente cliente =
                await this.service.ExisteCliente(email, password);
            if (cliente != null)
            {
                ClaimsIdentity identity =
               new ClaimsIdentity
               (CookieAuthenticationDefaults.AuthenticationScheme
               , ClaimTypes.Name, ClaimTypes.Role);
                identity.AddClaim
                    (new Claim(ClaimTypes.Name, cliente.Email));
                identity.AddClaim
                    (new Claim(ClaimTypes.NameIdentifier, cliente.IdCliente.ToString()));
                identity.AddClaim
                    (new Claim("Nombre", cliente.Nombre));
                identity.AddClaim
                    (new Claim("Apellidos", cliente.Apellidos));
                identity.AddClaim
                    (new Claim("Imagen", cliente.Imagen));
                identity.AddClaim
                    (new Claim(ClaimTypes.Role, "Cliente"));

                ClaimsPrincipal user = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync
                    (CookieAuthenticationDefaults.AuthenticationScheme
                    , user);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewData["MENSAJE"] = "Usuario/Password incorrectos";
                return View();
            }
        }

        public IActionResult RegisterCliente()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterCliente
            (string nombre, string apellidos, string email, string password, IFormFile file)
        {
            string bucketName = file.FileName;
            using (Stream stream = file.OpenReadStream())
            {
                await this.serviceS3.UploadFileAsync(bucketName, stream);
            }
            await this.service.RegistrarClienteAsync
                (nombre, apellidos, email, password, bucketName);
            ViewData["MENSAJE"] = "Usuario registrado correctamente";
            return View();
        }

        #endregion

        #region ARTISTA

        public IActionResult LoginArtista()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginArtista(string email
            , string password)
        {
            ViewData["BUCKETURL"] = this.BucketUrl;
            Artista artista =
            await this.service.ExisteArtista(email, password);
            if (artista != null)
            {
                ClaimsIdentity identity =
               new ClaimsIdentity
               (CookieAuthenticationDefaults.AuthenticationScheme
               , ClaimTypes.Name, ClaimTypes.Role);
                identity.AddClaim
                    (new Claim(ClaimTypes.Name, artista.Email));
                identity.AddClaim
                    (new Claim(ClaimTypes.NameIdentifier, artista.IdArtista.ToString()));
                identity.AddClaim
                    (new Claim("Nombre", artista.Nombre));
                if (artista.Apellidos == null)
                {
                    identity.AddClaim
                    (new Claim("Apellidos", ""));
                }
                else
                {
                    identity.AddClaim
                    (new Claim("Apellidos", artista.Apellidos));
                }
                identity.AddClaim
                    (new Claim("Imagen", artista.Imagen));
                identity.AddClaim
                    (new Claim(ClaimTypes.Role, "Artista"));

                ClaimsPrincipal user = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync
                    (CookieAuthenticationDefaults.AuthenticationScheme
                    , user);
                
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewData["MENSAJE"] = "Usuario/Password incorrectos";
                return View();
            }
        }

        public IActionResult RegisterArtista()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterArtista
            (string nombre, string apellidos, string nick, string descripcion,
            string email, string password, IFormFile file, string imagenfondo)
        {
            string bucketName = file.FileName;

            using (Stream stream = file.OpenReadStream())
            {
                await this.serviceS3.UploadFileAsync(bucketName, stream);
            }
            imagenfondo = "default.jpg";
            await this.service.RegistrarArtistaAsync
                (nombre, apellidos, nick, descripcion,
                email, password, bucketName, imagenfondo);
            ViewData["MENSAJE"] = "Usuario registrado correctamente";
            return View();
        }

        #endregion

        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync
                (CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult ErrorAcceso()
        {
            return View();
        }
    }
}
