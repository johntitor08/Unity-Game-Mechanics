# 🧩 Unity Puzzle Oyunu – TAM & GÜNCEL DOKÜMANTASYON

Bu doküman, proje üzerinde **son noktaya kadar birlikte ulaştığımız**, çalışan ve sadeleştirilmiş sürümün **tamamını** anlatır.
Amaç: Bu dosyayı açan biri **hiç soru sormadan** projeyi kurabilsin, çalıştırabilsin ve genişletebilsin.

---

## 1️⃣ Projenin Genel Özeti

Bu proje, Unity UI sistemi kullanılarak geliştirilmiş, sürükle-bırak tabanlı bir **puzzle oyunudur**.

### 🎮 Temel Özellikler

✔ Grid tabanlı puzzle sistemi
✔ Zorluk modları (Kolay / Orta / Zor)
✔ Sabit puzzle alanı
✔ Otomatik shuffle
✔ Drag & snap sistemi
✔ Doğru yerde kilitlenen parçalar
✔ Hamle sayacı
✔ Süre sayacı
✔ Win panel (zafer ekranı)

## 2️⃣ Zorluk Modları

Puzzle alanı **her zaman sabittir**:

Puzzle Alanı: 1200 × 900

Zorluk sadece **grid boyutunu** değiştirir.

| Zorluk   | Grid  | Parça Boyutu |
| -------- | ----- | ------------ |
| 🟢 Kolay | 4 × 4 | 300 × 225    |
| 🟡 Orta  | 5 × 5 | 240 × 180    |
| 🔴 Zor   | 6 × 6 | 200 × 150    |

Parça boyutları **manuel ayarlanmaz**, runtime’da otomatik hesaplanır.

---

## 3️⃣ Gerekli Script Dosyaları

Zorunlu script’ler:

* `PuzzleManager.cs`
* `PuzzlePiece.cs`
* `GameUI.cs`
* `PuzzleImageSelector.cs`
* `PuzzleMainMenu.cs`

Destekleyici:

* `PuzzleEvents.cs`

---

## 4️⃣ Canvas ve UI Kurulumu

### Canvas Oluşturma

UI → Canvas

***Canvas Ayarları***

* Render Mode: Screen Space - Overlay
* Canvas Scaler:

  * UI Scale Mode: Scale With Screen Size
  * Reference Resolution: 1920 × 1080
  * Match: 0.5

EventSystem yoksa:

UI → Event System

## 5️⃣ Sahne Hiyerarşisi (NET YAPI)

Canvas
├── TopBar
│   ├── MovesText (TMP)
│   └── TimeText (TMP)
│
├── GameArea (Panel)   → 1200 × 900
│   └── PuzzleContainer (RectTransform)
│
├── WinPanel (Panel)
│   ├── TitleText (TMP)
│   ├── WinMovesText (TMP)
│   ├── WinTimeText (TMP)
│   └── ReplayButton
│
└── MainMenu (opsiyonel)
    ├── EasyButton
    ├── MediumButton
    └── HardButton

## 6️⃣ GameArea & PuzzleContainer

### GameArea

* Width: 1200
* Height: 900
* Anchor: Middle Center

### PuzzleContainer

* Anchor: Stretch – Stretch
* Offsets: 0

> PuzzleManager, oyun başında container boyutunu **kilitler**.

---

## 7️⃣ PuzzlePiece Prefab

UI → Image → PuzzlePiece

**Component’ler:**

* RectTransform
* Image
* PuzzlePiece

Prefab oluştur → sahnedeki geçici objeyi sil.

---

## 8️⃣ PuzzleManager.cs – Görevleri

PuzzleManager oyunun **beynidir**.

### Sorumluluklar

* Puzzle alanını sabitlemek
* Grid’e göre parça üretmek
* Görseli parçalara bölmek
* Shuffle yapmak
* Snap mesafesini hesaplamak
* Kazanma kontrolü

### Önemli Notlar

* Grid değeri **zorluk seçimiyle** değişir
* Puzzle her restart’ta tamamen yeniden kurulur

---

## 9️⃣ Shuffle Sistemi

* Puzzle oluşturulduktan sonra otomatik çalışır
* Parçalar puzzle alanının sağına rastgele dağılır
* Her restart’ta tekrar edilir

---

## 🔟 Drag & Snap Sistemi

* Mouse pozisyonu UI koordinatlarıyla birebir eşlenir
* Parça doğru noktaya yeterince yaklaşınca snap olur
* Snap sonrası:

  * `raycastTarget` kapatılır
  * Parça tekrar sürüklenemez

---

## 1️⃣1️⃣ Hamle Sistemi

* Her **ilk drag başlangıcı** = 1 hamle
* `PuzzleEvents.OnMoveMade` tetiklenir
* Hamle bilgisi üst bardan izlenir

---

## 1️⃣2️⃣ Süre Sistemi

* Puzzle başladığında otomatik başlar
* Win olduğunda durur
* Format:

MM:SS

---

## 1️⃣3️⃣ Win Panel (Zafer Ekranı)

### Davranış

* Tüm parçalar doğru yerindeyse açılır
* Süre durur
* Hamle ve süre bilgileri gösterilir

### WinPanel Ayarları

* Başlangıçta kapalı
* Anchor: Stretch – Stretch
* Yarı saydam arkaplan önerilir

---

## 1️⃣4️⃣ Replay (Tekrar Oyna)

Replay butonu:

* Puzzle yeniden oluşturulur
* Süre sıfırlanır
* Hamle sıfırlanır
* Parçalar yeniden shuffle edilir

---

## 1️⃣5️⃣ Oyun Akışı (ÖZET)

1️⃣ Oyuncu zorluk seçer
2️⃣ Grid ayarlanır
3️⃣ Puzzle oluşturulur
4️⃣ Süre başlar
5️⃣ Oyuncu parçaları yerleştirir
6️⃣ Puzzle tamamlanır
7️⃣ Win panel açılır 🎉

---

## ✅ SON DURUM

Bu proje:

* Temiz mimariye sahip
* Event tabanlı
* Kolayca genişletilebilir
* Production seviyesine yakındır

---

## 🚀 Genişletme Önerileri

* ⭐ Zorluğa göre yıldız sistemi
* 💾 En iyi skor kaydı
* 🎵 Ses & animasyonlar
* ⏱️ Süre limitli challenge mode

---

> Bu doküman, projenin **referans kaynağıdır**.
> Kod değiştikçe burası güncellenmelidir.
