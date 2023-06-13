using Microsoft.AspNetCore.Mvc;
using MvcAWSApiConciertosMySql.Helpers;
using MvcAWSApiConciertosMySql.Models;
using Newtonsoft.Json;
using ProyectoTienda2.Services;
using PyoyectoNugetTienda;
using System.Security.Claims;

namespace ProyectoTienda2.Controllers
{
    public class ArtistaController : Controller
    {
        private ServiceApi service;
        private ServiceStorageS3 serviceS3;
        private string BucketUrl;
        string miSecreto = HelperSecretManager.GetSecretAsync().Result;
        public ArtistaController
            (ServiceApi service, ServiceStorageS3 serviceS3, IConfiguration configuration)
        {
            KeysModel model = JsonConvert.DeserializeObject<KeysModel>(miSecreto);
            this.service = service;
            this.serviceS3 = serviceS3;
            this.BucketUrl = model.BucketUrl;
        }

        public async Task<IActionResult> DetallesArtista(int idartista)
        {

            DatosArtista artista = await this.service.DetailsArtistaAsync(idartista);
            ViewData["CONTARPRODUCT"] = artista.listaProductos.Count;
            ViewData["PERFIL"] = this.BucketUrl + artista.artista.Imagen;
            ViewData["FOTOFONDO"] = this.BucketUrl + artista.artista.ImagenFondo;
            ViewData["BUCKETURL"] = this.BucketUrl;
            return View(artista);
        }
        public async Task<IActionResult> PerfilArtista
            (int idartista, int? idInfoArteEliminado)
        {
            DatosArtista artista = new DatosArtista();

            if (idInfoArteEliminado != null)
            {
                await this.service.DeleteInfoArteAsync(idInfoArteEliminado.Value);
            }

            artista = await this.service.DetailsArtistaAsync(idartista);
            ViewData["CONTARPRODUCT"] = artista.listaProductos.Count;
            ViewData["PERFIL"] = this.BucketUrl + artista.artista.Imagen;
            ViewData["FOTOFONDO"] = this.BucketUrl + artista.artista.ImagenFondo;
            ViewData["BUCKETURL"] = this.BucketUrl;
            return View(artista);
        }

        [HttpPost]
        public async Task<IActionResult> PerfilArtista
            (int idartista, IFormFile fileFondo)
        {
            string bucketName = fileFondo.FileName;
            using (Stream stream = fileFondo.OpenReadStream())
            {
                await this.serviceS3.UploadFileAsync(bucketName, stream);
            }
            await this.service.CambiarImagenFondoAsync(idartista, bucketName);
            return RedirectToAction("PerfilArtista", new { idartista = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value) });
        }

        public async Task<IActionResult> EditarPerfilArtista(int idartista)
        {
            DatosArtista artista = await this.service.DetailsArtistaAsync(idartista);
            return View(artista);
        }

        [HttpPost]
        public async Task<IActionResult> EditarPerfilArtista
            (int idartista, string nombre, string apellidos, string nick, string descripcion,
            string email, IFormFile file)
        {
            DatosArtista artista = new DatosArtista();
            string bucketName = file.FileName;
            using (Stream stream = file.OpenReadStream())
            {
                await this.serviceS3.UploadFileAsync(bucketName, stream);
            }
            idartista = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await this.service.PerfilArtista
                (idartista, nombre, apellidos, nick, descripcion,
                email, bucketName);

            return RedirectToAction("PerfilArtista", new { idartista = idartista });
        }
    }
}
