using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GCodeThumbnailJoiner
{
    internal class Program
    {
        private static string _path = @"D:\Downloads\uploads\";

        static void Main()
        {
            foreach (var gcodeFile in Directory.GetFiles(_path, "*.gcode", SearchOption.AllDirectories))
            {
                string pngPath = Path.ChangeExtension(gcodeFile, ".png");
                if (File.Exists(pngPath))
                {
                    List<string> originalGcode = File.ReadAllLines(gcodeFile).ToList();
                    string imageBase64 = Convert.ToBase64String(File.ReadAllBytes(pngPath));

                    int insertIndex = 0;
                    foreach (var line in originalGcode)
                    {
                        if (line.Contains("Generated with Cura"))
                            break;

                        insertIndex++;
                    }

                    List<string> base64Chunks = ChunksUpto(imageBase64, 78).ToList();

                    var sb = new StringBuilder();
                    sb.AppendLine($"; thumbnail begin 600 600 {imageBase64.Length}");

                    foreach (var chunk in base64Chunks)
                        sb.AppendLine($"; {chunk}");

                    sb.AppendLine("; thumbnail end");

                    List<string> newGCode = originalGcode;
                    newGCode.Insert(insertIndex + 1, sb.ToString());

                    File.Move(gcodeFile, gcodeFile + ".bak");
                    File.WriteAllText(gcodeFile, string.Join("\r\n", newGCode));
                }
            }
        }

        private static IEnumerable<string> ChunksUpto(string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }
    }
}
