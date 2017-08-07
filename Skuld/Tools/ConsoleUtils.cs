using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Skuld.Tools
{
    public class ConsoleUtils
    {
        public static string PrettyLines(List<string[]>lines, int padding=1)
        {
            int ElementCount = lines[0].Length;
            int[] MaxValues = new int[ElementCount];

            for (int i = 0; i < ElementCount; i++)
                MaxValues[i] = lines.Max(x => x[i].Length) + padding;

            StringBuilder sb = new StringBuilder();
            bool isFirst = true;

            foreach(var line in lines)
            {
                if (!isFirst)
                    sb.AppendLine();

                isFirst = false;

                for(int i=0;i<line.Length;i++)
                {
                    var value = line[i];
                    sb.Append(value.PadRight(MaxValues[i]));
                }
            }
            return Convert.ToString(sb);
        }
    }
}
