using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Bethesda.TofuDetector.Services;

public interface ISkyrimMutagenService
{
	public ISkyrimMod? PatchMod { get; }
	public IGameEnvironment<ISkyrimMod, ISkyrimModGetter>? SkyrimEnvironment { get; }

	public void Load(ModKey[] loadOrder);

	public void SavePatch(string path);
}