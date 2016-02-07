cd WireCloud
start cmd /c cloudStart.bat


cd ..
cd RoutingController
::start cmd /c routingControllerStart.bat

cd ..
cd Directory
start cmd /c startDirectory0.bat

cd ..
cd Policy
start cmd /c startPolicy0.bat

cd ..
cd NCC
start cmd /c startNCC0.bat

cd ..
cd LRM
::start cmd /c startLRM0.bat

cd ..
cd CC
start cmd /c startConnectionController0.bat

cd ..
cd NetworkNode
start cmd /c startNode0.bat

::start cmd /c startNode1.bat

::start cmd /c startNode2.bat

::start cmd /c startNode3.bat

cd ..
cd NetworkClient
start cmd /c startClient1.bat
start cmd /c startClient2.bat
exit
