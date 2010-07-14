set VERSION=1.2
set BUILD=c:\windows\microsoft.net\framework\v3.5\msbuild.exe

rem ############################################################################
rem ## Build & Test

%BUILD% Pauthor.sln /t:Rebuild /p:Configuration=Release

cd Test\PauthorTestRunner\bin\Debug
PauthorTestRunner.exe
if ERRORLEVEL 1 goto END
cd ..\..\..\..\

rem ############################################################################
rem ## Create Binary Distribution

set DISTRO_DIR=Pauthor-v%VERSION%

rmdir /s/q %DISTRO_DIR%
mkdir %DISTRO_DIR%\bin

cp bin\AniseLib.dll %DISTRO_DIR%\bin
cp bin\DeepZoomTools.dll %DISTRO_DIR%\bin
cp bin\PauthorLib.dll %DISTRO_DIR%\bin
cp bin\Pauthor.exe %DISTRO_DIR%\bin
cp LICENSE.txt %DISTRO_DIR%
cp README-bin.txt %DISTRO_DIR%\README.txt

rem ############################################################################
rem ## Create Source Only Distribution

set DISTRO_DIR=Pauthor-Source-v%VERSION%

rmdir /s/q %DISTRO_DIR%
mkdir %DISTRO_DIR%\bin

cp bin\AniseLib.dll %DISTRO_DIR%\bin
cp bin\DeepZoomTools.dll %DISTRO_DIR%\bin
cp bin\PauthorLib.dll %DISTRO_DIR%\bin
cp bin\Pauthor.exe %DISTRO_DIR%\bin
cp LICENSE.txt %DISTRO_DIR%
cp README-src.txt %DISTRO_DIR%\README.txt
cp "API Reference.chm" %DISTRO_DIR%
cp Pauthor-no-tests.sln %DISTRO_DIR%\Pauthor.sln
xcopy /i/s/exclude:create-distro-excludes.txt Source %DISTRO_DIR%\Source
xcopy /i/s/exclude:create-distro-excludes.txt "Sample Project - RSS Crawler" %DISTRO_DIR%\"Sample Project - RSS Crawler"

rem ############################################################################
rem ## Create Source & Test Distribution

set DISTRO_DIR=Pauthor-Source-Test-v%VERSION%

rmdir /s/q %DISTRO_DIR%
mkdir %DISTRO_DIR%\bin

cp bin\AniseLib.dll %DISTRO_DIR%\bin
cp bin\DeepZoomTools.dll %DISTRO_DIR%\bin
cp bin\PauthorLib.dll %DISTRO_DIR%\bin
cp bin\Pauthor.exe %DISTRO_DIR%\bin
cp LICENSE.txt %DISTRO_DIR%
cp README-src.txt %DISTRO_DIR%\README.txt
cp "API Reference.chm" %DISTRO_DIR%
cp Pauthor.sln %DISTRO_DIR%
xcopy /i/s/exclude:create-distro-excludes.txt Source %DISTRO_DIR%\Source
xcopy /i/s/exclude:create-distro-excludes.txt Test %DISTRO_DIR%\Test
xcopy /i/s/exclude:create-distro-excludes.txt "Sample Project - RSS Crawler" %DISTRO_DIR%\"Sample Project - RSS Crawler"

rem ###########################################################################
rem ## Create Sample Collection Distribution

set DISTRO_DIR=Pauthor-Sample-Collection

rmdir /s/q %DISTRO_DIR%
mkdir %DISTRO_DIR%

xcopy /i/s "Sample Collection" "%DISTRO_DIR%"

:END

