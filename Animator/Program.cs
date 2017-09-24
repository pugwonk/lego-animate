using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Newtonsoft.Json;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Animator
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }
            // Options were parsed fine
            int frames;
            if (options.inBluePrint != null)
            {
                List<int> itemIds = loadBluePrint(options);
                frames = createPov(itemIds, options);
            } else
            {
                frames = createPov(null, options);
            }
            createIni(frames, options);
        }

        private static List<int> loadBluePrint(Options options)
        {
            string infile = File.ReadAllText(options.inBluePrint);
            JObject d = JObject.Parse(infile);
            List<int> ids = new List<int>();

            doSteps(d["model"]["steps"], ids);
            return ids;
        }

        private static void doSteps(JToken steps, List<int> ids)
        {
            foreach (JToken step in steps)
            {
                //var b = steps["parts"];
                //var x = steps["submodels"];
                foreach (var part in step["parts"])
                {
                    string pid = (string)part.SelectToken("id");
                    ids.Add(int.Parse(pid));
                }
                foreach (var subm in step["submodels"])
                {
                    //var x = subm["isCallout"];
                    doSteps(subm["steps"], ids);
                }
            }
        }

        private static void createIni(int frames, Options options)
        {
            // Create INI
            string line;
            var sb = new StringBuilder();
            System.IO.StreamReader file = new System.IO.StreamReader("../../template.ini");
            while ((line = file.ReadLine()) != null)
            {
                if (line.Equals("Final_Frame="))
                    line += options.frameCount.ToString();
                if (line.Equals("Final_Clock="))
                    line += options.degreeSpin.ToString();
                if (line.Equals("Width="))
                    line += options.imgWidth.ToString();
                if (line.Equals("Height="))
                    line += options.imgHeight.ToString();
                if (line.Equals("Antialias="))
                    line += (options.useAA ? "Yes" : "No");
                if (line.Equals("Quality="))
                    line += options.quality.ToString();
                sb.AppendLine(line);
            }

            file.Close();
            using (System.IO.StreamWriter files = new System.IO.StreamWriter(options.inPov.ToLower().Replace(".pov", "-anim.ini")))
            {
                files.WriteLine(sb.ToString());
            }
        }

        private static int createPov(List<int> itemIds, Options options)
        {
            // Create POV
            string line;
            int onItem = -1;
            int lastItem = -1;
            var sb = new StringBuilder();
            System.IO.StreamReader file = new System.IO.StreamReader(options.inPov);
            while ((line = file.ReadLine()) != null)
            {
                if (line.Equals("#declare ldd_model_transformation = transform { translate <0,0,0> }"))
                    line = "#declare ldd_model_transformation = transform { translate <2,1,1> }";
                if (line.Equals("#declare ldd_model = union {"))
                    onItem = 0;
                if ((onItem > -1) && (line.Equals("}")))
                {
                    sb.AppendLine("rotate <0,clock,0>");
                    onItem = -1;
                }
                if (onItem > 0)
                {
                    // Find where this item appears in the ordered list
                    int itemTiming;
                    if (true)
                        itemTiming = onItem - 1;
                    else
                    {
                        itemTiming = itemIds.FindIndex(a => a.Equals(onItem - 1));
                        if (itemTiming == -1)
                            break;
                    }
                    sb.AppendLine("#if(clock >= " + (itemTiming * ((double)options.finishBuildDegree / itemIds.Count)).ToString() + ")");
                    sb.AppendLine(line);
                    sb.AppendLine("#end");
                    lastItem = onItem;
                }
                else
                {
                    sb.AppendLine(line);
                }
                if (onItem != -1)
                    onItem++;
            }

            file.Close();
            using (System.IO.StreamWriter files = new System.IO.StreamWriter(options.inPov.ToLower().Replace(".pov", "-anim.pov")))
            {
                files.WriteLine(sb.ToString());
            }
            return lastItem;
        }
    }
}
