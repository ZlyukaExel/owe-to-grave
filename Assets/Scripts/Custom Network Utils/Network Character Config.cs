using Mirror;
using UnityEngine;

[RequireComponent(typeof(CharacterConfigManager))]
public class NetworkCharacterConfig : NetworkBehaviour
{
    [HideInInspector]
    [SyncVar(hook = nameof(OnConfigChanged))]
    public CharacterConfig config;
    private CharacterConfigManager configManager;

    public override void OnStartServer()
    {
        base.OnStartServer();
        configManager = GetComponent<CharacterConfigManager>();
        SetConfig(configManager.GetConfig());
    }

    private void SetConfig(CharacterConfig config)
    {
        if (!isOwned)
            return;

        CmdSetConfig(config);
    }

    [Command(requiresAuthority = false)]
    public void CmdSetConfig(CharacterConfig config)
    {
        this.config = config;
    }

    private void OnConfigChanged(CharacterConfig oldVal, CharacterConfig newVal)
    {
        configManager.SetConfig(newVal);
    }
}
