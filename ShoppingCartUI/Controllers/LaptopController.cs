using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingCartUI.Data;
using ShoppingCartUI.Models;
using System.IO;
using ShoppingCartUI.Utilities;
using RestSharp;
using Newtonsoft.Json;
using Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.Hosting;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;


namespace ShoppingCartUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LaptopController : Controller
    {
        private readonly ApplicationDbContext _context;
#if DEBUG
        private readonly string? _targetFilePath;
#endif
        private readonly string? _apikey;
        private readonly ILogger<LaptopController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IImageUrlRepository _imageUrlRepository;
        private readonly IWebHostEnvironment _hostEnvironment;

        public LaptopController(ApplicationDbContext context, IConfiguration config, ILogger<LaptopController> logger, IWebHostEnvironment hostEnvironment, IImageUrlRepository imageUrlRepository)
        {
            _context = context;
#if DEBUG
            _targetFilePath = config.GetValue<string>("StoredFilesPath");
#endif
            _configuration = config;
            _apikey = config.GetValue<string>("VirusTotal");
            _logger = logger;
            _imageUrlRepository = imageUrlRepository;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Admin/Create
        public IActionResult Create()
        {
            //var currentdict = Directory.GetCurrentDirectory();
            //var wcurr = System.IO.Path.Combine(currentdict, "wwwroot");
            //if(!Directory.Exists(wcurr))
            //    _logger.LogInformation(wcurr);
            //var imgcurr = System.IO.Path.Combine(currentdict, "img");
            //if(!Directory.Exists(imgcurr))
            //    _logger.LogInformation(imgcurr);
            //_logger.LogInformation("This is a create page!" + Directory.GetCurrentDirectory());
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "BrandName");
            return View();
        }

        // POST: Admin/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// Asynchronously creates a new laptop entry.
        /// Uploads the laptop's image file to VirusTotal for scanning.
        /// If the file is safe, saves the laptop entry to the database and redirects to the index page.
        /// If the file is not safe or the ModelState is not valid, adds an error to the ModelState and returns the view with the laptop data.
        /// </summary>
        /// <param name="laptop">form data</param>
        /// <returns>The index view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ModelName,Processor,Price,ImageFile,BrandId")] Laptop laptop)
        {
            var formFileContent =
                await FileHelpers.ProcessFormFile<Laptop>(
                    laptop.ImageFile, ModelState);

            RestResponse response = await ScanVirus("https://www.virustotal.com/api/v3/files", laptop.ImageFile!.FileName, formFileContent);

            if (response.IsSuccessful)
            {
                _logger.LogInformation("File successfully uploaded to VirusTotal, checking report...");

                dynamic? json = JsonConvert.DeserializeObject(response.Content!);
                string resource = json is not null ? json.data.id : "";
                if (await GetReport(resource, _apikey!))
                {
                    if (ModelState.IsValid)
                    {
#if DEBUG
                        // For the file name of uploaded file stored
                        // server side, use Path.GetRandomFileName to generate a safe
                        // random file name.
                        var trustedFileNameForFileStorage = GenerateRandomFileName(laptop.ImageFile.FileName);
                        string? filePath = null;
                        if (_targetFilePath is not null)
                            filePath = Path.Combine(_targetFilePath, trustedFileNameForFileStorage);

                        laptop.ImageFileName = trustedFileNameForFileStorage;
                        _context.Add(laptop);
                        await _context.SaveChangesAsync();
                        using (var fileStream = System.IO.File.Create(filePath))
                        {
                            await fileStream.WriteAsync(formFileContent);

                            // To work directly with a FormFile, use the following
                            // instead:
                            //await FileUpload.FormFile.CopyToAsync(fileStream);
                        }


#elif RELEASE
                        //var fileName = GenerateRandomFileName(laptop.ImageFile.FileName);
                        laptop.ImageFileName = GenerateRandomFileName(laptop.ImageFile.FileName);
                        if (laptop.ImageFileName.EndsWith(".webp"))
                        {
                            //The best practice is to use Azure Blob Container in conjunction with Azure Front Door to show images in img tags.
                            var blobServiceClient = GetBlobServiceClient(_configuration.GetValue<string>("accountName")!);
                            var containerClient = blobServiceClient.GetBlobContainerClient("laptopimageforcart");
                            await _context.Laptops.AddAsync(laptop);
                            _context.SaveChanges();
                            BlobClient blobClient = containerClient.GetBlobClient(laptop.ImageFileName);
                            BinaryData binaryData = new(formFileContent);
                            await blobClient.UploadAsync(binaryData, true);
                            //Download the blob to StoredFilesPath
                            var path = Directory.GetCurrentDirectory();
                            _logger.LogInformation("Path is {path}", path);
                            if (Path.Exists(path))
                            {
                                var uploadedPath = Path.Combine(_hostEnvironment.WebRootPath, "uploadedimg");
                                try
                                {
                                    //_logger.LogInformation($"The Path exists and the Path is {path}. It's ready to download blob.");
                                    //path = System.IO.Path.Combine(path, "img");
                                    //if (!Directory.Exists(path))
                                    //{
                                    //    Directory.CreateDirectory(path);
                                    //}
                                    var filepath = System.IO.Path.Combine(uploadedPath, laptop.ImageFileName);
                                    _logger.LogInformation("The Path exists and the Path is {filepath}. It's ready to download blob.", filepath);
                                    await blobClient.DownloadToAsync(filepath);
                                }
                                catch (RequestFailedException ex)
                                {
                                    _logger.LogInformation("The exception content is {ex.Message}", ex.Message);
                                }
                                catch (DirectoryNotFoundException ex)
                                {
                                    _logger.LogInformation("The exception content is {ex.Message}", ex.Message);
                                }
                                catch (FileNotFoundException ex)
                                {
                                    _logger.LogInformation("The exception content is {ex.Message}", ex.Message);
                                }
                            }
                        }
                        else
                        {
                            RestResponse imgResponse = await UploadImageAsync("https://api.imgur.com/3/image", laptop.ImageFile!.FileName, formFileContent);
                            if (imgResponse.IsSuccessful)
                            {
                                _logger.LogInformation("Image uploaded successfully. Response content:");
                                dynamic? imgjson = JsonConvert.DeserializeObject(imgResponse.Content!);
                                string id = imgjson is not null ? imgjson.data.id : "";
                                string url = imgjson is not null ? imgjson.data.link : "";
                                string deleteHash = imgjson is not null ? imgjson.data.deletehash : "";
                                laptop.ImageFileName = id + Path.GetExtension(laptop.ImageFileName);
                                await _imageUrlRepository.AddImageUrlModelAsync(url, deleteHash, laptop);
                                _logger.LogInformation("{imgResponse.Content}", imgResponse.Content);
                            }
                            else
                            {
                                _logger.LogError("Image upload failed. status code: {imgResponse.StatusCode}, error message: {imgResponse.ErrorMessage}", imgResponse.StatusCode, imgResponse.ErrorMessage);
                            }
                            
                        }
#endif  // RELEASE
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            else
            {
                _logger.LogError("Failed to upload file to VirusTotal: {response.Content}", response.Content);
            }
            ModelState.AddModelError("ImageFile", "The uploaded file is not a valid image file. Please upload a valid image file.");
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "BrandName", laptop.BrandId);
            return View(laptop);
        }

        // GET: Admin/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var laptop = await _context.Laptops
                .Include(l => l.Brand)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (laptop == null)
            {
                return NotFound();
            }

            return View(laptop);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var laptop =  await _context.Laptops.Include(l => l.ImageUrl).FirstAsync(m => m.Id == id);

            //string filepath = Path.Combine(_hostEnvironment.WebRootPath, laptop.ImageFileName);
            //try
            //{
            //    if (System.IO.File.Exists(filepath))
            //        System.IO.File.Delete(filepath);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogDebug(ex.Message);
            //}
            string deleteHash = string.Empty;
            if (laptop.ImageUrl != null)
            {
                deleteHash = await _imageUrlRepository.DeleteImageUrlModelAsync(laptop.ImageUrl);
            } 
            else 
            {
                string filepath = Path.Combine(_hostEnvironment.WebRootPath, "img" ,laptop.ImageFileName!);
                if (System.IO.File.Exists(filepath))
                    System.IO.File.Delete(filepath);
                else 
                    await RemoveBlobFileAsync(laptop.ImageFileName!);
                _context.Remove(laptop);
            }

            await _context.SaveChangesAsync();
            await DeleteImageAsync("https://api.imgur.com/3/image", deleteHash);
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var laptop = await _context.Laptops
                .Include(l => l.Brand)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (laptop == null)
            {
                return NotFound();
            }

            return View(laptop);
        }

        // GET: Admin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var laptop = await _context.Laptops.FindAsync(id);
            if (laptop == null)
            {
                return NotFound();
            }
            ViewData["Brand"] = new SelectList(_context.Brands, "Id", "BrandName", laptop.BrandId);
            return View(laptop);
        }

        // POST: Admin/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ModelName,Processor,ImageFileName,Price,BrandId")] Laptop laptop)
        {
            if (id != laptop.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(laptop);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LaptopExists(laptop.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "BrandName", laptop.BrandId);
            return View(laptop);
        }

        // GET: Admin
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Laptops.Include(l => l.Brand)
                                                       .Include(l => l.ImageUrl);
                                                       
            return View(await applicationDbContext.ToListAsync());
        }

        [NonAction]
        private async Task<bool> GetReport(string resource, string apikey)
        {
            var client = new RestClient("https://www.virustotal.com/api/v3/analyses/" + resource);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            request.AddHeader("x-apikey", apikey);

            var response = await client.GetAsync(request);
            if (response.IsSuccessful)
            {
                // Parse the response and check the detection results
                dynamic? json = JsonConvert.DeserializeObject(response.Content!);
                int detectedCount = json?.data.attributes.stats.harmless + json?.data.attributes.stats.malicious;
                _logger.LogInformation("Number of engines detected： {detectedCount}", detectedCount);
                if (detectedCount > 0)
                {
                    _logger.LogInformation("Some engines have detected that this file contains malicious code.");
                    return false;
                }
                else
                {
                    _logger.LogInformation("No malicious code was detected in this file.");
                    return true;
                }
            }
            else
            {
                _logger.LogError("Unable to retrieve report: {response.Content}", response.Content);
                return true;
            }
        }

        private bool LaptopExists(int id)
        {
            return _context.Laptops.Any(e => e.Id == id);
        }

        [NonAction]
        private static BlobServiceClient GetBlobServiceClient(string accountName)
        {
            BlobServiceClient client = new(
                new Uri($"https://{accountName}.blob.core.windows.net"),
                new DefaultAzureCredential());

            return client;
        }


        [NonAction]
        private async Task<RestResponse> UploadImageAsync(string endpoint, string fileName, byte[] formFileContent)
        {
            var options = new RestClientOptions(endpoint);
            var client = new RestClient(options);
            var request = new RestRequest("")
            {
                AlwaysMultipartFormData = true
            };
            var clientId = _configuration.GetValue<string>("ClientID");
            request.AddHeader("Authorization", $"Client-ID {clientId}");
            request.AddHeader("Content-Type", "multipart/form-data");
            //request.AddHeader("x-apikey", _apikey!);
            //request.FormBoundary = "---011000010111000001101001";
            request.AddFile("image", formFileContent, fileName);
            return await client.PostAsync(request);
        }

        [NonAction]
        private async Task DeleteImageAsync(string endpoint, string deleteHash)
        {
            var options = new RestClientOptions(endpoint);
            var client = new RestClient(options);
            var request = new RestRequest(deleteHash, Method.Delete);
            var clientId = _configuration.GetValue<string>("ClientID");
            request.AddHeader("Authorization", $"Client-ID {clientId}");
            //request.AddHeader("Content-Type", "application/json");
            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Image is deleted and The content is {response.Content}", response.Content);
            }
            else
            {
                _logger.LogError("Status code is {response.StatusCode} and the error is {response.ErrorMessage}", response.StatusCode, response.ErrorMessage);
            }
            return;
        }

        [NonAction]
        private static string GenerateRandomFileName(string fileName)
        {
            var tmp = Path.GetRandomFileName();
            return tmp.Split(".")[0] + Path.GetExtension(fileName);
        }

        [NonAction]
        private async Task<RestResponse> ScanVirus(string endpoint, string fileName ,byte[] content)
        {
            var options = new RestClientOptions(endpoint);
            var client = new RestClient(options);
            var request = new RestRequest("")
            {
                AlwaysMultipartFormData = true
            };
            request.AddHeader("accept", "application/json");
            request.AddHeader("x-apikey", _apikey!);
            request.FormBoundary = "---011000010111000001101001";
            request.AddFile("file", content, fileName);
            return await client.PostAsync(request);
        }

        [NonAction]
        private async Task RemoveBlobFileAsync(string imageName)
        {
            var blobServiceClient = GetBlobServiceClient(_configuration.GetValue<string>("accountName")!);
            var containerClient = blobServiceClient.GetBlobContainerClient("laptopimageforcart");
            BlobClient blobClient = containerClient.GetBlobClient(imageName);
            await blobClient.DeleteAsync();
        }

    }
}