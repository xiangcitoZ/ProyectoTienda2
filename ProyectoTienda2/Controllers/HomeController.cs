using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using MvcAWSApiConciertosMySql.Helpers;
using MvcAWSApiConciertosMySql.Models;
using Newtonsoft.Json;
using ProyectoTienda2.Extensions;
using ProyectoTienda2.Filters;
using ProyectoTienda2.Services;
using PyoyectoNugetTienda;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;

namespace ProyectoTienda2.Controllers
{
    public class HomeController : Controller
    {

        private ServiceApi service;
        private ServiceStorageS3 serviceS3;
        private ServiceAwsCache serviceAws;
        private string BucketUrl;
        string miSecreto = HelperSecretManager.GetSecretAsync().Result;

        public HomeController
            (ServiceApi service, ServiceStorageS3 serviceS3, IConfiguration configuration, ServiceAwsCache serviceAws)

        {
            KeysModel model = JsonConvert.DeserializeObject<KeysModel>(miSecreto);
            this.service = service;
            this.serviceS3 = serviceS3;
            this.serviceAws = serviceAws;
            this.BucketUrl = model.BucketUrl;
        }

        public async Task<IActionResult> Index(int? idfavorito)
        {
            if(idfavorito != null)
            {
                //List<int> favoritos;
                //if(HttpContext.Session.GetObject<List<int>>("FAVORITOS") == null)
                //{
                //    favoritos = new List<int>();
                //}
                //else
                //{
                //    favoritos = HttpContext.Session.GetObject<List<int>>("FAVORITOS");
                //}
                //favoritos.Add(idfavorito.Value);
                //HttpContext.Session.SetObject("FAVORITOS", favoritos);
                
                DatosArtista cuadro = await this.service.FindInfoArteAsync(idfavorito.Value);
                await this.serviceAws.AddFavoritoAsync(cuadro.infoProducto);
               

            }
            DatosArtista infoArtes = await this.service.GetInfoArteAsync();
            
            ViewData["BUCKETURL"] = this.BucketUrl;

            return View(infoArtes);
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> ProductosFavoritos(int? ideliminar)
        {
            List<int> idsFavoritos =
                HttpContext.Session.GetObject<List<int>>("FAVORITOS");

                ViewData["BUCKETURL"] = this.BucketUrl;


            DatosArtista cuadros = await this.serviceAws.GetFavoritosAsync();
            return View(cuadros);

              
            
        }


        public async Task<IActionResult> EliminarFavorito(int idfavorito)
        {
            await this.serviceAws.DeleteFavoritoAsync(idfavorito);
            return RedirectToAction("ProductosFavoritos");
        }


        public async Task<IActionResult> Details(int idproducto)
        {
            DatosArtista infoProduct = await this.service.FindInfoArteAsync(idproducto);

            ViewData["BUCKETURL"] = this.BucketUrl;

            return View(infoProduct);
        }

        public IActionResult NuevoProducto()
        {
            ViewData["BUCKETURL"] = this.BucketUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NuevoProducto
            (string titulo, int precio, string descripcion, IFormFile file, int idartista)
        {
            string bucketName = file.FileName;
            using (Stream stream = file.OpenReadStream())
            {
                await this.serviceS3.UploadFileAsync(bucketName, stream);
            }

            idartista = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            await this.service.AgregarProductoAsync(titulo, precio, descripcion, bucketName, idartista);
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
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