﻿using System;
using System.Drawing;
using System.IO;
using CommandLine;
using Pack.Core.Image;
using Pack.Runner.Options;
using SevenZip;

namespace Pack.Runner
{
    class Program
    {
        static int Main(string[] args)
        {
            var options = new MainOptions();
            var result = 1;
            if (!Parser.Default.ParseArgumentsStrict(args, options,
                (verb, subOptions) =>
                {
                    if (subOptions == null) return;
                    try
                    {
                        if (verb == "make")
                        {
                            var makeOptions = subOptions as MakeOptions;
                            result = MakeImage(makeOptions);
                        }

                        if (verb == "check")
                        {
                            var checkOptions = subOptions as CheckOptions;
                            result = CheckImage(checkOptions);
                        }

                        if (verb == "open")
                        {
                            var openOptions = subOptions as OpenOptions;
                            result = OpenImage(openOptions);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        result = 1;
                    }
                }))
            {
                return 1;
            }
            return result;
        }

        private static int OpenImage(OpenOptions openOptions)
        {
            if (openOptions == null) throw new ArgumentNullException(nameof(openOptions));
            if (!File.Exists(openOptions.InputFilePath))
                throw new FileNotFoundException(@"Input file not found",
                    openOptions.InputFilePath);

            using (var bmp = (Bitmap) Image.FromFile(openOptions.InputFilePath))
            {
                var output = openOptions.OutputFilePath.TrimEnd('\\');
                var res = Reader.ReadData(bmp, SevenZipExtractor.ExtractBytes);
                if (Path.HasExtension(output))
                {
                    File.WriteAllBytes(output, res.Item2);
                }
                else
                {
                    File.WriteAllBytes(Path.Combine(output, res.Item1),
                        res.Item2);
                }
            }
            return 0;
        }

        private static int CheckImage(CheckOptions checkOptions)
        {
            if (checkOptions == null) throw new ArgumentNullException(nameof(checkOptions));
            if (!File.Exists(checkOptions.InputFilePath))
                throw new
                    FileNotFoundException(
                    @"Input file not found", checkOptions.InputFilePath);

            using (var bmp = (Bitmap)Image.FromFile(checkOptions.InputFilePath))
            {
                return Detector.ImageIsPacked(bmp)
                    ? 0
                    : 1;
            }
        }

        private static int MakeImage(MakeOptions makeOptions)
        {
            if (makeOptions == null) throw new ArgumentNullException(nameof(makeOptions));
            if (!File.Exists(makeOptions.InputFilePath))
                throw new FileNotFoundException(
                    @"Input file not found", makeOptions.InputFilePath);

            var random = new Random();
            var palette = new[]
            {
                Color.FromArgb(86, 81, 117),
                Color.FromArgb(83, 138, 149),
                Color.FromArgb(103, 183, 158),
                Color.FromArgb(255, 183, 39),
                Color.FromArgb(228, 73, 28)
            };

            using (var bmp = Writer.CreatePackedImage(File.ReadAllBytes(makeOptions.InputFilePath),
                Path.GetFileName(makeOptions.InputFilePath),
                (_) => palette[random.Next(0, palette.Length)],
                SevenZipCompressor.CompressBytes,
                200,
                200))
            {
                bmp.Save(makeOptions.OutputFilePath);
            }

            return 0;
        }
    }
}
