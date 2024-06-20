using HighlightsVault.Helpers;
using HighlightsVault.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace HighlightsVault.Controllers
{
    public class HighlightsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HighlightsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (TempData.ContainsKey("ErrorMessage"))
            {
                string err = TempData["ErrorMessage"].ToString();
                ModelState.AddModelError(string.Empty, err);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(PasswordViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Password? password = await _context.Passwords
                        .FirstOrDefaultAsync(p => p.PasswordValue == model.InputPassword);

                    if (password != null)
                    {
                        HttpContext.Session.SetString("HasAccess", "true");
                        return RedirectToAction("HighlightsVault");
                    }
                    else
                    {
                        HttpContext.Session.SetString("HasAccess", "false");
                        TempData["ErrorMessage"] = "Invalid Password";
                        string err = TempData["ErrorMessage"].ToString();
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["ErrorMessage"] = $"An error occurred while trying to load the main highlights login page.\n{ex.Message}";
                return View(model);
            }            
        }

        /// <summary>
        /// supports pagination, through JS Scroll events w/ AJAX (you could add HTML/Divs for customization if desired)
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<IActionResult> HighlightsVault(int page = 1, int pageSize = 10) // int page = 1, int pageSize = 10
        {
            if (TempData.ContainsKey("ErrorMessage"))
            {
                string err = TempData["ErrorMessage"].ToString();
                ModelState.AddModelError(string.Empty, err);
            }

            List<Highlight> highlights = new List<Highlight>();
            try
            {
                var totalHighlights = await _context.Highlights.CountAsync();
                    highlights = await _context.Highlights.OrderByDescending(g => g.HighlightDate)
                                                  .Skip((page - 1) * pageSize)
                                                  .Take(pageSize)
                                                  .ToListAsync();

                var viewModel = new PagedHighlightsViewModel
                {
                    Highlights = highlights,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalHighlights / pageSize),
                    TotalHighlights = totalHighlights
                };

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {                
                    return PartialView("_HighlightRows", viewModel.Highlights);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["ErrorMessage"] = "An error occurred while trying to load the highlights.";
                return Index();
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddHighlight(string steamID, string steamIDs,
            string userDescription, string userDescriptionMultiple,
            DateTime highlightDate, DateTime highlightDateMultiple,
            bool createGroup, string groupName)
        {
            // very basic auth
            bool isHasAccess;
            try
            {
                isHasAccess = bool.Parse(HttpContext.Session.GetString("HasAccess"));
            }
            catch (Exception)
            {
                return new NotFoundResult();
            }

            if (!isHasAccess)
            {
                return new NotFoundResult();
            }
            else
            {
                try
                {
                    List<string> steamIDOrTitleList = new List<string>();
                    // Check if the form is submitted with single or multiple steam IDs
                    bool isValidSteamId = false;

                    if (!string.IsNullOrEmpty(steamID))
                    {

                        string newSteamId = ExtractSteamIDFromProfileURL(steamID);
                        isValidSteamId = IsValidSteamID64(newSteamId);

                        steamIDOrTitleList.Add(newSteamId);

                    }
                    else if (!string.IsNullOrEmpty(steamIDs))
                    {
                        steamIDOrTitleList = steamIDs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(id => ExtractSteamIDFromProfileURL(id.Trim())).ToList();
                        if (steamIDOrTitleList.Any(id => !IsValidSteamID64(ExtractSteamIDFromProfileURL(id))))
                        {
                            TempData["ErrorMessage"] = "One or more SteamIDs are invalid or do not exist.";
                            return RedirectToAction("HighlightsVault");
                        }
                    }

                    HighlightsVaultGroup newGroup = null;
                    if (createGroup)
                    {
                        newGroup = new HighlightsVaultGroup { CreatedAt = DateTime.Now };
                        _context.HighlightsVaultGroups.Add(newGroup);
                        await _context.SaveChangesAsync(); // Save group to get its ID
                    }

                    for (int i = 0; i < steamIDOrTitleList.Count; i++)
                    {
                        var newHighlight = new Highlight();
                        if (steamIDOrTitleList.Count == 1) // handle single insert
                        {
                            newHighlight = new Highlight
                            {
                                SteamID = steamIDOrTitleList[i],
                                UserDescription = userDescription,
                                HighlightDate = highlightDate,
                                CreatedAt = DateTime.Now,
                                GroupId = newGroup?.GroupId
                            };
                        }
                        else if (steamIDOrTitleList.Count > 1)
                        {
                            newHighlight = new Highlight
                            {
                                SteamID = steamIDOrTitleList[i],
                                UserDescription = userDescriptionMultiple,
                                HighlightDate = highlightDateMultiple,
                                CreatedAt = DateTime.Now,
                                GroupId = newGroup?.GroupId
                            };
                        }

                        if (!isValidSteamId)
                        {
                            string emptyString = "empty"; // must have some bytes

                            newHighlight.HighlightPersonName = "...";
                            newHighlight.ProfilePictureUrl = "";
                            newHighlight.ProfileUrl = "";
                            newHighlight.ProfilePicture = Encoding.ASCII.GetBytes(emptyString);
                            newHighlight.ProfilePictureUrl = "";
                            newHighlight.ProfileUrl = "";

                        }
                        else
                        {
                            // Valid steam id entered
                            // Fetch user profile data using Steam Web API
                            var steamProfileUrl = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key=1AF2C3C4D50621471ACCAB137D8A161F&steamids={newHighlight.SteamID}";
                            using (var httpClient = new HttpClient())
                            {
                                var response = await httpClient.GetAsync(steamProfileUrl);
                                if (response.IsSuccessStatusCode)
                                {
                                    var json = await response.Content.ReadAsStringAsync();
                                    dynamic data = JsonConvert.DeserializeObject(json);
                                    var profile = data.response.players[0];
                                    newHighlight.ProfilePictureUrl = profile.avatarfull.ToString();
                                    newHighlight.ProfileUrl = profile.profileurl.ToString();
                                    newHighlight.HighlightPersonName = profile.personaname.ToString();

                                    // Download the image and store it in the database
                                    if (!string.IsNullOrEmpty(newHighlight.ProfilePictureUrl))
                                    {
                                        newHighlight.ProfilePicture = await DownloadImageAsync(newHighlight.ProfilePictureUrl);
                                    }
                                }
                                else
                                {
                                    newHighlight.ProfilePictureUrl = "https://avatars.steamstatic.com/5536d0161c0ddd455c94f6f908379dde60125d01_full.jpg";
                                    newHighlight.ProfileUrl = "https://steamcommunity.com/profiles/76561198973143587/";
                                }
                            }

                        }
                        
                        _context.Highlights.Add(newHighlight);
                    }

                    await _context.SaveChangesAsync();

                    return RedirectToAction("HighlightsVault");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    TempData["ErrorMessage"] = "An error occurred while trying to add your highlight.";
                    return RedirectToAction("HighlightsVault");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditHighlight([FromForm] int Id, [FromForm] string SteamName, [FromForm] string UserDescription,
            [FromForm] DateTime highlightDate,
            [FromForm] IFormFile profilePicture,
            [FromForm] IFormFile videoFile)
        {
            // very basic auth
            bool isHasAccess;
            try
            {
                isHasAccess = bool.Parse(HttpContext.Session.GetString("HasAccess"));
            }
            catch (Exception ex)
            {
                return new NotFoundResult();
            }

            if (!isHasAccess)
            {
                return new NotFoundResult();
            }
            else
            {
                Highlight highlight = new Highlight();

                highlight = await _context.Highlights.FindAsync(Id);
                if (highlight == null || (DateTime.UtcNow - highlight.CreatedAt) > TimeSpan.FromHours(24))
                {
                    // Handle error (highlight not found or not editable)
                    return NotFound();
                }

                highlight.HighlightPersonName = SteamName;
                highlight.UserDescription = UserDescription;

                // Get the current date and time
                DateTime currentDateTime = DateTime.Now;
                DateTime highlightDateTime = new DateTime(highlightDate.Year, highlightDate.Month, highlightDate.Day,
                                                               currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second);

                highlight.HighlightDate = highlightDateTime;

                // Handle profile picture upload
                if (profilePicture != null && profilePicture.Length > 0)
                {
                    const long maxFileSizeImage = 10 * 1024 * 1024; // 10 MB in bytes

                    if (profilePicture.Length > maxFileSizeImage)
                    {
                        TempData["ErrorMessage"] = "Image size cannot exceed 10 MB.";
                        return RedirectToAction("HighlightsVault");
                    }
                    else
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await profilePicture.CopyToAsync(memoryStream);
                            highlight.ProfilePicture = memoryStream.ToArray();
                        }
                    }
                }

                // Handle video file upload
                if (videoFile != null && videoFile.Length > 0)
                {
                    const long maxFileSizeVideo = 800 * 1024 * 1024; // 800 MB in bytes
                    if (videoFile.Length > maxFileSizeVideo)
                    {
                        TempData["ErrorMessage"] = "Video size cannot exceed 800 MB.";
                        return RedirectToAction("HighlightsVault");
                    }
                    else
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await videoFile.CopyToAsync(memoryStream);
                            highlight.Clip = memoryStream.ToArray();
                        }
                    }                    
                }

                try
                {
                    _context.Update(highlight);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw;
                }

                return RedirectToAction("HighlightsVault");
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHighlight(int id)
        {
            var highlight = await _context.Highlights.FindAsync(id);
            if (highlight == null)
            {
                return NotFound();
            }

            if (DateTime.UtcNow - highlight.CreatedAt <= TimeSpan.FromHours(24))
            {
                _context.Highlights.Remove(highlight);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(HighlightsVault));
            }
            else
            {
                TempData["ErrorMessage"] = "You can only delete highlight entries within 24 hours of creation.";
                return View("HighlightsVault", await _context.Highlights.ToListAsync());
            }
        }

        [HttpGet]
        public IActionResult GetClip(int id)
        {
            var highlight = _context.Highlights.Find(id);
            if (highlight == null || highlight.Clip == null)
            {
                return NotFound();
            }

            var videoStream = new MemoryStream(highlight.Clip);
            return new FileStreamResult(videoStream, "video/mp4");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public string ExtractSteamIDFromProfileURL(string steamID)
        {
            // Check if the input is in the Steam Community profile URL format
            if (steamID.StartsWith("https://steamcommunity.com/profiles/"))
            {
                // Extract the SteamID from the URL
                var steamIDParts = steamID.Split('/');
                if ((steamIDParts.Length == 6 || steamIDParts.Length == 5) && steamIDParts[4].Length == 17 && long.TryParse(steamIDParts[4], out long id))
                {
                    return id.ToString(); // Return the extracted SteamID
                }
                return null; // Invalid Steam Community profile URL format
            }
            return steamID; // Return the input as it is (assuming it's already a valid SteamID64)
        }

        public bool IsValidSteamID64(string steamID)
        {
            // SteamID64 format: 17-digit number
            if (!Regex.IsMatch(steamID, @"^\d{17}$"))
            {
                return false;
            }

            // Check against the Steam Web API to validate if the SteamID exists
            var steamProfileUrl = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key=1AF2C3C4D50621471ACCAB137D8A161F&steamids={steamID}";
            using (var httpClient = new HttpClient())
            {
                var response = httpClient.GetAsync(steamProfileUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    dynamic data = JsonConvert.DeserializeObject(json);
                    var players = data.response.players;
                    // Check if any player data is returned for the given SteamID
                    if (players != null && players.Count > 0)
                    {
                        return true; // SteamID exists
                    }
                }
            }
            return false; // SteamID does not exist or API request failed
        }

        private async Task<Highlight> CreateHighlightAsync(string steamID, string description, DateTime highlightDate, int? groupId)
        {
            // Get the current date and time
            DateTime currentDateTime = DateTime.Now;

            // Append the current time (hour, minute, second) to the selected highlight date
            DateTime highlightDateTime = new DateTime(highlightDate.Year, highlightDate.Month, highlightDate.Day,
                                                   currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second);

            // Create a new GreatBookHighlights object
            var newHighlight = new Highlight
            {
                SteamID = steamID,
                UserDescription = description,
                HighlightDate = highlightDateTime, // Use the combined date and time
                CreatedAt = DateTime.Now,
                GroupId = groupId
            };

            // Fetch user profile data using Steam Web API
            var steamProfileUrl = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key=1AF2C3C4D50621471ACCAB137D8A161F&steamids={steamID}";
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(steamProfileUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    dynamic data = JsonConvert.DeserializeObject(json);
                    var profile = data.response.players[0];
                    newHighlight.ProfilePictureUrl = profile.avatarfull.ToString();
                    newHighlight.ProfileUrl = profile.profileurl.ToString();
                    newHighlight.HighlightPersonName = profile.personaname.ToString();

                    if (!string.IsNullOrEmpty(newHighlight.ProfilePictureUrl))
                    {
                        byte[] imageBytes = await DownloadImageAsync(newHighlight.ProfilePictureUrl);
                        newHighlight.ProfilePicture = imageBytes;
                    }
                }
                else
                {
                    // Handle error
                    newHighlight.ProfilePictureUrl = "https://avatars.steamstatic.com/9008d99567b5a95b16432e30f2d81067e36f49b2_full.jpg";
                    newHighlight.ProfileUrl = "https://steamcommunity.com/profiles/76561198046513952/";
                }
            }

            return newHighlight;
        }

        private async Task<byte[]> DownloadImageAsync(string imageUrl)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(imageUrl);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
            }
            return null;
        }

        public string GetImageSrc(byte[] imageBytes)
        {
            if (imageBytes != null && imageBytes.Length > 0)
            {
                string base64String = Convert.ToBase64String(imageBytes);
                return $"data:image/jpeg;base64,{base64String}";
            }
            return "/path/to/default/image.jpg"; // Default image if no profile picture is available
        }

    }
}
