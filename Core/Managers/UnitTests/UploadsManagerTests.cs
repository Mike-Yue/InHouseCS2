namespace InHouseCS2.Core.Managers.UnitTests;

using FluentAssertions;

[TestClass]
public sealed class UploadsManagerTests
{
    [TestMethod]
    public void TestMethod1()
    {
        var uploadsManager = new UploadsManager();
        uploadsManager.GetUploadURL("123", "dem").Should().Be("123");
    }
}
