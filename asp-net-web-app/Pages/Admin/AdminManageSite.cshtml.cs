using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using asp_net_web_app.Data;

namespace asp_net_web_app.Pages
{
    public class AdminManageSiteModel : PageModel
    {
        private readonly DatabaseWrapper _context;

        public AdminManageSiteModel(DatabaseWrapper context)
        {
            _context = context;
        }

        public List<DbSite> AllSites { get; set; } = new List<DbSite>();
        public List<DbSitePhoto> SelectedPhotos { get; set; } = new List<DbSitePhoto>();
        public List<DbSitePrice> SelectedPrices { get; set; } = new List<DbSitePrice>();
        public bool ActiveSiteStatus { get; set; } = true;

        public string CurrentAction { get; set; } = "List";
        public int ActiveSiteId { get; set; }
        public string ActiveSiteNumber { get; set; } = string.Empty;

        public void OnGet()
        {
            CurrentAction = "List";
            AllSites = _context.Sites.ToList();
        }

        public void OnGetCreateSite(int? id)
        {
            if (id.HasValue)
            {
                CurrentAction = "Edit";
                var site = _context.Sites.Find(id.Value);
                if (site != null)
                {
                    ActiveSiteId = site.Id;
                    ActiveSiteNumber = site.SiteNumber;
                    ActiveSiteStatus = site.IsAvailable;
                }
            }
            else
            {
                CurrentAction = "Create";
                ActiveSiteId = 0;
                ActiveSiteStatus = true;
            }
        }

        public void OnGetManagePhotos(int id)
        {
            CurrentAction = "Photos";
            ActiveSiteId = id;
            var site = _context.Sites.Find(id);
            if (site != null) { ActiveSiteNumber = site.SiteNumber; }
            SelectedPhotos = _context.SitePhotos.Where(p => p.DbSiteId == id).ToList();
        }

        public void OnGetManagePrices(int id)
        {
            CurrentAction = "Prices";
            ActiveSiteId = id;
            var site = _context.Sites.Find(id);
            if (site != null) { ActiveSiteNumber = site.SiteNumber; }
            SelectedPrices = _context.SitePrices.Where(p => p.DbSiteId == id).ToList();
        }

        public IActionResult OnPostCreateSite(string siteNum, string category)
        {
            DbSite newSite = new DbSite { SiteNumber = siteNum, Category = category, IsAvailable = true };
            _context.Sites.Add(newSite);
            _context.SaveChanges();
            return RedirectToPage();
        }

        public IActionResult OnPostUpdateSite(int siteId, string siteNum, string category, string isAvailable)
        {
            var siteToChange = _context.Sites.Find(siteId);

            if (siteToChange != null)
            {
                siteToChange.SiteNumber = siteNum;
                siteToChange.Category = category;

                if (isAvailable == "true")
                {
                    siteToChange.IsAvailable = true;
                }
                else
                {
                    siteToChange.IsAvailable = false;
                }

                _context.SaveChanges();
            }

            return RedirectToPage();
        }

        [BindProperty]
        public IFormFile PhotoFile { get; set; } = null!;

        public IActionResult OnPostAddPhoto(int siteId)
        {
            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(PhotoFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    PhotoFile.CopyTo(fileStream);
                }

                string relativeWebPath = "/uploads/" + uniqueFileName;

                DbSitePhoto newPhoto = new DbSitePhoto { DbSiteId = siteId, PhotoUrl = relativeWebPath };
                _context.SitePhotos.Add(newPhoto);
                _context.SaveChanges();
            }

            return RedirectToPage(new { handler = "ManagePhotos", id = siteId });
        }


        public IActionResult OnPostDeletePhoto(int photoId, int siteId)
        {
            var photo = _context.SitePhotos.Find(photoId);
    
            if (photo != null)
            {
                string relativePath = photo.PhotoUrl.TrimStart('/');
                string physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

                try
                {
                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                }
                catch (Exception ex)
                {
                    
                }
                _context.SitePhotos.Remove(photo);
                _context.SaveChanges();
            }   

            return RedirectToPage(new { handler = "ManagePhotos", id = siteId });
        }


        public IActionResult OnPostAddPrice(int siteId, decimal cost, DateTime start, DateTime end)
        {
            DbSitePrice newPrice = new DbSitePrice { DbSiteId = siteId, Cost = cost, Start = start, End = end };
            _context.SitePrices.Add(newPrice);
            _context.SaveChanges();
            return RedirectToPage(new { handler = "ManagePrices", id = siteId });
        }

        public IActionResult OnPostDeletePrice(int priceId, int siteId)
        {
            var rule = _context.SitePrices.Find(priceId);
            if (rule != null) { _context.SitePrices.Remove(rule); _context.SaveChanges(); }
            return RedirectToPage(new { handler = "ManagePrices", id = siteId });
        }

        public IActionResult OnPostDeleteSite(int siteId)
        {
            var site = _context.Sites.Find(siteId);
            if (site != null) { _context.Sites.Remove(site); _context.SaveChanges(); }
            return RedirectToPage();
        }
    }
}