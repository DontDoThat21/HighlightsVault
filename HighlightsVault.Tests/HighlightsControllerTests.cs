using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using HighlightsVault;
using HighlightsVault.Controllers;
using HighlightsVault.Models;
using Moq;
using EntityFrameworkCore.Testing.Moq;


public class HighlightsControllerTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly ApplicationDbContext _context;
    private readonly HighlightsController _controller;

    public HighlightsControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new ApplicationDbContext(options);

        // Seed the database with test data
        _context.Highlights.AddRange(new List<Highlight>
        {
            new Highlight
            {
                HighlightDate = DateTime.Now,
                HighlightPersonName = "Person1",
                ProfilePicture = new byte[0],
                ProfilePictureUrl = "http://example.com/pic1.jpg",
                ProfileUrl = "http://example.com/profile1",
                SteamID = "12345678901234567",
                UserDescription = "Description1",
                CreatedAt = DateTime.Now
            },
            new Highlight
            {
                HighlightDate = DateTime.Now.AddDays(-1),
                HighlightPersonName = "Person2",
                ProfilePicture = new byte[0],
                ProfilePictureUrl = "http://example.com/pic2.jpg",
                ProfileUrl = "http://example.com/profile2",
                SteamID = "12345678901234568",
                UserDescription = "Description2",
                CreatedAt = DateTime.Now.AddDays(-1)
            }
        });
        _context.SaveChanges();

        _controller = new HighlightsController(_context);
    }

    [Fact]
    public async Task Index_Post_ValidPassword_ShouldRedirectToHighlightsVault()
    {
        // Arrange
        var passwordViewModel = new PasswordViewModel { InputPassword = "test" };
        _context.Passwords.Add(new Password { PasswordValue = "test" });
        _context.SaveChanges();

        // Act
        var result = await _controller.Index(passwordViewModel);

        // Assert
        var redirectToActionResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("HighlightsVault.Models.PasswordViewModel",
            redirectToActionResult.Model.ToString());
        //Assert.Equal("true", _controller.HttpContext.Session.GetString("HasAccess"));
    }

    [Fact]
    public async Task HighlightsVault_ReturnsViewWithPagedHighlights()
    {
        // Mock HttpContext and Request
        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new HeaderDictionary { { "X-Requested-With", "XMLHttpRequest" } };

        mockRequest.Setup(r => r.Headers).Returns(mockHeaders);
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = mockHttpContext.Object
        };

        // Act
        var result = await _controller.HighlightsVault();

        // Assert
        var viewResult = Assert.IsType<PartialViewResult>(result);
        var model = Assert.IsType<List<Highlight>>(viewResult.Model);
        Assert.True(model.Count >= 1);
    }

}
