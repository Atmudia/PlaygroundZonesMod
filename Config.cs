using SRML.Config.Attributes;

namespace PlaygroundZonesMod
{
    [ConfigFile("config", "SETTINGS")]
    public static class Config
    {
        [ConfigComment("Enable or disable default spawners")]
        public static bool EnableDefaultSpawners = false;
    }
}