[System.Serializable]
public class CharacterConfig
{
    public int weaponId = 0;
    public int pantsId = 0;
    public int topId = 0;
    public int shoesId = 0;
    public int glovesId = 0;
    public int hatId = 0;
    public int hairId = 0;
    public int maskId = 0;
    public bool inCombat = false;

    public CharacterConfig() { }

    public CharacterConfig(CharacterConfig other)
    {
        weaponId = other.weaponId;
        pantsId = other.pantsId;
        topId = other.topId;
        shoesId = other.shoesId;
        glovesId = other.glovesId;
        hatId = other.hatId;
        hairId = other.hairId;
        maskId = other.maskId;
        inCombat = other.inCombat;
    }
}
