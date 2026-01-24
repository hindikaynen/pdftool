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
