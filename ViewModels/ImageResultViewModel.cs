namespace ImageResizer.Models
{
    // ==========================================================
    //  IMAGE RESULT VIEW MODEL
    //  Represents all data required by the UI to display results
    //  after performing image resizing with both algorithms.
    //  Includes file paths, dimensions, performance metrics,
    //  and optional error information.
    // ==========================================================
    public class ImageResultViewModel
    {
        // ======================================================
        //  IMAGE PATHS
        // ======================================================

        // Path to the original source image.
        public string? SourceImage { get; set; }

        // Path to the image resized with pixel-based interpolation.
        public string? PixelImage { get; set; }

        // Path to the image resized with bilinear interpolation.
        public string? BilinearImage { get; set; }

        // ======================================================
        //  PERFORMANCE METRICS
        // ======================================================

        // Processing time (in milliseconds) for pixel-based resize.
        public double TimePixel { get; set; }

        // Processing time (in milliseconds) for bilinear resize.
        public double TimeBilinear { get; set; }

        // Indicates which method performed faster.
        public string? Faster { get; set; }

        // ======================================================
        //  DIMENSIONS
        // ======================================================

        // Width of the source image (in pixels).
        public int SourceWidth { get; set; }

        // Height of the source image (in pixels).
        public int SourceHeight { get; set; }

        // Width of the pixel-based resized image.
        public int PixelWidth { get; set; }

        // Height of the pixel-based resized image.
        public int PixelHeight { get; set; }

        // Width of the bilinear resized image.
        public int BilinearWidth { get; set; }

        // Height of the bilinear resized image.
        public int BilinearHeight { get; set; }

        // ======================================================
        //  ERROR HANDLING
        // ======================================================

        // Stores an error message if the resize operation failed.
        public string? Error { get; set; }
    }
}
