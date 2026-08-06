using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Runs;
using StS2Aris.StS2ArisCode.Config;

namespace StS2Aris.StS2ArisCode.Mechanics;

public static class ClassAltarHostSettingSync
{
    public static bool IsEnabled { get; private set; }

    public static void InitializeForRun(INetGameService netService)
    {
        switch (netService.Type)
        {
            case NetGameType.Singleplayer:
                IsEnabled = ArisModConfig.ForceClassAltarFirstEvent;
                break;
            case NetGameType.Host:
                IsEnabled = ArisModConfig.ForceClassAltarFirstEvent;
                if (netService.IsConnected)
                {
                    
                    StS2ArisMain.Logger.Info($"[ClassAltarHostSettingSync] SendFromHost IsEnabled={IsEnabled}");

                    CustomMessageWrapper.Send(new ClassAltarHostSettingMessage
                    {
                        Enabled = IsEnabled
                    }, netService);
                }
                break;
            case NetGameType.Client:
                // The host's buffered message replaces this before event selection begins.
                IsEnabled = false;
                break;
            default:
                IsEnabled = false;
                break;
        }
    }

    internal static void ReceiveFromHost(bool enabled)
    {
        if (RunManager.Instance.NetService.Type == NetGameType.Client)
        {
            StS2ArisMain.Logger.Info($"[ClassAltarHostSettingSync] ReceiveFromHost IsEnabled={enabled}");
            IsEnabled = enabled;
        }
    }
}

public sealed class ClassAltarHostSettingMessage : ICustomMessage
{
    public bool Enabled { get; set; }

    public bool ShouldBroadcast => false;

    public void HandleMessage(ulong senderId)
    {
        ClassAltarHostSettingSync.ReceiveFromHost(Enabled);
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteBool(Enabled);
    }

    public void Deserialize(PacketReader reader)
    {
        Enabled = reader.ReadBool();
    }
}
