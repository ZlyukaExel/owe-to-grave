using Mirror;
using UnityEngine;

[RequireComponent(typeof(CharacterConfigManager))]
public class NetworkCharacterConfig : NetworkBehaviour
{
    [HideInInspector]
    [SyncVar(hook = nameof(OnConfigChanged))]
    public CharacterConfig config;
    public CharacterConfigManager configManager { get; private set; }

    public void Start()
    {
        configManager = GetComponent<CharacterConfigManager>();
        base.OnStartServer();
        CmdSetConfig(configManager.GetConfig());
    }

    [Command(requiresAuthority = false)]
    public void CmdSetConfig(CharacterConfig config)
    {
        this.config = config;
    }

    private void OnConfigChanged(CharacterConfig oldVal, CharacterConfig newVal)
    {
        if (!configManager)
            configManager = GetComponent<CharacterConfigManager>();
        configManager.SetConfig(newVal);
    }
}
