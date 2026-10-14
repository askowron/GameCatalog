namespace GameCatalog.App.Services;

/// <summary>Bounded stack of undo actions (newest first).</summary>
public sealed class UndoService
{
    public const int Capacity = 50;
    private readonly LinkedList<Action> _actions = new();

    public int Count => _actions.Count;

    public void Push(Action undo)
    {
        _actions.AddLast(undo);
        if (_actions.Count > Capacity) _actions.RemoveFirst();
    }

    public bool TryUndo()
    {
        if (_actions.Last is not { } last) return false;
        _actions.RemoveLast();
        last.Value();
        return true;
    }

    public void Clear() => _actions.Clear();
}
