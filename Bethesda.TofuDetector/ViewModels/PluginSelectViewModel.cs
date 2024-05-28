using DynamicData;
using DynamicData.Binding;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Skyrim;

namespace Bethesda.TofuDetector.ViewModels;

public class PluginSelectViewModel
{
	public IObservableCollection<Models.PluginLoad> PluginLoads { get; }
	public ILoadOrder<IModListing<ISkyrimModGetter>> FinalLoadOrder { get; }

	public PluginSelectViewModel()
	{
		FinalLoadOrder = LoadOrder.Import<ISkyrimModGetter>(GameRelease.SkyrimSE);

		var pluginsToLoad = FinalLoadOrder
			.Select(loadOrderItem => new Models.PluginLoad(loadOrderItem.Key.FileName, FinalLoadOrder.IndexOf(loadOrderItem), loadOrderItem.Key) { WillLoad = loadOrderItem.Value.Enabled })
			.ToList();

		PluginLoads = new ObservableCollectionExtended<Models.PluginLoad>(pluginsToLoad);
	}

	public void Commit()
	{
		foreach (var plugin in PluginLoads)
		{
			if (plugin.WillLoad)
			{
				continue;
			}

			FinalLoadOrder.RemoveKey(plugin.ModKey);
		}
	}

	public void EnableAll()
	{
		foreach (var plugins in PluginLoads)
		{
			plugins.WillLoad = true;
		}
	}

	public void DisableAll()
	{
		foreach (var plugins in PluginLoads)
		{
			plugins.WillLoad = false;
		}
	}

	public void InvertSelection()
	{
		foreach (var plugins in PluginLoads)
		{
			plugins.WillLoad = !plugins.WillLoad;
		}
	}
}