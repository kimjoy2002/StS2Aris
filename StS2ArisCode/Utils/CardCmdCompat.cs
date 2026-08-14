using System.Reflection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace StS2Aris.StS2ArisCode.Utils;

public static class CardCmdCompat
{
    private static readonly MethodInfo? ExhaustMethod = typeof(CardCmd)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .FirstOrDefault(method =>
        {
            if (method.Name != nameof(CardCmd.Exhaust))
            {
                return false;
            }

            ParameterInfo[] parameters = method.GetParameters();
            return parameters.Length == 4
                   && parameters[0].ParameterType == typeof(PlayerChoiceContext)
                   && parameters[1].ParameterType == typeof(CardModel)
                   && parameters[2].ParameterType == typeof(bool)
                   && parameters[3].ParameterType == typeof(bool);
        });

    public static async Task Exhaust(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal = false,
        bool skipVisuals = false)
    {
        if (ExhaustMethod == null)
        {
            throw new MissingMethodException(typeof(CardCmd).FullName, nameof(CardCmd.Exhaust));
        }

        try
        {
            var task = (Task?)ExhaustMethod.Invoke(
                null,
                [choiceContext, card, causedByEthereal, skipVisuals]);
            if (task == null)
            {
                throw new InvalidOperationException("CardCmd.Exhaust did not return a Task.");
            }

            await task;
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            throw ex.InnerException;
        }
    }
}
