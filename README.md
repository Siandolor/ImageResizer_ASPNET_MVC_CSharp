
# ImageResizer

A lightweight **ASP.NET Core MVC** application for comparing **two image resizing algorithms** — pixel-based and bilinear interpolation.  
Built using **C#**, **ImageSharp**, and **.NET 8**, this project demonstrates efficient image scaling, masking, and performance benchmarking.  
Target output is **.png**.

---

## Features

- **Upload Images**  
  Upload `.png`, `.jpg`, or `.jpeg` images directly from your browser.
- **Dual Resize Modes**
  - **Pixel-based:** Simple nearest-neighbor resizing.
  - **Bilinear:** Smooth interpolation using weighted averages.
- **Output Images**  
  Output `.png` images directly to your browser.
- **Performance Benchmarking**  
  Compares both methods and reports execution time in milliseconds.
- **Mask-Out Region**  
  Optionally exclude edge areas (0.0–0.5) for cleaner results.
- **Responsive UI**  
  Clean Bootstrap layout with instant visual feedback.

---

## Project Structure

```
ImageResizer/
├── Controllers/
│   └── ImageController.cs          # Handles upload & resize actions
│
├── Models/
│   └── ImageResizeResult.cs        # Internal result model
│
├── ViewModels/
│   ├── ImageResultViewModel.cs     # View model for UI output
│   └── ErrorViewModel.cs           # Error handler
│
├── Services/
│   └── ImageResizeService.cs       # Core logic for both resize methods
│
├── Views/
│   ├── Image/Index.cshtml          # Upload & result display view
│   └── Shared/_Layout.cshtml       # Main layout (Bootstrap-based)
│
├── wwwroot/
│   ├── css/site.css                # UI styling
│   └── resized/                    # Output folder for processed images
│
├── Program.cs                      # App configuration & routing
└── README.md
```

---

## How It Works

1. **Image Upload**
   - File is saved to `/wwwroot/resized/`.
2. **Processing**
   - Both resizing methods run sequentially using unsafe pointer operations for speed.
3. **Timing & Output**
   - Execution times are measured with `Stopwatch` and displayed in the UI.
4. **Results**
   - The app displays original, pixel, and bilinear images side by side with resolution details.

---

## Setup & Run

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- Visual Studio / VS Code with C# extension

### Installation

```bash
  git clone https://github.com/Siandolor/ImageResizer_ASPNET_MVC_CSharp.git
  cd ImageResizer
  dotnet restore
  dotnet run
```

Then open your browser and navigate to:

```
https://localhost:5001
```

---

## Technical Notes

- **Framework:** ASP.NET Core MVC (.NET 8)
- **Imaging Library:** SixLabors.ImageSharp
- **Algorithm:** Pixel-based and Bilinear resizing (unsafe pointer math for performance)
- **Language:** C#
- **UI:** Bootstrap 5 + Razor Views
- **Output:** PNG images saved in `/wwwroot/resized/`

---

## Author
**Daniel Fitz, MBA, MSc, BSc**  
Vienna, Austria  
Developer & Security Technologist — *Post-Quantum Cryptography, Blockchain/Digital Ledger & Simulation*  
C/C++ · C# · Java · Python · Visual Basic · ABAP · JavaScript/TypeScript

International Accounting · Macroeconomics & International Relations · Physiotherapy · Computer Sciences  
Former Officer of the German Federal Armed Forces

---

## License
**MIT License** — free for educational and research use.  
Attribution required for redistribution or commercial adaptation.

---

> “Measure twice, resize once.”  
> — Daniel Fitz, 2025
