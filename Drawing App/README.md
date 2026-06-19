# Unity Drawing App — Tam Özellikli Çizim Tuvali

## Kurulum

1. Unity 2022.3+ LTS projesi aç
2. `Assets/Scripts/` klasörünü projeye kopyala
3. `BrushSettings` ScriptableObject oluştur:
   - Project panelinde sağ tık → Create → Drawing → BrushSettings
4. Sahneyi kur (aşağıya bak)

## Sahne Kurulumu

```
Main (GameObject)
├── Managers (GameObject)
│   ├── LayerManager.cs        ← canvasWidth/Height ayarla
│   ├── UndoRedoManager.cs
│   ├── SaveManager.cs
│   └── AudioManager.cs        ← AudioClip'leri Inspector'dan bağla
│
├── Canvas (Unity UI Canvas — Screen Space Overlay)
│   ├── DrawingSurface (RawImage)
│   │   └── DrawingCanvas.cs
│   │
│   └── UI Panel
│       ├── Toolbar
│       │   └── UIManager.cs   ← Butonları Inspector'dan bağla
│       ├── LayerPanel
│       │   └── LayerPanelUI.cs
│       └── ColorPanel
│           └── ColorWheelUI.cs
```

## Klavye Kısayolları

| Tuş | Eylem |
|-----|-------|
| B | Fırça |
| E | Silgi |
| F | Flood fill |
| I | Damlalık |
| Ctrl+Z | Geri al |
| Ctrl+Y | İleri al |

## Özellikler

- **Fırça**: Yumuşak/sert kenar (hardness), opasite, renk karıştırma
- **Araçlar**: Fırça, silgi, flood fill, şekil (çizgi/dikdörtgen/elips), damlalık
- **Katmanlar**: Ekle, sil, gizle/göster, opasite, merge down
- **Undo/Redo**: 50 adım, tüm katmanlar snapshot
- **Kayıt**: PNG (düzleştirilmiş) + native format (katman katman)
- **Renk**: HSV slider, hex input
- **Ses**: Fırça, silgi, fill, undo, kayıt sesleri

## Dosya Yapısı

```
Scripts/
├── Core/
│   ├── BrushSettings.cs      — ScriptableObject: araç & fırça ayarları
│   ├── DrawingCanvas.cs      — Ana çizim motoru, pointer events
│   └── LayerManager.cs       — Katman yönetimi
├── Tools/
│   ├── BrushTool.cs          — Fırça / silgi çizimi
│   ├── FillTool.cs           — Flood fill
│   ├── ShapeTool.cs          — Geometrik şekiller
│   └── EyedropperTool.cs     — Renk örnekleme
├── IO/
│   ├── UndoRedoManager.cs    — Undo/redo stack
│   └── SaveManager.cs        — PNG & native kayıt/yükleme
├── Audio/
│   └── AudioManager.cs       — Ses yönetimi
└── UI/
    ├── UIManager.cs          — Buton & slider bağlantıları
    ├── LayerPanelUI.cs       — Katman listesi UI
    └── ColorWheelUI.cs       — HSV renk seçici
```
