using Mirror;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterConfigManager))]
public class NetworkCharacterConfig : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnConfigChangedHook))]
    public CharacterConfig syncConfig;
    public CharacterConfigManager configManager { get; private set; }
    public UnityEvent<CharacterConfig, CharacterConfig> OnConfigChanged = new();

    [SyncVar(hook = nameof(OnActiveChangedHook))]
    public bool isActive = true;

    private void Awake()
    {
        configManager = GetComponent<CharacterConfigManager>();
    }

    public override void OnStartServer()
    {
        if (!configManager)
            configManager = GetComponent<CharacterConfigManager>();
        syncConfig = configManager.GetConfig();

        configManager.OnConfigChanged.AddListener(RequestConfigChange);
    }

    public void RequestConfigChange(CharacterConfig newConfig)
    {
        if (isServer)
        {
            syncConfig = newConfig;
            configManager.SetConfig(newConfig);
        }
        else
            CmdSetConfig(newConfig);
    }

    [Command(requiresAuthority = false)]
    private void CmdSetConfig(CharacterConfig newConfig)
    {
        syncConfig = newConfig;
    }

    [Command(requiresAuthority = false)]
    public void CmdSetActive(bool active)
    {
        isActive = active;
    }

    private void OnConfigChangedHook(CharacterConfig oldVal, CharacterConfig newVal)
    {
        OnConfigChanged.Invoke(oldVal, newVal);
    }

    private void OnActiveChangedHook(bool oldVal, bool newVal)
    {
        configManager.SetActive(newVal);
    }
}
