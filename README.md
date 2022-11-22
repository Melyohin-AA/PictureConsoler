<div align="center">
  <p align="right">v[1.102.4.5]</p>
  <h2 align="center">Picture Consoler</h2>
  <p align="center">Display pictures in the Windows console</p>
</div>

<div align="center">
  <img src="https://github.com/Melyohin-AA/PictureConsoler/raw/master/_ReadmeFiles/main.png" alt="main">
</div>


### About the project

The project is an application designed to output pictures in different variants to the Windows console.

##### Features:
* Support of different formats of source image (BMP, PNG, JPEG, TIFF, GIF)
* Support of different raster fonts (4x6, 16x8, 6x9, 8x9, 5x12, 7x12, 8x12, 16x12, 12x16, 10x18)
* Replaying GIF-animations
* Additional processing of source images (Sobel operator, outline highlighting)
* Different palette options (standard fixed palette, special palette for each frame, special palette for whole image)
* Writing processed images in PCUF files
* Reading files in PCUF format and obsolete PCFF and PCXF formats
* Convertion files from PCUF format to PNG or GIF format
* option to specify the source file path with command-line arguments
* Target OS - Windows 10

##### Technology stack:
* `C#`
* `.NET Framework 4.7.2`
* `Console Application`


### Usage

1. Compile build of the `SensorMonitor` project via Visual Studio compiler (*OR* download [configured build](https://drive.google.com/file/d/1kOY1syEP82-f2W1pyNbwSb1MWq62hlPd/view?usp=sharing))
2. Run `PictureConsoler.exe`
3. Specify source file path
4. Select options of image processing/displaying/replaying/writing
5. Wait while the image is being rendered

##### Notice:
1. It is necessary to choose `Raster Fonts` as a font in the Command Prompt settings
2. Using of the `Use legacy console` option in the Command Prompt settings is recommended
3. After source image is loaded and processed it is divided into rectangular pieces which width is equal to the font's one, and which height is equal to a half of font's one (half cell symbol is used for image displaying)
4. While specifying font, it is possible to specify any font size in the `<font index>~<width>~<height half>` form
5. Using of PCX or PCM palette modes supposes the calculation of appropriate color set; this process may take from several seconds to several dozens of minutes what depends on a number of differnent colors after image was divided into pieces and on specified processing settings
6. While replaying a GIF, such controls are active:
  * `Space` - jump to the next frame (out of autoplay mode)
  * `Tab` - toggle autoplay mode
  * `R` - jump to the first frame
  * `Arrows` - varying of an autoplay interval (left/right - dec/inc by 1ms, down/up - dec/inc in 2 times)
7. Palette switching requires extra time, so it is recommended to use PCM mode instead of PCX for GIF autoplaying

### Demo

<a href="https://drive.google.com/drive/folders/14Gxi2ahGzbZTBQqTF1I1Mo_9dbcLqTxh">Picture gallery:</a><br/>
[![Demo](https://github.com/Melyohin-AA/PictureConsoler/raw/master/_ReadmeFiles/product-screenshot.png)](https://drive.google.com/drive/folders/14Gxi2ahGzbZTBQqTF1I1Mo_9dbcLqTxh)
