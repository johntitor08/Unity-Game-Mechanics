using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}

public static class PuzzleEvents
{
    public static Action<Sprite> OnImageChanged;
    public static Action OnPuzzleRestarted;
    public static Action OnMoveMade;
    public static Action OnPuzzleCompleted;
    public static Action<Difficulty> OnDifficultyChanged;
    public static Action OnPuzzleStarted;
}

public class PuzzleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject piecePrefab;
    [SerializeField] private RectTransform puzzleContainer;
    [SerializeField] private GameUI gameUI;

    [Header("Grid Settings")]
    [SerializeField] private int gridX = 5;
    [SerializeField] private int gridY = 5;

    [Tooltip("Puzzle Size")]
    [SerializeField] private Vector2 puzzleSize = new(1200, 900);

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip snapClip;

    private Sprite currentImage;
    private Vector2 pieceSize;
    private readonly List<PuzzlePiece> pieces = new();

    void OnEnable()
    {
        PuzzleEvents.OnImageChanged += SetPuzzleImage;
        PuzzleEvents.OnDifficultyChanged += SetDifficulty;
        PuzzleEvents.OnPuzzleRestarted += RestartPuzzle;
    }

    void OnDisable()
    {
        PuzzleEvents.OnImageChanged -= SetPuzzleImage;
        PuzzleEvents.OnDifficultyChanged -= SetDifficulty;
        PuzzleEvents.OnPuzzleRestarted -= RestartPuzzle;
    }

    void SetPuzzleImage(Sprite image)
    {
        currentImage = image;
        RestartPuzzle();
    }

    void SetDifficulty(Difficulty diff)
    {
        switch (diff)
        {
            case Difficulty.Easy:
                gridX = gridY = 4;
                break;
            case Difficulty.Medium:
                gridX = gridY = 5;
                break;
            case Difficulty.Hard:
                gridX = gridY = 6;
                break;
        }

        RestartPuzzle();
    }

    void RestartPuzzle()
    {
        ClearPuzzle();

        if (gameUI != null)
            gameUI.ResetUI();

        if (!currentImage) return;
        CreatePuzzle();
    }

    void CreatePuzzle()
    {
        puzzleContainer.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal, puzzleSize.x);
        puzzleContainer.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical, puzzleSize.y);

        pieceSize = new Vector2(
            puzzleSize.x / gridX,
            puzzleSize.y / gridY
        );

        Texture2D tex = currentImage.texture;
        int pixelW = tex.width / gridX;
        int pixelH = tex.height / gridY;

        for (int y = 0; y < gridY; y++)
        {
            for (int x = 0; x < gridX; x++)
            {
                GameObject obj = Instantiate(piecePrefab, puzzleContainer);
                RectTransform rt = obj.GetComponent<RectTransform>();
                rt.sizeDelta = pieceSize;
                rt.localScale = Vector3.one;

                Sprite pieceSprite = Sprite.Create(
                    tex,
                    new Rect(
                        x * pixelW,
                        (gridY - 1 - y) * pixelH,
                        pixelW,
                        pixelH
                    ),
                    new Vector2(0.5f, 0.5f),
                    100f
                );

                obj.GetComponent<Image>().sprite = pieceSprite;
                PuzzlePiece piece = obj.GetComponent<PuzzlePiece>();
                piece.Initialize(new Vector2Int(x, y), this);
                pieces.Add(piece);
            }
        }

        ShufflePieces();
        PuzzleEvents.OnPuzzleStarted?.Invoke();
    }

    void ShufflePieces()
    {
        if (pieces.Count == 0) return;
        List<Vector2> positions = new List<Vector2>();
        _ = puzzleSize.x / 2f - pieceSize.x / 2f;
        _ = puzzleSize.y / 2f - pieceSize.y / 2f;
        int gridCols = Mathf.CeilToInt(puzzleSize.x / pieceSize.x);
        int gridRows = Mathf.CeilToInt(puzzleSize.y / pieceSize.y);
        float cellWidth = puzzleSize.x / gridCols;
        float cellHeight = puzzleSize.y / gridRows;

        for (int y = 0; y < gridRows; y++)
        {
            for (int x = 0; x < gridCols; x++)
            {
                float posX = -puzzleSize.x / 2f + cellWidth / 2f + x * cellWidth;
                float posY = puzzleSize.y / 2f - cellHeight / 2f - y * cellHeight;
                positions.Add(new Vector2(posX, posY));
            }
        }

        for (int i = 0; i < positions.Count; i++)
        {
            int swapIndex = UnityEngine.Random.Range(i, positions.Count);
            (positions[swapIndex], positions[i]) = (positions[i], positions[swapIndex]);
        }

        for (int i = 0; i < pieces.Count; i++)
        {
            PuzzlePiece piece = pieces[i];
            Vector2 pos = positions[i];
            float offsetX = UnityEngine.Random.Range(-cellWidth * 0.3f, cellWidth * 0.3f);
            float offsetY = UnityEngine.Random.Range(-cellHeight * 0.3f, cellHeight * 0.3f);
            pos += new Vector2(offsetX, offsetY);
            Vector2 correctPos = GetCorrectPosition(piece.gridPos);

            if (Vector2.Distance(pos, correctPos) < GetSnapDistance())
            {
                pos += new Vector2(GetSnapDistance(), GetSnapDistance());
            }

            piece.GetComponent<RectTransform>().anchoredPosition = pos;
            piece.ResetPiece();
        }
    }

    public Vector2 GetCorrectPosition(Vector2Int gp)
    {
        float startX = -puzzleSize.x / 2f + pieceSize.x / 2f;
        float startY = puzzleSize.y / 2f - pieceSize.y / 2f;

        return new Vector2(
            startX + gp.x * pieceSize.x,
            startY - gp.y * pieceSize.y
        );
    }

    public float GetSnapDistance()
    {
        return Mathf.Min(pieceSize.x, pieceSize.y) * 0.35f;
    }

    public void PlaySnapSound()
    {
        if (audioSource != null && snapClip != null)
            audioSource.PlayOneShot(snapClip);
    }

    public void CheckWin()
    {
        foreach (PuzzlePiece p in pieces)
        {
            if (!p.IsInCorrectPosition())
                return;
        }

        PuzzleEvents.OnPuzzleCompleted?.Invoke();
    }

    void ClearPuzzle()
    {
        foreach (PuzzlePiece p in pieces)
        {
            if (!p) continue;

            if (p.TryGetComponent(out Image img) && img.sprite)
                Destroy(img.sprite);

            Destroy(p.gameObject);
        }

        pieces.Clear();
    }
}
