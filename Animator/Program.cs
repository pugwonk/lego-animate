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
            List<int> itemIds;
            if (options.inBluePrint != null)
                itemIds = loadBluePrintSteps(options);
            else
                itemIds = loadPovSteps(options);
            createPov(itemIds, options);
            createIni(options);
            deleteOldImages(options.inPov);
            Console.WriteLine();
            Console.WriteLine("Done!");
            Console.WriteLine("To run the animation, open '" + options.inPov.ToLower().Replace(".pov", "-anim.ini") + "' in POV-Ray. Don't forget to leave LDD to POV-Ray open.");
        }

        private static void deleteOldImages(string inPov)
        {
            String povPath = Path.GetFullPath(inPov);
            string povName = Path.GetFileName(povPath);
            string povFolder = Path.GetDirectoryName(povPath);
            var dir = new DirectoryInfo(povFolder);
            string filesToGo = povName.ToLower().Replace(".pov", "-anim*.png");
            var oldImages = dir.EnumerateFiles(filesToGo);
            if (oldImages.Count() > 0)
            {
                Console.WriteLine("Deleting old output files " + filesToGo);
                foreach (var file in oldImages)
                {
                    file.Delete();
                }
            }
        }

        private static List<int> loadBluePrintSteps(Options options)
        {
            Console.WriteLine("Loading steps from BluePrint " + options.inBluePrint);
            string infile = File.ReadAllText(options.inBluePrint);
            JObject d = JObject.Parse(infile);
            List<int> ids = new List<int>();

            doBluePrintSteps(d["model"]["steps"], ids);
            return ids;
        }

        private static List<int> loadPovSteps(Options options)
        {
            Console.WriteLine("Loading steps from POV");
            throw new NotImplementedException();
        }

        private static void doBluePrintSteps(JToken steps, List<int> ids)
        {
            foreach (JToken step in steps)
            {
                foreach (var part in step["parts"])
                {
                    string pid = (string)part.SelectToken("id");
                    ids.Add(int.Parse(pid));
                }
                foreach (var subm in step["submodels"])
                {
                    doBluePrintSteps(subm["steps"], ids);
                }
            }
        }

        private static void createIni(Options options)
        {
            string fileName = options.inPov.ToLower().Replace(".pov", "-anim.ini");
            Console.WriteLine("Creating INI " + fileName);
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
                if (line.Equals("Input_File_Name="))
                    line += options.inPov.ToLower().Replace(".pov", "-anim.pov");
                sb.AppendLine(line);
            }

            file.Close();
            using (System.IO.StreamWriter files = new System.IO.StreamWriter(fileName))
            {
                files.WriteLine(sb.ToString());
            }
        }

        private static void createPov(List<int> itemIds, Options options)
        {
            string fileName = options.inPov.ToLower().Replace(".pov", "-anim.pov");
            Console.WriteLine("Creating POV " + fileName);
            string line;
            int onItem = -1;
            int dumpUnions = 0;
            bool ignoreLine;
            var sb = new StringBuilder();
            System.IO.StreamReader file = new System.IO.StreamReader(options.inPov);
            while ((line = file.ReadLine()) != null)
            {
                if (line.Equals("#declare ldd_model_transformation = transform { translate <0,0,0> }"))
                    line = "#declare ldd_model_transformation = transform { translate <2,1,1> }";
                if (line.Equals("#declare ldd_model = union {"))
                    onItem = 0;
                ignoreLine = false;
                if (line.Trim().Equals("union {"))
                { // it's an unnecessary union that LDD to POV-Ray made
                    dumpUnions++;
                    ignoreLine = true;
                }
                if (line.Equals("}"))
                {
                    if (onItem > -1)
                    {
                        if (dumpUnions > 0)
                        {
                            dumpUnions--;
                            ignoreLine = true;
                        }
                        else
                        {
                            if (options.degreeSpin != 0)
                                sb.AppendLine("rotate <0,clock,0>");
                            onItem = -1;
                        }
                    }
                }
                if (!ignoreLine)
                {
                    if (onItem > 0)
                    {
                        // Find where this item appears in the ordered list
                        int itemTiming = itemIds.FindIndex(a => a.Equals(onItem - 1));
                        if (itemTiming == -1)
                            break;
                        sb.AppendLine("// Item " + onItem.ToString() + ", index " + itemTiming.ToString());
                        if (options.degreeSpin != 0)
                            sb.AppendLine("#if(clock >= " + (itemTiming * ((double)options.finishBuildDegree / itemIds.Count)).ToString() + ")");
                        sb.AppendLine(line);
                        sb.AppendLine("#end");
                    }
                    else
                    {
                        sb.AppendLine(line);
                    }
                    if (onItem != -1)
                        onItem++;
                }
            }

            file.Close();
            using (System.IO.StreamWriter files = new System.IO.StreamWriter(fileName))
            {
                files.WriteLine(sb.ToString());
            }
        }
    }
}
