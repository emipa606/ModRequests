@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

echo �� �ணࠬ�� ��� � ������ .png 䠩��� � ����� � ���ன �ᯮ������ � � �� ��������� ��ࢮ� ᫮�� � ������� ��� �� ��஥ ᫮��. ����� ࠡ�⠥� � �⤥��묨 �㪢���, �� ����ࠬ�, ��ࠬ� � �.�. �� ���⢨⥫쭠 � ॣ����� � �� ࠡ�⠥� � �஡����� (� _ ࠡ�⠥�).

:begin
SET /P old=��ࢮ� ᫮��:
SET /P new=��஥ ᫮��:
if /I "%old%"  NEQ  ""    if /I "%new%" NEQ  ""   if /I "%old%" NEQ "%new%" goto :rename
goto :begin

:rename
for /R %%# in (".\*.png") do (
  SET "File=%%~nx#"
  
  if "!File:%new%=%old%!" NEQ "!File:%old%=%new%!" (
    REN "%%#" "!File:%old%=%new%!"
    echo ��२��������� !File:%new%=%old%! � !File:%old%=%new%!
  )
)
echo ��⮢�.
goto :begin