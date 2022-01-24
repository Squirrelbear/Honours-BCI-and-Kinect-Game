# Honours BCI and Kinect Game

The code provided in this repository was what I used for testing with participants during my Honours year in 2012. It is provided as-is and has not been cleaned up to make it easier for other developers. The concepts presented may still be useful to some people, and you can find out more details about the work by reviewing the Thesis.pdf or ExpoPoster.pdf. 

The MatlabServer folder contains a copy of the code used for passing BCI information to my server code. I did not write the MatlabServer code it is provided as is in case anyone is interested in replicating the input.

- ([Youtube Demo](https://youtu.be/CxklsQ8ch8A))

The paper from this work in iTagPaper.pdf was presented at a 2013 iTag coference. The slides were shared for all presentations at the conference via slideshare. My supervisor went to the conference to present papers on behalf of multiple peers in Nottingham, United Kingdom in October 2013. ([Presentation Slides](https://www.slideshare.net/iTAG_conf/evaluation-of-a-natural-user-interaction-gameplay-system-using-the-microsoft-kinect-augmented-with-noninvasive-brain-computer-interfaces))

# Required SDKs/Runtimes

You may need some or all of these to run/compile the provided files:
* KinectSDK v1.0
* Microsoft Speech Platform SDK
* MS Kinect Lang Pack enUS
* Speech Platform Runtime
* XNA Game Studio 4.0

# Controls

If there is a Kinect connected the following will be enabled:
Hand icon will use your right hand by default to start with. (this can be changed by voice command)

Voice commands include:
* "rotate tile left", <- rotates a currently selected tile 90 degrees to the left
* "rotate tile right", <- rotates a currently selected tile 90 degrees to the right
* "drop tile", <- drops a currently selected tile into an empty zone or swaps with an existing other tile
* "place tile", <- same as above
* "change hands", <- toggles which hand is used for the input for selection and manipulation
* "swap hands", <- same as above
* "select tile", <- selects a tile when no other is currently selected
* "grab tile"  <- same as above

Status text will appear in the top left whether or not the kinect is attached.
It provides details on the FPS, alpha state, kinect status, and feedback from the voice and other commands

BCI input works as follows:

Default state uses gamepad input only. 
* Hold right trigger down to give a gradated input
* Press A to disable/enable gamepad input (this also means you can lock the gradated trigger input for a period of time)


If there is no gamepad connected or the A button on the pad has been pressed the keyboard controls will be automatically used.
* Plus = increase alpha level by 0.1
* Minus = decrease alpha level by 0.1
* Home = reset alpha to 0
* Insert = set alpha to -1
* End = set alpha to 1

If the kinect is NOT connected the following will automatically activate:
* Mouse input will be used instead of hands
* Speech input uses keyboard input of:
* 1 = "rotate tile left"
* 2 = "rotate tile right"
* 3 = "drop tile"
* 4 = "change hands" (redundant when Kinect not connected)
* 5 = "select tile"
