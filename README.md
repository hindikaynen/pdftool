# pdftool (JSON spec, pt-only) + NUnit tests (readable JSON)

Notes:
- Tests use C# raw string literals (""" ... """) for readable JSON. Requires C# 11+ (we use net8 + LangVersion latest).
- Tests include explicit `using System;` and `using System.IO;` to minimize future refactoring.

## Primitives v1 (fixed)
- rect: { type:"rect", rect:[x,y,w,h], cornerRadius?, fill?, stroke?, strokeWidth? }
- line: { type:"line", from:[x,y], to:[x,y], stroke?, strokeWidth? }
- text:
  - point text: { type:"text", at:[x,y], value, ... }
  - block text: { type:"text", rect:[x,y,w,h], wrap:true, value, ... }
- image (base64 only): { type:"image", rect:[x,y,w,h], data:{ mime:"image/png|image/jpeg", base64:"..." } }
- barcode: { type:"barcode", kind:"qr|code128", rect:[x,y,w,h], value:"...", options?:{...} }

## Run tests
```bash
dotnet test pdftool.sln
```


- PagesRangeParser.Resolve(range, totalPages) resolves "last" and returns distinct sorted pages.

## Apply (step 0/1)
```bash
dotnet run --project pdftool -- apply --in input.pdf --out output.pdf --json overlays.json
```


## PlacementResolver (corner)
- Coordinates are in PDF user space (origin bottom-left).
- Corner origin is (0,0)/(w,0)/(0,h)/(w,h) plus offset [dx,dy].
  - Example: topLeft with offset [10,-20] means 10pt right and 20pt down.

## Golden master renderer tests (Phase 1)
- Tests rasterize output PDF pages to PNG using PDFiumSharp and compare against baselines in `pdftool.Tests/TestData/expected`.
- To generate / update baselines:
  - edit `pdftool.Tests/GoldenTestConfig.cs` and enable `#define UPDATE_BASELINES`
  - run `dotnet test`
  - review generated/updated PNGs in `TestData/expected` and commit them
