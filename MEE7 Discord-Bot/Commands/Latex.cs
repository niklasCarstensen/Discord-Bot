﻿using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MEE7.Commands
{
    public class Latex : Command
    {
        readonly string inputPath = "Commands\\Latex\\input.tex";
        readonly string batchPath = "Commands\\Latex\\latex.bat";
        readonly string folderPath = "Commands\\Latex";

        public Latex() : base("latex", "Renders latex strings", false)
        {

        }

        public override Task Execute(SocketMessage message)
        {
            lock (this)
            {
                string latex = message.Content.Split(new char[] { ' ', '\n' }).Skip(1).Aggregate((x, y) => x + " " + y);

                if (!latex.StartsWith("\\documentclass["))
                    latex = "\\documentclass[preview,border=12pt]{standalone}\n\\usepackage{amsmath}\n\\usepackage{tikz}\n\\begin{document}\n" + latex + "\n\\end{document}";

                if (!File.Exists(inputPath))
                    File.Create(inputPath).Dispose();
                File.WriteAllText(inputPath, latex);

                Process converter = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = batchPath
                    }
                };
                converter.Start();
                converter.WaitForExit();

                string[] outputFilePaths = Directory.GetFiles(folderPath).Where(x => Path.GetFileNameWithoutExtension(x).Contains("output") && x.EndsWith(".png")).ToArray();

                if (outputFilePaths.Length == 0)
                {
                    Program.SendText("That didn't work.", message.Channel).Wait();
                    return Task.FromResult(default(object));
                }

                foreach (string outputPath in outputFilePaths)
                {
                    Bitmap output = null;
                    using (Bitmap latexOutput = new Bitmap(outputPath))
                    {
                        output = new Bitmap(latexOutput.Width, latexOutput.Height);
                        using (Graphics graphics = Graphics.FromImage(output))
                        {
                            graphics.FillRectangle(Brushes.White, new Rectangle(0, 0, latexOutput.Width, latexOutput.Height));
                            graphics.DrawImage(latexOutput, new Point(0, 0));
                        }
                    }
                    Program.SendBitmap(output, message.Channel).Wait();
                    output.Dispose();
                }

                outputFilePaths.Select(x => { File.Delete(x); return x; } ).ToArray();
            }

            return Task.FromResult(default(object));
        }
    }
}
