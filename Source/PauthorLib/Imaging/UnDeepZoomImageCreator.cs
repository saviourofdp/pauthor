//
// Pauthor - An authoring library for Pivot collections
// http://getpivot.com
//
// Copyright (c) 2010, by Microsoft Corporation
// All rights reserved.
//

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.XPath;

using Microsoft.LiveLabs.Pauthor.Crawling;

namespace Microsoft.LiveLabs.Pauthor.Imaging
{
    /// <summary>
    /// UnDeepZoomImageCreator converts a DeepZoom image back into a plain bitmap image.
    /// </summary>
    /// <remarks>
    /// The end result contains the same image as was used to create the original DeepZoom image in a JPEG format at the
    /// highest quality.
    /// </remarks>
    public class UnDeepZoomImageCreator : ImageCreator
    {
        private static PauthorLog Log = PauthorLog.Global;

        /// <summary>
        /// Creates a new image creator.
        /// </summary>
        public UnDeepZoomImageCreator()
        {
            // Do nothing.
        }

        /// <summary>
        /// Converts a DeepZoom image back into an ordinary bitmap.
        /// </summary>
        /// <param name="dziPath">the path (relative or absolute) to the DZI file to convert</param>
        /// <returns>an absolute path to the resulting image within this image creator's working directory</returns>
        public String UnDeepZoomImage(String dziPath)
        {
            try
            {
                String baseName = Path.GetFileNameWithoutExtension(dziPath);
                String finalImagePath = Path.Combine(this.WorkingDirectory, baseName + StandardImageFormatExtension);
                if (File.Exists(finalImagePath)) return finalImagePath;

                String baseDirectory = Directory.GetParent(dziPath).FullName;
                String filesDirectory = Path.Combine(baseDirectory,
                    Path.GetFileNameWithoutExtension(dziPath) + "_files");
                String maxDepthDirectory = this.FindMaxDepthDirectory(filesDirectory);
                String dziText = File.ReadAllText(dziPath);

                String xmlData = File.ReadAllText(dziPath);
                XPathHelper dzi = new XPathHelper(xmlData);
                String xmlns = this.DetermineNamespace(xmlData);
                if (xmlns != null)
                {
                    dzi.AddNamespace("d", xmlns);
                }

                String format = dzi.FindString("//d:Image/@Format");
                int tileSize = dzi.FindInt("//d:Image/@TileSize");
                int overlap = dzi.FindInt("//d:Image/@Overlap");
                int width = dzi.FindInt("//d:Size/@Width");
                int height = dzi.FindInt("//d:Size/@Height");

                Bitmap finalImage = new Bitmap(width, height);
                Graphics finalImageGraphics = Graphics.FromImage(finalImage);

                int colCount = (int)Math.Ceiling((double)width / tileSize);
                int rowCount = (int)Math.Ceiling((double)height / tileSize);
                for (int row = 0; row < rowCount; row++)
                {
                    for (int col = 0; col < colCount; col++)
                    {
                        String tileName = col + "_" + row + "." + format;
                        String tilePath = Path.Combine(maxDepthDirectory, tileName);
                        if (File.Exists(tilePath) == false)
                        {
                            Log.Warning("Could not find tile {0}, {1} for DZI {2} at zoom level {3}. " +
                                "The image for this tile may be incomplete.", col, row, Path.GetFileName(dziPath),
                                Path.GetFileName(maxDepthDirectory));
                            continue;
                        }

                        Bitmap tile = new Bitmap(tilePath);
                        float tileX = col * (tileSize);
                        float tileY = row * (tileSize);
                        finalImageGraphics.DrawImage(tile, tileX, tileY);
                    }
                }

                ImageCodecInfo codecInfo = ImageCodecInfo.GetImageEncoders()[StandardImageEncoder];
                EncoderParameters encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);

                finalImage.Save(finalImagePath, codecInfo, encoderParameters);
                return finalImagePath;
            }
            catch (XPathException e)
            {
                throw new PauthorException("Unable to decode DZI: " + dziPath, e);
            }
            catch (Exception e)
            {
                throw new PauthorException("Unable to un-deepzoom DZI: " + dziPath, e);
            }
        }

        private String FindMaxDepthDirectory(String filesDirectory)
        {
            int maxDepth = 0;
            foreach (String subdirectory in Directory.GetDirectories(filesDirectory))
            {
                String subdirectoryName = Path.GetFileName(subdirectory);
                int currentDepth = 0;
                if (Int32.TryParse(subdirectoryName, out currentDepth))
                {
                    maxDepth = Math.Max(maxDepth, currentDepth);
                }
            }
            return Path.Combine(filesDirectory, maxDepth.ToString());
        }

        private String DetermineNamespace(String xmlData)
        {
            String xmlnsTag = "xmlns=\"";
            int startIndex = xmlData.IndexOf(xmlnsTag);
            if (startIndex == -1) return null;

            startIndex += xmlnsTag.Length;
            int endIndex = xmlData.IndexOf('"', startIndex);
            if (endIndex == -1) return null;

            String xmlns = xmlData.Substring(startIndex, endIndex - startIndex);
            return xmlns;
        }
    }
}
