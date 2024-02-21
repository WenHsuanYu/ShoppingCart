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
using ShoppingCartUI.Utilities;
using RestSharp;
using Newtonsoft.Json;

namespace ShoppingCartUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LaptopController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string? _targetFilePath;
        private readonly string? _apikey;
        private readonly ILogger<LaptopController> _logger;

        public LaptopController(ApplicationDbContext context, IConfiguration config, ILogger<LaptopController> logger)
        {
            _context = context;
            _targetFilePath = config.GetValue<string>("StoredFilesPath");
            _apikey = config.GetValue<string>("VirusTotal");
            _logger = logger;
        }

        // GET: Admin/Create
        public IActionResult Create()
        {
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

            var options = new RestClientOptions("https://www.virustotal.com/api/v3/files");
            var client = new RestClient(options);
            var request = new RestRequest("");

            //string fileBase64 = Convert.ToBase64String(formFileContent);

            request.AlwaysMultipartFormData = true;
            request.AddHeader("accept", "application/json");
            request.AddHeader("x-apikey", _apikey!);
            request.FormBoundary = "---011000010111000001101001";
            request.AddFile("file", formFileContent, laptop.ImageFile!.FileName);
            var response = await client.PostAsync(request);

            if (response.IsSuccessful)
            {
                _logger.LogInformation("File successfully uploaded to VirusTotal, checking report...");

                dynamic? json = JsonConvert.DeserializeObject(response.Content);
                string resource = json is not null ? json.data.id : "";
                if (await GetReport(resource, _apikey!))
                {
                    if (ModelState.IsValid)
                    {
                        // For the file name of uploaded file stored
                        // server side, use Path.GetRandomFileName to generate a safe
                        // random file name.
                        var tmp = Path.GetRandomFileName();
                        var trustedFileNameForFileStorage = tmp.Split(".")[0] + Path.GetExtension(laptop.ImageFile.FileName);
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

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            else
            {
                _logger.LogError("Failed to upload file to VirusTotal:" + response.Content);
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
            var laptop = await _context.Laptops.FindAsync(id);
            if (laptop != null)
            {
                _context.Laptops.Remove(laptop);
            }

            await _context.SaveChangesAsync();
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,ModelName,Processor,Price,BrandId")] Laptop laptop)
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
            var applicationDbContext = _context.Laptops.Include(l => l.Brand);
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
                dynamic? json = JsonConvert.DeserializeObject(response.Content);
                int detectedCount = json?.data.attributes.stats.harmless + json?.data.attributes.stats.malicious;
                _logger.LogInformation("Number of engines detected：" + detectedCount);
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
                _logger.LogError("Unable to retrieve report:" + response.Content);
                return true;
            }
        }

        private bool LaptopExists(int id)
        {
            return _context.Laptops.Any(e => e.Id == id);
        }
    }
}