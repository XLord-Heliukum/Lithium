using LithiumKit.Models;

namespace LithiumKit.Services;

public interface IEngineManager
{
    IReadOnlyList<WineEngine> Engines { get; }
    void Refresh();
    WineEngine? GetById(string id);
}

/// <summary>Aggregates engines discovered by the active platform backend.</summary>
public sealed class EngineManager : IEngineManager
{
    private readonly BackendRegistry _backends;
    private readonly List<WineEngine> _engines = new();

    public EngineManager(BackendRegistry backends)
    {
        _backends = backends;
        Refresh();
    }

    public IReadOnlyList<WineEngine> Engines => _engines;

    public void Refresh()
    {
        _engines.Clear();
        foreach (var engine in _backends.Current.DiscoverEngines())
            _engines.Add(engine);

        if (_engines.Count == 0)
            _engines.Add(new WineEngine("wine", "Wine (auto)", null, "wine"));
    }

    public WineEngine? GetById(string id) => _engines.FirstOrDefault(e => e.Id == id);
}
