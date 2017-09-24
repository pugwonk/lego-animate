using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Animator
{
    class Options
    {
        [Option('i', "input", Required = true,
        HelpText = "Input POV file.")]
        public string inPov { get; set; }

        [Option('b', "blueprint", Required = false,
        HelpText = "Input BluePrint instruction file.")]
        public string inBluePrint { get; set; }

        [Option('w', "width", Required = false, DefaultValue = 320,
        HelpText = "Width of output images.")]
        public int imgWidth { get; set; }

        [Option('h', "height", Required = false, DefaultValue = 200,
        HelpText = "Height of output images.")]
        public int imgHeight { get; set; }

        [Option('f', "frames", Required = false, DefaultValue = 50,
        HelpText = "Number of frames to render.")]
        public int frameCount { get; set; }

        [Option('r', "rotate", Required = false, DefaultValue = 0,
        HelpText = "Number of degrees to rotate model during animation.")]
        public int degreeSpin { get; set; }

        [Option('e', "endbuildat", Required = false, DefaultValue = 0,
        HelpText = "Degrees of rotation at which to finish building.")]
        public int finishBuildDegree { get; set; }

        [Option('q', "quality", Required = false, DefaultValue = 9,
        HelpText = "POV-Ray quality setting to render at (1-9).")]
        public int quality { get; set; }

        [Option('a', "antialias", Required = false, DefaultValue = false,
        HelpText = "Whether to antialias resulting images.")]
        public bool useAA { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
