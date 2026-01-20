using System.Reflection;
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace NoTallMounts
{
    public record ModMetaData : AbstractModMetadata
    {
        public override string ModGuid { get; init; } = "com.ajdj100.NoTallMounts";
        public override string Name { get; init; } = "NoTallMounts";
        public override string Author { get; init; } = "ajdj100";
        public override List<string>? Contributors { get; init; }
        public override SemanticVersioning.Version Version { get; init; } = new("1.0.1");
        public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
        public override List<string>? Incompatibilities { get; init; }
        public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
        public override string? Url { get; init; }
        public override bool? IsBundleMod { get; init; }
        public override string License { get; init; } = "MIT";
    }

    [Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 3)]
    public class NoTallMounts(ISptLogger<NoTallMounts> logger, ModHelper modHelper, DatabaseService db) : IOnLoad
    {
        public Task OnLoad()
        {
            //load config
            var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

            var config = modHelper.GetJsonDataFromFile<ModConfig>(modPath, "config.json");

            if (config.Scopes.Length < 1)
            {
                logger.Error("[NoTallMounts] Config is empty, skipping");
                return Task.CompletedTask;
            }

            //load db
            var items = db.GetItems();
            var shrunkCount = 0;

            foreach (var scopeID in config.Scopes)
            {
                items.TryGetValue(scopeID, out var item);

                //guard
                if (item?.Properties == null)
                {
                    logger.Warning($"[NoTallMounts] Could not find item {scopeID}");
                    continue;
                }

                //patch item size
                item.Properties.ExtraSizeUp = 0;
                shrunkCount++;
            }

            logger.Success($"[NoTallMounts] Shrunk {shrunkCount} mounts!");

            return Task.CompletedTask;
        }
    }

    public record ModConfig
    {
        public string[] Scopes { get; init; } = [];
    }



}
