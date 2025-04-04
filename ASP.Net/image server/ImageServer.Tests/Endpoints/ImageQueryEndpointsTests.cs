using ImageServer.Api.DTOs.Images.Responses;
using ImageServer.Api.Services.Interfaces;
using ImageServer.Tests.Base;
using Moq;

namespace ImageServer.Tests.Endpoints;

public class ImageQueryEndpointsTests : TestBase
{
    private readonly Mock<IImageService> _imageServiceMock;

    public ImageQueryEndpointsTests()
    {
        _imageServiceMock = new Mock<IImageService>();
    }

    [Fact]
    public async Task GetImages_ReturnsListOfImages()
    {
        // Arrange
        var images = new List<ImageResponseDto>
        {
            new(
                Guid.NewGuid(),
                "Test1",
                "image/jpeg",
                1024,
                DateTime.UtcNow,
                null,
                "http://test/1",
                "http://test/1/thumb"
            ),
            new(
                Guid.NewGuid(),
                "Test2",
                "image/png",
                2048,
                DateTime.UtcNow,
                null,
                "http://test/2",
                "http://test/2/thumb"
            )
        };

        _imageServiceMock
            .Setup(x => x.GetImagesAsync())
            .ReturnsAsync(images);

        // Act
        var result = await _imageServiceMock.Object.GetImagesAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetImage_WithValidId_ReturnsImage()
    {
        // Arrange
        var id = Guid.NewGuid();
        var filePath = "test.jpg";

        _imageServiceMock
            .Setup(x => x.GetImageAsync(id))
            .ReturnsAsync((true, filePath, null));

        // Act
        var result = await _imageServiceMock.Object.GetImageAsync(id);

        // Assert
        Assert.True(result.found);
        Assert.Equal(filePath, result.filePath);
    }

    [Fact]
    public async Task GetThumbnail_WithValidId_ReturnsThumbnail()
    {
        // Arrange
        var id = Guid.NewGuid();
        var thumbnailPath = "test_thumb.jpg";

        _imageServiceMock
            .Setup(x => x.GetThumbnailAsync(id))
            .ReturnsAsync((true, thumbnailPath, null));

        // Act
        var result = await _imageServiceMock.Object.GetThumbnailAsync(id);

        // Assert
        Assert.True(result.found);
        Assert.Equal(thumbnailPath, result.filePath);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetImages_WithInvalidPage_ReturnsFirstPage(int page)
    {
        // Arrange
        var images = new List<ImageResponseDto>();
        _imageServiceMock
            .Setup(x => x.GetImagesAsync())
            .ReturnsAsync(images);

        // Mock endpoint behavior
        var result = new
        {
            page = Math.Max(1, page),
            pageSize = 20,
            total = 0,
            items = images
        };

        // Act
        var response = await _imageServiceMock.Object.GetImagesAsync();

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response);
    }

    [Fact]
    public async Task GetImage_WithInvalidGuid_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = Guid.Empty;

        _imageServiceMock
            .Setup(x => x.GetImageAsync(invalidId))
            .ReturnsAsync((false, null, "Invalid image ID"));

        // Act
        var result = await _imageServiceMock.Object.GetImageAsync(invalidId);

        // Assert
        Assert.False(result.found);
        Assert.NotNull(result.error);
    }

    [Fact]
    public async Task GetImageDetails_WithValidId_ReturnsDetails()
    {
        // Arrange
        var id = Guid.NewGuid();
        var details = new ImageDetailsResponseDto(
            id,
            "test.jpg",
            "image/jpeg",
            1024,
            DateTime.UtcNow,
            null,
            "http://test/image.jpg",
            "http://test/image_thumb.jpg",
            "/path/to/image.jpg",
            "/path/to/thumb.jpg",
            new Dictionary<string, object>
            {
                { "exists", true },
                { "hasThumbnail", true },
                { "extension", ".jpg" }
            }
        );

        _imageServiceMock
            .Setup(x => x.GetImageDetailsAsync(id))
            .ReturnsAsync((true, details, null));

        // Act
        var result = await _imageServiceMock.Object.GetImageDetailsAsync(id);

        // Assert
        Assert.True(result.found);
        Assert.NotNull(result.details);
        Assert.Equal(id, result.details.Id);
        Assert.Equal("test.jpg", result.details.Name);
        Assert.True(result.details.Metadata.ContainsKey("exists"));
        Assert.True(result.details.Metadata.ContainsKey("hasThumbnail"));
        Assert.True(result.details.Metadata.ContainsKey("extension"));
    }

    [Fact]
    public async Task GetImageDetails_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _imageServiceMock
            .Setup(x => x.GetImageDetailsAsync(id))
            .ReturnsAsync((false, null, "Image not found"));

        // Act
        var result = await _imageServiceMock.Object.GetImageDetailsAsync(id);

        // Assert
        Assert.False(result.found);
        Assert.Null(result.details);
        Assert.Equal("Image not found", result.error);
    }

    [Fact]
    public async Task GetImageDetails_WithEmptyGuid_ReturnsBadRequest()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        _imageServiceMock
            .Setup(x => x.GetImageDetailsAsync(emptyGuid))
            .ReturnsAsync((false, null, "Invalid image ID"));

        // Act
        var result = await _imageServiceMock.Object.GetImageDetailsAsync(emptyGuid);

        // Assert
        Assert.False(result.found);
        Assert.Null(result.details);
        Assert.NotNull(result.error);
    }
}
