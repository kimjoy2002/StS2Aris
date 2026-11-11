using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using StS2Aris.StS2ArisCode.Extensions;

namespace StS2Aris.StS2ArisCode.Potions;

public abstract class StS2ArisPotion : CustomPotionModel
{
    private string PotionImagePath => $"potions/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".ImagePath();
    private string PotionOutlinePath => $"potions/{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".ImagePath();

    public override string? CustomPackedImagePath =>
        ResourceLoader.Exists(PotionImagePath) ? PotionImagePath : null;

    public override string? CustomPackedOutlinePath =>
        ResourceLoader.Exists(PotionOutlinePath) ? PotionOutlinePath : null;
}
