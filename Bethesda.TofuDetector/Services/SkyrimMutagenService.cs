using Mutagen.Bethesda;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Skyrim;

namespace Bethesda.TofuDetector.Services;

public class SkyrimMutagenService : ISkyrimMutagenService
{
	private static ISkyrimMutagenService? Instance { get; set; }

	public static ISkyrimMutagenService GetOrCreateInstance(GameRelease gameRelease)
	{
		Instance ??= new SkyrimMutagenService(gameRelease);
		return Instance;
	}

	public ISkyrimMod? PatchMod { get; private set; }
	public IGameEnvironment<ISkyrimMod, ISkyrimModGetter>? SkyrimEnvironment { get; private set; }
	private readonly GameRelease _gameRelease;

	private SkyrimMutagenService(GameRelease gameRelease)
	{
		_gameRelease = gameRelease;
	}

	public SkyrimMutagenService(GameRelease gameRelease, ModKey[] loadOrder)
	{
		_gameRelease = gameRelease;
		Load(loadOrder);
	}

	public void Load(ModKey[] loadOrder)
	{
		SkyrimEnvironment?.Dispose();

		PatchMod = new SkyrimMod(ModKey.FromName(new Guid().ToString(), ModType.Plugin), _gameRelease.ToSkyrimRelease());
		SkyrimEnvironment = GameEnvironment.Typical.Builder<ISkyrimMod, ISkyrimModGetter>(_gameRelease)
			.WithLoadOrder(loadOrder)
			.WithOutputMod(PatchMod)
			.Build();
	}

	public void SavePatch(string path)
	{
		PatchMod?.WriteToBinary(path, new BinaryWriteParameters()
		{
			ModKey = ModKeyOption.NoCheck
		});
	}

	~SkyrimMutagenService()
	{
		SkyrimEnvironment?.Dispose();
	}
}