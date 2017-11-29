:start
powershell -Command Get-Content -Path $env:USERPROFILE\AppData\LocalLow\DefaultCompany\UnityLowLevelMultiplayerExample\output_log.txt -Wait
goto start