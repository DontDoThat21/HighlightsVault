using HighlightsVault.Helpers;
using HighlightsVault.Models;
using System;
using Xunit;

public class HighlightsHelperTests
{
    [Fact]
    public void GetImageSrc_ValidImageBytes_ReturnsBase64String()
    {
        // Arrange
        byte[] imageBytes = new byte[] { 1, 2, 3, 4, 5 };
        string expectedBase64String = "data:image/jpeg;base64,AQIDBAU=";

        // Act
        string result = HighlightsHelper.GetImageSrc(imageBytes);

        // Assert
        Assert.Equal(expectedBase64String, result);
    }

    [Fact]
    public void GetImageSrc_NullImageBytes_ReturnsDefaultImagePath()
    {
        // Arrange
        byte[] imageBytes = null;
        string expectedDefaultImagePath = "/path/to/default/image.jpg";

        // Act
        string result = HighlightsHelper.GetImageSrc(imageBytes);

        // Assert
        Assert.Equal(expectedDefaultImagePath, result);
    }

    [Fact]
    public void GetImageSrc_EmptyImageBytes_ReturnsDefaultImagePath()
    {
        // Arrange
        byte[] imageBytes = new byte[] { };
        string expectedDefaultImagePath = "/path/to/default/image.jpg";

        // Act
        string result = HighlightsHelper.GetImageSrc(imageBytes);

        // Assert
        Assert.Equal(expectedDefaultImagePath, result);
    }

    [Fact]
    public void GetRowClass_GroupHighlightWithSameGroupId_ReturnsSameGroupColor()
    {
        // Arrange
        var highlight1 = new Highlight { GroupId = 0 };
        var highlight2 = new Highlight { GroupId = 0 };
        int previousGroupColorId = 0;
        string previousGroupId = null;

        // Act
        string result1 = HighlightsHelper.GetRowClass(highlight1, ref previousGroupColorId, ref previousGroupId);
        string result2 = HighlightsHelper.GetRowClass(highlight2, ref previousGroupColorId, ref previousGroupId);

        // Assert
        Assert.Equal(result1, result2);
    }


    [Fact]
    public void GetRowClass_NormalHighlight_ReturnsNormalHighlightClass()
    {
        // Arrange
        var highlight = new Highlight { GroupId = null };
        int previousGroupColorId = 0;
        string previousGroupId = null;

        // Act
        string result = HighlightsHelper.GetRowClass(highlight, ref previousGroupColorId, ref previousGroupId);

        // Assert
        Assert.Equal("normal-highlight", result);
    }
}
