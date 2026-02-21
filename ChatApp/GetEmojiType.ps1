[System.Reflection.Assembly]::LoadFrom('C:\Users\Chris\.nuget\packages\microsoft.fluentui.aspnetcore.components\4.14.0\lib\net8.0\Microsoft.FluentUI.AspNetCore.Components.dll') | Out-Null
[System.Reflection.Assembly]::LoadFrom('C:\Users\Chris\.nuget\packages\microsoft.fluentui.aspnetcore.components.emoji\4.14.0\lib\net8.0\Microsoft.FluentUI.AspNetCore.Components.Emoji.dll') | Out-Null
$asm = [System.Reflection.Assembly]::LoadFrom('C:\Users\Chris\.nuget\packages\microsoft.fluentui.aspnetcore.components.emoji\4.14.0\lib\net8.0\Microsoft.FluentUI.AspNetCore.Components.Emojis.SmileysEmotion.dll')
$asm.GetExportedTypes() | Select-Object -First 30 FullName
