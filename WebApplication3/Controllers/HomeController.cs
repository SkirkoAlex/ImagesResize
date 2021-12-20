using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class HomeController : Controller
    {
        private  ConcurrentQueue<IFormFile> queue = new ConcurrentQueue<IFormFile>();
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            queue.Enqueue(file);
            if (queue.TryDequeue(out file))
            {
                using var image = Image.Load(file.OpenReadStream());

                if (image != null)
                {
                    //Копируем оригинал
                    var fileName = Path.GetFileName(file.FileName);
                    var pathToOrigin = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\content\images", "origin.png");
                    using (var fileStream = new FileStream(pathToOrigin, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    //Ресайзим в маленький
                    image.Mutate(x => x.Resize(30, 30));
                    var pathToSmall = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\content\thumbprint", "small.png");
                    image.Save(pathToSmall);
                    //Ресайзим в большой
                    image.Mutate(x => x.Resize(150, 200));
                    var pathToBig = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\content\thumbprint", "big.png");
                    image.Save(pathToBig);
                }
            }
            
            return Ok();
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
