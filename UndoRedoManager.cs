using System.Collections.Generic;
using UnityEngine;

public class UndoRedoManager : MonoBehaviour
{
    public static UndoRedoManager Instance { get; private set; }
    private readonly int maxHistory = 50;
    private readonly LinkedList<Texture2D[]> _undoHistory = new();
    private readonly Stack<Texture2D[]> _redoStack = new();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Push(Texture2D[] layerSnapshots)
    {
        if (_undoHistory.Count >= maxHistory)
        {
            DestroySnapshot(_undoHistory.Last.Value);
            _undoHistory.RemoveLast();
        }

        _undoHistory.AddFirst(layerSnapshots);
        ClearRedoStack();
    }

    public Texture2D[] Undo()
    {
        if (_undoHistory.Count <= 1)
            return null;

        var current = _undoHistory.First.Value;
        _undoHistory.RemoveFirst();
        _redoStack.Push(current);
        return _undoHistory.First.Value;
    }

    public Texture2D[] Redo()
    {
        if (_redoStack.Count == 0)
            return null;

        var state = _redoStack.Pop();
        _undoHistory.AddFirst(state);
        return state;
    }

    public bool CanUndo => _undoHistory.Count > 1;
    public bool CanRedo => _redoStack.Count > 0;

    void ClearRedoStack()
    {
        while (_redoStack.Count > 0)
            DestroySnapshot(_redoStack.Pop());
    }

    void DestroySnapshot(Texture2D[] snapshot)
    {
        if (snapshot == null)
            return;

        foreach (var tex in snapshot)
            if (tex != null)
                Destroy(tex);
    }

    void OnDestroy()
    {
        foreach (var snap in _undoHistory)
            DestroySnapshot(snap);

        _undoHistory.Clear();
        ClearRedoStack();
    }

    public static Texture2D[] TakeSnapshot(List<LayerManager.Layer> layers)
    {
        var snaps = new Texture2D[layers.Count];

        for (int i = 0; i < layers.Count; i++)
        {
            var src = layers[i].texture;
            var snap = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
            snap.SetPixels32(src.GetPixels32());
            snap.Apply();
            snaps[i] = snap;
        }

        return snaps;
    }
}
