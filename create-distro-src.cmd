set VERSION=1.1
set DISTRO_DIR=Pauthor-Source-v%VERSION%
set BUILD=c:\windows\microsoft.net\framework\v3.5\msbuild.exe

rmdir /s/q %DISTRO_DIR%
mkdir %DISTRO_DIR%\bin

%BUILD% Source\Pauthor.sln /t:Rebuild /p:Configuration=Release

cp bin\AniseLib.dll %DISTRO_DIR%\bin
cp bin\DeepZoomTools.dll %DISTRO_DIR%\bin
cp bin\PauthorLib.dll %DISTRO_DIR%\bin
cp bin\Pauthor.exe %DISTRO_DIR%\bin
cp LICENSE.txt %DISTRO_DIR%
cp README-src.txt %DISTRO_DIR%\README.txt
cp "API Reference.chm" %DISTRO_DIR%
xcopy /i/s "Sample Collection" %DISTRO_DIR%\"Sample Collection"
xcopy /i/s/exclude:create-distro-excludes.txt Source %DISTRO_DIR%\Source
xcopy /i/s/exclude:create-distro-excludes.txt "Sample Project - RSS Crawler" %DISTRO_DIR%\"Sample Project - RSS Crawler"

echo Press enter to finish
readline
