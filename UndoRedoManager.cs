using System.Collections.Generic;
using UnityEngine;

public class UndoRedoManager : MonoBehaviour
{
    public static UndoRedoManager Instance { get; private set; }
    private readonly int maxHistory = 50;
    private Stack<Texture2D[]> _undoStack = new();
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
        if (_undoStack.Count >= maxHistory)
        {
            var list = new List<Texture2D[]>(_undoStack);
            list.RemoveAt(list.Count - 1);
            _undoStack = new Stack<Texture2D[]>(list);
        }

        _undoStack.Push(layerSnapshots);
        _redoStack.Clear();
    }

    public Texture2D[] Undo()
    {
        if (_undoStack.Count <= 1)
            return null;

        var current = _undoStack.Pop();
        _redoStack.Push(current);
        return _undoStack.Peek();
    }

    public Texture2D[] Redo()
    {
        if (_redoStack.Count == 0)
            return null;

        var state = _redoStack.Pop();
        _undoStack.Push(state);
        return state;
    }

    public bool CanUndo => _undoStack.Count > 1;

    public bool CanRedo => _redoStack.Count > 0;

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
