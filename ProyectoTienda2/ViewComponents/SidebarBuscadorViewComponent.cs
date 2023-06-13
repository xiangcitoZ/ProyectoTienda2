using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ProyectoTienda2.Services;
using PyoyectoNugetTienda;
using System.Collections.Generic;

namespace ProyectoTienda2.ViewComponents
{
    [ViewComponent(Name = "SidebarBuscador")]
    public class SidebarBuscadorViewComponent : ViewComponent
    {
        private ServiceApi service;
        private string BucketUrl;

        public SidebarBuscadorViewComponent(ServiceApi service, IConfiguration configuration)
        {
            this.service = service;
            this.BucketUrl = configuration.GetValue<string>("AWS:BucketUrl");
        }

        public async Task<IViewComponentResult> InvokeAsync(string query)
        {
            DatosArtista artistas;
                artistas = await this.service.GetArtistasAsync();
            ViewData["BUCKETURL"] = this.BucketUrl;
            return View(artistas);
        }

    }
}
