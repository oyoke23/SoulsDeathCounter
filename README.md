# Souls Death Counter

Death counter for Souls games - useful for Twitch/OBS streaming.

## Supported Games

- Dark Souls: Prepare To Die Edition
- Dark Souls Remastered
- Dark Souls II
- Dark Souls II: Scholar of the First Sin
- Dark Souls III
- Sekiro: Shadows Die Twice
- Elden Ring

## Download

Go to [Releases](../../releases) and download the latest `SoulsDeathCounter.exe`.

## How to Use

1. Run `SoulsDeathCounter.exe` as **Administrator** (right-click > Run as administrator)
2. Open your game and load your save
3. The counter will automatically detect the game and display your deaths

## OBS/StreamLabs Setup

### Option 1 - Window Capture (recommended)

1. Add a "Window Capture" source
2. Select "SoulsDeathCounter.exe"
3. Add a "Color Key" filter
4. Set color to `#404040` (RGB 64, 64, 64)
5. Adjust similarity until background disappears

### Option 2 - Text File

1. The program creates a `deaths.txt` file in the same folder
2. In OBS, add a "Text (GDI+)" source
3. Check "Read from file"
4. Select the `deaths.txt` file

## Customization

Edit `settings.txt` (created automatically):

```
Font_Color: #FFFFFF
Font_Type: Arial
Font_Size: 72
Font_Style: 1
Background_Color: #404040
```

Font styles: 0=Regular, 1=Bold, 2=Italic, 3=Bold+Italic

## Requirements

- Windows 10/11 (64-bit)
- Run as Administrator (required to read game memory)

## Notes

- This app only READS memory, it does NOT modify anything
- Will not be detected by anti-cheat systems
- Death count is saved automatically
