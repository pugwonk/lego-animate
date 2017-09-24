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
        // Order of operations:
        // * Save LXF (zoomed nicely) in input/original.lxf
        // * Run Blueprint, save as input/original.blueprint
        // * Convert LDD to Pov-Ray as input/original.pov
        // * Leave LDD running
        // * Run this code
        // * Delete *.png in output folder
        // * Load output/main.ini in Pov-Ray
        // * Convert to video in VirtualDub (get the XVID codec first and use that)

        // Todo:
        // * Fix (remove) the extra unions that LDD to Pov-Ray sometimes makes
        // * #if the includes that aren't needed yet at this stage in the render
        // * Make command line parameters
        // * Make it comment each item number in the POV file
        // * Change output filenames to myfile-anim.pov and myfile-anim.ini
        // * Remove the code translating the model location
        // * Embed template.ini in project

        const int frameCount = 250;
        const int degreeSpin = 720;
        const int finishBuildDegree = 540;
        //const int imgWidth = 512;
        //const int imgHeight = 384;
        const int imgWidth = 800;
        const int imgHeight = 600;
        const bool useAA = true;
        const int quality = 9;

        static void Main(string[] args)
        {
            List<int> itemIds = loadBluePrint();
            int frames = createPov(itemIds, true);
            createIni(frames);
        }

        private static List<int> loadBluePrint()
        {
            string infile = File.ReadAllText("../../input/original.blueprint");
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

        private static void createIni(int frames)
        {
            // Create INI
            string line;
            var sb = new StringBuilder();
            System.IO.StreamReader file = new System.IO.StreamReader("../../template.ini");
            while ((line = file.ReadLine()) != null)
            {
                if (line.Equals("Final_Frame="))
                    line += frameCount.ToString();
                if (line.Equals("Final_Clock="))
                    line += degreeSpin.ToString();
                if (line.Equals("Width="))
                    line += imgWidth.ToString();
                if (line.Equals("Height="))
                    line += imgHeight.ToString();
                if (line.Equals("Antialias="))
                    line += (useAA ? "Yes" : "No");
                if (line.Equals("Quality="))
                    line += quality.ToString();
                sb.AppendLine(line);
            }

            file.Close();
            using (System.IO.StreamWriter files = new System.IO.StreamWriter("../../output/run.ini"))
            {
                files.WriteLine(sb.ToString());
            }
        }

        private static int createPov(List<int> itemIds, bool useLDDorder)
        {
            // Create POV
            string line;
            int onItem = -1;
            int lastItem = -1;
            var sb = new StringBuilder();
            System.IO.StreamReader file = new System.IO.StreamReader("../../input/original.pov");
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
                    if (useLDDorder)
                        itemTiming = onItem - 1;
                    else
                    {
                        itemTiming = itemIds.FindIndex(a => a.Equals(onItem - 1));
                        if (itemTiming == -1)
                            break;
                    }
                    sb.AppendLine("#if(clock >= " + (itemTiming * ((double)finishBuildDegree/itemIds.Count)).ToString() + ")");
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
            using (System.IO.StreamWriter files = new System.IO.StreamWriter("../../output/run.pov"))
            {
                files.WriteLine(sb.ToString());
            }
            return lastItem;
        }
    }
}
