using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using ImageResizer.Models;

namespace ImageResizer.Services
{
    // ==========================================================
    //  IMAGE RESIZE SERVICE
    //  Provides image resizing functionality using two methods:
    //  - Pixel-based (nearest-neighbor)
    //  - Bilinear interpolation
    //  Includes runtime performance comparison and optional
    //  masking to exclude image borders during resize.
    // ==========================================================
    public class ImageResizeService
    {
        // ======================================================
        //  MAIN ENTRY POINT
        //  Loads the source image, resizes it using both methods,
        //  saves the output, and returns performance metrics and
        //  file paths as an ImageResizeResult object.
        // ======================================================
        public ImageResizeResult ProcessImage(string path, int targetSize, float maskOutPercent)
        {
            using var image = Image.Load<Rgba32>(path);
            int srcWidth = image.Width;
            int srcHeight = image.Height;
            int channels = 3;

            byte[] srcBytes = new byte[srcWidth * srcHeight * channels];
            int i = 0;

            // Copy RGB pixel data from ImageSharp image to byte array
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < srcHeight; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < srcWidth; x++)
                    {
                        srcBytes[i++] = row[x].R;
                        srcBytes[i++] = row[x].G;
                        srcBytes[i++] = row[x].B;
                    }
                }
            });

            // Calculate scale factor and target dimensions
            float scale = targetSize / (float)Math.Max(srcWidth, srcHeight);
            float targetWidthF = srcWidth * scale;
            float targetHeightF = srcHeight * scale;
            int targetWidth = (int)MathF.Round(targetWidthF);
            int targetHeight = (int)MathF.Round(targetHeightF);

            if (srcWidth >= srcHeight)
            {
                targetWidth = targetSize;
                targetHeight = (int)MathF.Round(srcHeight * (targetSize / (float)srcWidth));
            }
            else
            {
                targetHeight = targetSize;
                targetWidth = (int)MathF.Round(srcWidth * (targetSize / (float)srcHeight));
            }

            byte[] resizedPixel = new byte[targetWidth * targetHeight * channels];
            byte[] resizedBilinear = new byte[targetWidth * targetHeight * channels];

            // Pin the byte array for unmanaged memory access
            GCHandle handle = GCHandle.Alloc(srcBytes, GCHandleType.Pinned);
            IntPtr srcPtr = handle.AddrOfPinnedObject();

            // Run nearest-neighbor (pixel-based) resize
            var swPixel = Stopwatch.StartNew();
            ResizeImageWithMask(srcPtr, srcWidth, srcHeight, targetWidth, targetHeight, resizedPixel, channels, maskOutPercent);
            swPixel.Stop();

            // Run bilinear resize
            var swBilinear = Stopwatch.StartNew();
            ResizeImageBilinear(srcPtr, srcWidth, srcHeight, targetWidth, targetHeight, resizedBilinear, channels, maskOutPercent);
            swBilinear.Stop();

            handle.Free();

            // Save both output images
            string dir = Path.GetDirectoryName(path)!;
            string pixelOut = Path.Combine(dir, "Resized_Pixel.png");
            string bilinearOut = Path.Combine(dir, "Resized_Bilinear.png");

            SaveBytesAsPng(resizedPixel, targetWidth, targetHeight, channels, pixelOut);
            SaveBytesAsPng(resizedBilinear, targetWidth, targetHeight, channels, bilinearOut);

            // Determine which method was faster
            string msg = swBilinear.Elapsed.TotalMilliseconds < swPixel.Elapsed.TotalMilliseconds
                ? $"Bilinear method is {(swPixel.Elapsed.TotalMilliseconds / swBilinear.Elapsed.TotalMilliseconds):F2}× faster."
                : $"Pixel method is {(swBilinear.Elapsed.TotalMilliseconds / swPixel.Elapsed.TotalMilliseconds):F2}× faster.";

            // Build and return result
            return new ImageResizeResult
            {
                PixelImagePath = pixelOut,
                BilinearImagePath = bilinearOut,
                PixelTime = swPixel.Elapsed.TotalMilliseconds,
                BilinearTime = swBilinear.Elapsed.TotalMilliseconds,
                FasterMessage = msg,
                SourceWidth = srcWidth,
                SourceHeight = srcHeight,
                PixelWidth = targetWidth,
                PixelHeight = targetHeight,
                BilinearWidth = targetWidth,
                BilinearHeight = targetHeight
            };
        }

        // ======================================================
        //  BILINEAR RESIZE
        //  Uses bilinear interpolation with subpixel weighting.
        //  Processes the image in small tiles for cache efficiency.
        // ======================================================
        private unsafe void ResizeImageBilinear(IntPtr srcImagePtr, int srcWidth, int srcHeight, int targetWidth, int targetHeight, byte[] dst, int channels = 3, float maskOutPercent = 0.0f)
        {
            const int TILE = 256;
            int maskWidth = (int)(srcWidth * maskOutPercent);
            int maskHeight = (int)(srcHeight * maskOutPercent);
            float widthRatio = (float)(srcWidth - (2 * maskWidth)) / targetWidth;
            float heightRatio = (float)(srcHeight - (2 * maskHeight)) / targetHeight;

            int srcStride = srcWidth * channels;
            int dstStride = targetWidth * channels;

            int* xFloor = stackalloc int[targetWidth];
            int* yFloor = stackalloc int[targetHeight];
            byte* xWeight = stackalloc byte[targetWidth];
            byte* yWeight = stackalloc byte[targetHeight];

            for (int x = 0; x < targetWidth; x++)
            {
                float gx = (x * widthRatio) + maskWidth;
                int ix = (int)gx;
                if (ix >= srcWidth - 1) ix = srcWidth - 2;
                xFloor[x] = ix;
                xWeight[x] = (byte)((gx - ix) * 256f);
            }

            for (int y = 0; y < targetHeight; y++)
            {
                float gy = ((targetHeight - y - 1) * heightRatio) + maskHeight;
                int iy = (int)gy;
                if (iy >= srcHeight - 1) iy = srcHeight - 2;
                yFloor[y] = iy;
                yWeight[y] = (byte)((gy - iy) * 256f);
            }

            byte* srcPtr = (byte*)srcImagePtr.ToPointer();
            fixed (byte* dstPtr = dst)
            {
                for (int yBlock = 0; yBlock < targetHeight; yBlock += TILE)
                {
                    int yEnd = Math.Min(yBlock + TILE, targetHeight);
                    for (int y = yBlock; y < yEnd; y++)
                    {
                        int yF = yFloor[y];
                        int yN = yF + 1;
                        int wy = yWeight[y];
                        int wyInv = 256 - wy;
                        int row1 = yF * srcStride;
                        int row2 = yN * srcStride;
                        int dstRow = y * dstStride;

                        for (int x = 0; x < targetWidth; x++)
                        {
                            int xF = xFloor[x];
                            int xN = xF + 1;
                            int wx = xWeight[x];
                            int wxInv = 256 - wx;

                            int w00 = (wxInv * wyInv) >> 8;
                            int w10 = (wx * wyInv) >> 8;
                            int w01 = (wxInv * wy) >> 8;
                            int w11 = (wx * wy) >> 8;

                            byte* srcTL = srcPtr + row1 + xF * channels;
                            byte* srcTR = srcPtr + row1 + xN * channels;
                            byte* srcBL = srcPtr + row2 + xF * channels;
                            byte* srcBR = srcPtr + row2 + xN * channels;
                            byte* dstPixel = dstPtr + dstRow + x * channels;

                            dstPixel[0] = (byte)((srcTL[0] * w00 + srcTR[0] * w10 + srcBL[0] * w01 + srcBR[0] * w11) >> 8);
                            dstPixel[1] = (byte)((srcTL[1] * w00 + srcTR[1] * w10 + srcBL[1] * w01 + srcBR[1] * w11) >> 8);
                            dstPixel[2] = (byte)((srcTL[2] * w00 + srcTR[2] * w10 + srcBL[2] * w01 + srcBR[2] * w11) >> 8);
                        }
                    }
                }
            }
        }

        // ======================================================
        //  PIXEL-BASED RESIZE
        //  Simple nearest-neighbor resizing method.
        //  Reads pixels directly from memory and applies
        //  optional masking to exclude border areas.
        // ======================================================
        private void ResizeImageWithMask(IntPtr srcImagePtr, int srcWidth, int srcHeight, int targetWidth, int targetHeight, byte[] dst, int channels = 3, float maskOutPercent = 0.0f)
        {
            int maskWidth = (int)(srcWidth * maskOutPercent);
            int maskHeight = (int)(srcHeight * maskOutPercent);
            float widthRatio = (float)(srcWidth - (2 * maskWidth)) / targetWidth;
            float heightRatio = (float)(srcHeight - (2 * maskHeight)) / targetHeight;

            for (int y = targetHeight - 1; y >= 0; y--)
            {
                int srcY = (int)(((targetHeight - y - 1) * heightRatio) + maskHeight);
                for (int x = 0; x < targetWidth; x++)
                {
                    int srcX = (int)((x * widthRatio) + maskWidth);
                    int srcIndex = (srcY * srcWidth + srcX) * channels;
                    int dstIndex = (y * targetWidth + x) * channels;

                    for (int c = 0; c < channels; c++)
                    {
                        dst[dstIndex + c] = Marshal.ReadByte(srcImagePtr, srcIndex + c);
                    }
                }
            }
        }

        // ======================================================
        //  SAVE AS PNG
        //  Converts raw RGB byte array into an ImageSharp image
        //  and saves it as a PNG file on disk.
        // ======================================================
        private void SaveBytesAsPng(byte[] bytes, int w, int h, int channels, string path)
        {
            using var img = new Image<Rgba32>(w, h);

            img.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < h; y++)
                {
                    int srcY = h - 1 - y;
                    var row = accessor.GetRowSpan(y);

                    for (int x = 0; x < w; x++)
                    {
                        int srcIndex = (srcY * w + x) * channels;
                        row[x] = new Rgba32(
                            bytes[srcIndex],
                            bytes[srcIndex + 1],
                            bytes[srcIndex + 2],
                            255);
                    }
                }
            });

            img.SaveAsPng(path);
        }
    }
}
