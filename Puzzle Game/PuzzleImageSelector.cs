using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PuzzleImageSelector : MonoBehaviour
{
    [SerializeField] private List<Sprite> puzzleImages = new();
    [SerializeField] private bool randomizeOnStart = true;
    [SerializeField] private Image previewImage;
    [SerializeField] private Text imageNameText;
    private int currentIndex = -1;
    private Sprite selectedImage;

    void Start()
    {
        if (randomizeOnStart)
            SelectNewRandomImage();
    }

    public void SelectNewRandomImage()
    {
        if (puzzleImages.Count == 0) return;
        int newIndex = currentIndex;

        if (puzzleImages.Count > 1)
        {
            while (newIndex == currentIndex)
            {
                newIndex = Random.Range(0, puzzleImages.Count);
            }
        }

        currentIndex = newIndex;
        ApplySelection();
    }

    public void SelectImageByIndex(int index)
    {
        if (index < 0 || index >= puzzleImages.Count) return;
        currentIndex = index;
        ApplySelection();
    }

    public void NextImage()
    {
        if (puzzleImages.Count == 0) return;
        currentIndex = (currentIndex + 1) % puzzleImages.Count;
        ApplySelection();
    }

    public void PreviousImage()
    {
        if (puzzleImages.Count == 0) return;
        currentIndex = (currentIndex - 1 + puzzleImages.Count) % puzzleImages.Count;
        ApplySelection();
    }

    void ApplySelection()
    {
        selectedImage = puzzleImages[currentIndex];
        if (previewImage) previewImage.sprite = selectedImage;
        if (imageNameText) imageNameText.text = selectedImage.name;
        PuzzleEvents.OnImageChanged?.Invoke(selectedImage);
    }

    public Sprite GetSelectedImage() => selectedImage;

    public int GetImageCount() => puzzleImages.Count;

    public Sprite GetImageByIndex(int i) => puzzleImages[i];
}
