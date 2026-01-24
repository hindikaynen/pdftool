using System;
using System.IO;
using PDFtoImage;

namespace PdfTool.Tests;

public static class PdfRasterizer
{
    /// <summary>
    /// Renders a single PDF page to PNG using PDFtoImage (pdfium-based renderer).
    /// Page number is 1-based.
    /// </summary>
    public static void RenderPageToPng(string pdfPath, int pageNumber1Based, int dpi, string pngPath)
    {
        if (pageNumber1Based < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber1Based));

        Directory.CreateDirectory(Path.GetDirectoryName(pngPath)!);

        using var pdfStream = File.OpenRead(pdfPath);

        // PDFtoImage uses 0-based page index.
        var pageIndex0 = pageNumber1Based - 1;

        Conversion.SavePng(
            pngPath,
            pdfStream,
            pageIndex0,
            options: new RenderOptions(Dpi: dpi, WithAnnotations: true)
        );
    }
}
