@echo off
setlocal enabledelayedexpansion

:: Path to ImageMagick (assuming 'magick' is available in PATH)
set IM=magick

:: Loop through common image formats
for %%F in (*.jpg *.jpeg *.png *.bmp *.gif) do (
    for /f "tokens=1,2 delims= " %%A in ('%IM% identify -format "%%w %%h" "%%F"') do (
        set W=%%A
        set H=%%B

        if !W! GTR 512 if !H! GTR 512 (
            echo Resizing %%F (Original: !W!x!H!) to 500x500
            %IM% "%%F" -resize 500x500^> "resized_%%F"
        )
    )
)

echo Done.
pause
