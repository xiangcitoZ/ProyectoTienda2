using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;
using PyoyectoNugetTienda;
using ProyectoTienda2.Services;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using MvcAWSApiConciertosMySql.Helpers;
using MvcAWSApiConciertosMySql.Models;
using Newtonsoft.Json;

namespace ProyectoTienda2.Controllers
{
    public class ClienteController : Controller
    {
        private ServiceApi service;
        private ServiceStorageS3 serviceS3;
        private string BucketUrl;
        string miSecreto = HelperSecretManager.GetSecretAsync().Result;

        public ClienteController(ServiceApi service, ServiceStorageS3 serviceS3, IConfiguration configuration)
        {
            KeysModel model = JsonConvert.DeserializeObject<KeysModel>(miSecreto);
            this.service = service;
            this.serviceS3 = serviceS3;
            this.BucketUrl = model.BucketUrl;
        }

        public async Task<IActionResult> DetallesCliente(int idcliente)
        {
            idcliente = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            DatosArtista cliente = await this.service.FindCliente(idcliente);

            //ViewData["PERFIL"] = await this.serviceBlob.GetBlobAsync(this.containerName, cliente.cliente.Imagen);

            ViewData["PERFIL"] = this.BucketUrl + cliente.cliente.Imagen;

            return View(cliente);
        }

        public async Task<IActionResult> EditarCliente(int idcliente)
        {
            DatosArtista cliente = new DatosArtista();
            idcliente = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            cliente = await this.service.FindCliente(idcliente);
            //ViewData["PERFIL"] = await this.serviceBlob.GetBlobAsync(this.containerName, cliente.cliente.Imagen);
            ViewData["PERFIL"] = this.BucketUrl + cliente.cliente.Imagen;

            return View(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> EditarCliente
            (int idcliente, string nombre, string apellidos, string email, IFormFile file)
        {
            string bucketName = file.FileName;
            using (Stream stream = file.OpenReadStream())
            {
                await this.serviceS3.UploadFileAsync(bucketName, stream);
            }
            idcliente = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await this.service.EditarClienteAsync
                (idcliente, nombre, apellidos, email, bucketName);
            return RedirectToAction("DetallesCliente");
        }

        public IActionResult ErrorAccesoCliente()
        {
            return View();
        }
    }
}
