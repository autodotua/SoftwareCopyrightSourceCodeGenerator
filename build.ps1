try {
    $c = $true
    $s = $true
    if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
        throw "未安装.NET SDK"
    }

    $publishDirectory = "Publish"
    if  (Test-Path $publishDirectory -PathType Container) {
        Remove-Item -r $publishDirectory 
    }

    New-Item -Path $publishDirectory -ItemType Directory | Out-Null
    
    Clear-Host

    Write-Output "正在发布win-x64"
    dotnet publish SoftwareCopyrightSourceCodeGenerator.Desktop -r win-x64 -c Release -o Publish/win-x64 --self-contained $c /p:PublishSingleFile=$s 
    Move-Item Publish/win-x64/SoftwareCopyrightSourceCodeGenerator.Desktop.exe Publish/win-x64/scscg.exe

    Write-Output "正在发布linux-x64"
    dotnet publish SoftwareCopyrightSourceCodeGenerator.Desktop -r linux-x64 -c Release -o Publish/linux-x64 --self-contained $c /p:PublishSingleFile=$s 
    Move-Item Publish/linux-x64/SoftwareCopyrightSourceCodeGenerator.Desktop Publish/linux-x64/scscg.exe
    # if ($b) {
    #     Write-Output "正在发布browser-wasm"
    #     dotnet publish MyDiary.UI/MyDiary.UI.Browser -r browser-wasm -c Release -o Publish/web --self-contained true
    #     Copy-Item -Recurse $Env:TEMP/MyDiary_Release\MyDiary.UI.Browser\net8.0\browser-wasm/AppBundle Publish/web
    # }
   
    Write-Output "操作完成"

    Invoke-Item Publish
    pause
}
catch {
    Write-Error $_
}