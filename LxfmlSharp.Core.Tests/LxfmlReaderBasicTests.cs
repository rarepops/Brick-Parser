using LxfmlSharp.Core.Tests.Helpers;
using Assert = TUnit.Assertions.Assert;

namespace LxfmlSharp.Core.Tests;

[ParallelGroup("BasicTests")]
public class LxfmlReaderBasicTests
{
    [Test]
    public Task Parses_Minimal_Model_With_Versions_And_Bricks()
    {
        // Arrange
        var xml = TestFiles.ReadSample("minimal.lxfml");

        // Act
        var model = LxfmlReader.LoadFromString(xml);

        // Assert
        return Verify(model);
    }

    [Test]
    public Task Materials_Parses_Comma_Delimited_Color_And_Variant()
    {
        // Arrange
        var xml = TestFiles.ReadSample("materials.lxfml");

        // Act
        var m = LxfmlReader.LoadFromString(xml);
        var mats = m.Bricks[0].Parts[0].Materials;

        // Assert
        return Verify(mats);
    }

    [Test]
    public async Task Throws_On_Missing_LXFML_Root()
    {
        // Arrange
        var xml = TestFiles.ReadSample("missing-root.lxfml");

        // Act
        var exception = Assert.Throws<InvalidDataException>(() => LxfmlReader.LoadFromString(xml));

        // Assert
        await Verify(exception!.Message);
    }

    [Test]
    public async Task Throws_On_Missing_Bricks_Container()
    {
        // Arrange
        var xml = TestFiles.ReadSample("missing-bricks.lxfml");

        // Act
        var exception = Assert.Throws<InvalidDataException>(() => LxfmlReader.LoadFromString(xml));

        // Assert
        await Verify(exception!.Message);
    }

    [Test]
    public async Task Throws_On_NonNumeric_DesignId()
    {
        // Arrange
        var xml = TestFiles.ReadSample("nonnumeric-designid.lxfml");

        // Act
        var exception = Assert.Throws<InvalidDataException>(() => LxfmlReader.LoadFromString(xml));

        // Assert
        await Verify(exception!.Message);
    }

    [Test]
    public async Task Throws_On_Bone_With_Wrong_Transform_Length()
    {
        // Arrange
        var xml = TestFiles.ReadSample("wrong-transform.lxfml");

        // Act
        var exception = Assert.Throws<InvalidDataException>(() => LxfmlReader.LoadFromString(xml));

        // Assert
        await Verify(exception!.Message);
    }

    [Test]
    public Task Skips_Unknown_Elements_Without_Throwing()
    {
        // Arrange
        var xml = TestFiles.ReadSample("unknown-elements.lxfml");

        // Act
        var model = LxfmlReader.LoadFromString(xml);

        // Assert
        return Verify(model);
    }

    [Test]
    public async Task Throws_On_Bone_With_NaN_In_Transform()
    {
        // Arrange
        var xml = TestFiles.ReadSample("nan-transform.lxfml");

        // Act
        var exception = Assert.Throws<InvalidDataException>(() => LxfmlReader.LoadFromString(xml));

        // Assert
        await Verify(exception!.Message);
    }
}
