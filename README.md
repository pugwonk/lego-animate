# Introduction

This program takes files created by the LDD to POV-Ray and converts them into build animations for rendering in POV-Ray. First things first - this takes a long time to run (it could be weeks). It only works on Windows and will require the following:

* [Lego Digital Designer](http://ldd.lego.com/en-us/)
* An LXF file from LDD containing the model you want to render
* the [LDD to POV-Ray Converter](http://ldd2povray.lddtools.com/) (remember to reboot after installing this)
* This program (download the executable here XXXX)
* (Optional) [BluePrint](https://www.eurobricks.com/forum/index.php?/forums/topic/108346-software-blueprint-a-building-instruction-generator-for-ldd/)
* Some way of turning many picture files into a video (I used [VirtualDub](http://virtualdub.org/))

If you are an impatient sort, you can jump straight to the [Running It](#running-it) section. But, I warn you, you're going to have things go wrong that I'm about to describe in earlier sections and after a week or two of your computer rendering the thing you're going to discover it doesn't look right.

# Build Order

There are two ways to pick the build order. Probably the best one (and my original idea) is to generate some instructions with BluePrint, and draw the pieces in the order suggested by BluePrint. However, while messing around with this I discovered that LDD actually orders the parts as you draw them in the designer. So we can just steal that order. It's not perfect - if you delete a piece and add another one, the new one gets plopped in the order position of the original, so it can go out of sync easily.

In the examples below I'm not using a MOC of mine - these are renders of the very nice [Joe's Cantina](https://rebrickable.com/mocs/MOC-2618/Berth/joes-cantina) by Bertha.

Here's an example of a build order built in BlueRender:
And here's the same model with the original LDD build order:

You can see that there are some artifacts in the wrong place when using the LDD order - as these were the first pieces placed, it was easy to track them down and just correct the order.

Here's that model with some pieces manually fudged to be later in the order:
And finally here's that with rotation turned on:

As you can see, using the LDD order is a bit more attractive but won't work well for models that have been tweaked a lot in LDD.

# Rotation

My converter can ask POV-Ray to rotate the model while it's being built. This is done via the r and fb parameters (see #command line parameters).

# Running It

This is my recommend sequence of steps.

1. Install everything listed in the [Introduction](#introduction)
1. Run Windows Update on your computer. Because it's going to be on for a long time, and you don't want it forcing a reboot. Ask me how I know
1. Open your LXF file in LDD and put it in the middle of the screen (moving the model doesn't mark the file as needing saved so you may have to add a brick and remove it)
1. Save the model and close LDD
1. Load the LDD to POV-Ray converter and create a POV file from it. I'd recommend setting the level of detail on the "Model" screen to the second setting ("original LDD geometry + visible bevels") because rendering the "Lego" logos on all the bricks is very time consuming
1. Do not close the LDD to POV-Ray converter. It has generated binary files that POV-Ray will need and needs to be kept running
1. Prepare a very low-resolution, low-quality 200x100 animation in 50 frames by running `animate -i myfile.pov -q1 -w200 -h100 -f50`
1. Open POV-Ray and load myfile-anim.ini. Hit "Run"
   1. Make a pot of tea
1. Flip through the various resulting PNG files. The renders will look horrible but don't worry - does the build order look decent?
1. If it doesn't:
   1. Run BluePrint
   1. Import your LXF file
   1. Save the resulting instructions
   1. Prepare a very low-resolution, low-quality 200x100 animation in 50 frames by running `animate -i myfile.pov -q1 -w200 -h100 -f50 instructions.blueprint`
   1. Render `myfile-anim.ini` in POV-Ray
   1. Does the build order look decent? If so, keep adding the blueprint file on the command line. If not, remove it (you may want to look at [Manually Editing Build Order](#manually-editing-build-order))
1. If you do not want to rotate the model during animation, generate a lovely high-quality 100-frame render by running `animate -i myfile.pov -q1 -aa -w800 -h600 -f100 instructions.blueprint`
   1. Make an entire jug of tea
1. If you do want to rotate the model, generate a low-quality animation by running `animate myfile.pov -q1 -w200 -h100 -f50 instructions.blueprint -r720 -fb540`. This will rotate the model by 720 degrees during the build, and finish the actual building at 540 degrees
1. Render this in POV-Ray. If the model twirls in an odd way on its axis, move it around a bit in LDD and go back to step ???
1. If this looks good, generate a high quality 250 frame render by using something like `animate myfile.pov -q1 -w800 -h600 -f250 -r720 -aa -fb540 instructions.blueprint`
1. Render this in POV-Ray
   1. Make tea for your entire neighbourhood and distribute it
1. Once you're happy with the PNG files, combine them into a video. To do this using VirtualDub:
   1. Download the XVID video codec, which seems to work best
   1. Open VirtualDub
   1. Select File..Open Video File and choose the first frame of your render
   1. Select Video..Frame Rate and select a frame rate that will work (10fps isn't too bad - 25fps looks lovely but bear in mind your 250 frame video will last 10 seconds)
   1. Select Video..Compression and select Xvid MPEG-4 Codec
   1. Hit File..Save as AVI
1. Impress your friends
1. To make a truly gorgeous 4k 1000-frame version of your render, try `animate myfile.pov -q1 -w3840 -h2160 -f1000 -r720 -aa -fb540`
   1. Buy a tea plantation and start growing tea plants on it

# Command Line Options

# Manually Editing Build Order

# Things That Might Go Wrong

* If POV-Ray falls over on some sort of #include in the run.pov file, it's probably because you're not runing LDD to POV-Ray, or you haven't generated this exact model with it beforehand.
