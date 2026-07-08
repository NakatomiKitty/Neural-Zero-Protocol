using GdUnit4;
using NeuralZeroProtocol.Autoloads;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using static GdUnit4.Assertions;
namespace NeuralZeroProtocol.Tests.Scripts.Resources.CharacterData;

[TestSuite]
[RequireGodotRuntime]
public class TestCharacterStatResource
{
    private CharacterStatResource _characterStatResource;

    [BeforeTest]
    public void Setup()
    {
        _characterStatResource = AutoFree(new CharacterStatResource());
    }

    [TestCase(Rarity.Scrap, 1.0f, TestName = "GetStatMultiplier_ReturnsScrapMultiplier")]
    [TestCase(Rarity.Common, 1.15f, TestName = "GetStatMultiplier_ReturnsCommonMultiplier")]
    [TestCase(Rarity.Uncommon, 1.4375f, TestName = "GetStatMultiplier_ReturnsUncommonMultiplier")] 
    [TestCase(Rarity.Rare, 1.796875f, TestName = "GetStatMultiplier_ReturnsRareMultiplier")] 
    [TestCase(Rarity.SuperRare, 2.6953125f, TestName = "GetStatMultiplier_ReturnsSuperRareMultiplier")] 
    [TestCase(Rarity.UltraRare, 4.04296875f, TestName = "GetStatMultiplier_ReturnsUltraRareMultiplier")] 
    public void GetStatMultiplier_ReturnsCorrectMultiplier(Rarity rarity, float expectedMult)
    {
        float mult = _characterStatResource.GetStatMultiplier(rarity);

        AssertThat(mult).IsEqual(expectedMult);
    }
}