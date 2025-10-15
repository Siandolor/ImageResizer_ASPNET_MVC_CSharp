namespace ImageResizer.Models
{
    // ==========================================================
    //  IMAGE RESIZE RESULT MODEL
    //  Holds metadata and performance statistics for both
    //  pixel-based (nearest-neighbor) and bilinear interpolation
    //  resize operations. Includes output paths, timings, and
    //  original/resized image dimensions.
    // ==========================================================
    public class ImageResizeResult
    {
        // ======================================================
        //  FILE PATHS
        // ======================================================

        // Path to the output image generated using
        // pixel-based (nearest-neighbor) interpolation.
        public string PixelImagePath { get; set; }

        // Path to the output image generated using
        // bilinear interpolation.
        public string BilinearImagePath { get; set; }

        // ======================================================
        //  PERFORMANCE METRICS
        // ======================================================

        // Processing time (in ms) for the pixel-based resize.
        public double PixelTime { get; set; }

        // Processing time (in ms) for the bilinear resize.
        public double BilinearTime { get; set; }

        // Indicates which interpolation method was faster.
        public string FasterMessage { get; set; }

        // ======================================================
        //  DIMENSIONS
        // ======================================================

        // Width of the original source image (in pixels).
        public int SourceWidth { get; set; }

        // Height of the original source image (in pixels).
        public int SourceHeight { get; set; }

        // Output width for the pixel-based resized image.
        public int PixelWidth { get; set; }

        // Output height for the pixel-based resized image.
        public int PixelHeight { get; set; }

        // Output width for the bilinear resized image.
        public int BilinearWidth { get; set; }

        // Output height for the bilinear resized image.
        public int BilinearHeight { get; set; }
    }
}
