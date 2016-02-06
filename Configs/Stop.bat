taskkill /IM WireCloudTerminal.exe
taskkill /IM ConectionController.exe
taskkill /IM NetworkNode.exe
taskkill /IM NetworkClientNode.exe /f
taskkill /IM LRM.exe
taskkill /IM RoutingController.exe
taskkill /IM Policy.exe
taskkill /IM Directory.exe
taskkill /IM NetworkCallController.exe
timeout /t 3
taskkill /IM cmd.exe