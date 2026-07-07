using asp_net_web_app.Data;
using Microsoft.EntityFrameworkCore;

public class SiteLogic
{
    private readonly DatabaseWrapper _db;

    public SiteLogic(DatabaseWrapper db)
    {
        _db = db;
    }

    public async Task<List<Site>> GetAllSitesAsync()
    {
        return await _db.Sites.ToListAsync();
    }

    // Placeholder create — Site has no fields yet besides Id (auto-generated).
    // Once Site.cs has real attributes, add them as parameters here.
    public string AddSite()
    {
        _db.Sites.Add(new Site());
        _db.SaveChanges();
        return "success";
    }

    // Placeholder edit — nothing to edit yet since Site has no attributes.
    // TODO: once Site.cs has fields (e.g. siteNumber, status), add them as
    // parameters here and set them on the found site below.
    public string EditSite(int id)
    {
        var site = _db.Sites.Find(id);
        if (site == null) return "site not found";

        // TODO: site.someField = someValue;
        _db.SaveChanges();
        return "success";
    }

    public string DeleteSite(int id)
    {
        var site = _db.Sites.Find(id);
        if (site == null) return "site not found";

        _db.Sites.Remove(site);
        _db.SaveChanges();
        return "success";
    }
}
