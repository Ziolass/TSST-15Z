cd WireCloud
start cmd /c cloudStart.bat
cd ..
cd NetworkClient
start cmd /c startClient0-A.bat
cd ..
cd NetworkNode
start cmd /c startNode0.bat
start cmd /c startNode1.bat
exit
