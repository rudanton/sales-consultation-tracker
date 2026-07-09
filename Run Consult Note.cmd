@echo off
title Consult Note

cd /d "%~dp0"

echo.
echo ==========================================
echo          Consult Note
echo ==========================================
echo.

dotnet run --project src\ConsultNote\ConsultNote.csproj

echo.
echo ==========================================
echo      Consult Note has exited.
echo ==========================================
echo.
pause