using System.Reflection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace StS2Aris.StS2ArisCode.Utils;

public static class VfxCmdCompat
{
    private static readonly MethodInfo? PlayOnCreatureCenterMethod = typeof(VfxCmd)
        .GetMethod(nameof(VfxCmd.PlayOnCreatureCenter), [typeof(Creature), typeof(string)])
        ?? typeof(VfxCmd).GetMethod(nameof(VfxCmd.PlayOnCreatureCenter),
            [typeof(Creature), typeof(string), typeof(bool)]);

    public static void PlayOnCreatureCenter(Creature target, string path)
    {
        if (PlayOnCreatureCenterMethod == null)
        {
            throw new MissingMethodException(typeof(VfxCmd).FullName, nameof(VfxCmd.PlayOnCreatureCenter));
        }

        object?[] arguments = PlayOnCreatureCenterMethod.GetParameters().Length == 2
            ? [target, path]
            : [target, path, false];

        try
        {
            PlayOnCreatureCenterMethod.Invoke(null, arguments);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            throw ex.InnerException;
        }
    }
}
