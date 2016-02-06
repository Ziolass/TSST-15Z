cd WireCloud
start cmd /c cloudStart.bat

cd ..
cd Directory
start cmd /c startDirectory0.bat
start cmd /c startDirectory1.bat

cd ..
cd Policy
start cmd /c startPolicy0.bat
start cmd /c startPolicy1.bat

cd ..
cd NCC
start cmd /c startNCC0.bat
start cmd /c startNCC1.bat


cd ..
cd CC
start cmd /c startConnectionController1.bat
start cmd /c startConnectionController0.bat



cd ..
cd LRM
goto comment
start cmd /c startLRM0.bat
:comment
start cmd /c startLRM1.bat

cd ..
cd NetworkNode
start cmd /c startNodes_domainA.bat
start cmd /c startNodes_domainB.bat

cd ..
cd NetworkClient
start cmd /c startClient1.bat
start cmd /c startClient2.bat
start cmd /c startClient3.bat
exit
