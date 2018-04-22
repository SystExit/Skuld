using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Skuld.Utilities
{
    public class ConsoleUtils
    {
        public static string PrettyLines(List<string[]> lines, int padding=1)
        {
            int elementCount = lines[0].Length;
            int[] maxValues = new int[elementCount];

            for (int i = 0; i < elementCount; i++)
                maxValues[i] = lines.Max(x => x[i].Length) + padding;

            var sb = new StringBuilder();
            bool isFirst = true;

            foreach(var line in lines)
            {
                if (!isFirst)
                    sb.AppendLine();

                isFirst = false;

                for(int i=0;i<line.Length;i++)
                {
                    var value = line[i];
                    sb.Append(value.PadRight(maxValues[i]));
                }
            }
            return Convert.ToString(sb);
        }
    }
}
