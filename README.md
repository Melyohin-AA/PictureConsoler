<div id="top"></div>


<br />
<div align="center">
  <h2 align="center">Picture Consoler</h2>
  <p align="center">Вывод изображений в консоль Windows</p>
</div>

<div align="center">
  <img src="https://github.com/Melyohin-AA/PictureConsoler/raw/master/_ReadmeFiles/main.png" alt="main">
</div>

### О проекте

Данный проект представляет собой приложение, позволяющее выводить изображения в консоль Windows в различных вариациях.

Особенности продукта:
* Поддержка различных форматов исходного изображения (BMP, PNG, JPEG, TIFF, GIF)
* Поддержка различных растровых шрифтов (4x6, 16x8, 6x9, 8x9, 5x12, 7x12, 8x12, 16x12, 12x16, 10x18)
* Воспроизведение GIF-анимаций
* Дополнительные фильтры для исходных изображений (оператор Собеля, выделение контуров)
* Различные опции цветовой палитры (стандартная фиксированная палитра, собственная палитра для каждого кадра, собственная палитра для всего изображения)
* Запись обработанных для отображения в консоли изображений в формате PCUF
* Чтение PCUF и устаревших форматов PCFF и PCXF
* Возможность конвертации из PCUF в PNG или GIF
* Возможность указать путь до исходного изображения посредством аргументов командной строки
* Целевая ОС - Windows 10

Разработка велась на языке программирования `C#` с использованием `.NET Framework 4.7.2`. Среда разработки - `Microsoft Visual Studio 2017`. Тип приложения - `Console Application`.

![product-screenshot](product-screenshot.png)


### Использование

1. С помощью Visual Studio выполнить сборку проекта `PictureConsoler` (*ИЛИ* скачать [готовую сборку](https://drive.google.com/file/d/1kOY1syEP82-f2W1pyNbwSb1MWq62hlPd/view?usp=sharing))
2. Запустить `PictureConsoler.exe`
3. Указать путь до исходного файла
4. Выбрать желаемые опции обработки/отображения/вопроизведения/записи изображения
5. Подождать, пока происходит отрисовка изображения

Примечания:
1. Необходимо в настройках `Command Prompt` в качестве шрифта выбрать `Raster Fonts`
2. Рекомендуется в настройках `Command Prompt` включить опцию `Use legacy console`
3. После загрузки и обработки с помощью фильтров исходное изображение разбивается на прямоугольные фрагменты с шириной, равной ширине шрифта, и высотой, равной половине высоты шрифта (для отображения используется символ половины клетки)
4. При выборе размера шрифта можно указать произвольный размер в форме "{номер шрифта}~{ширина}~{половина высоты}"
5. Использование режимов цветовой палитры PCX или PCM подразумевает вычисление подходящих цветов, что, в зависимости от количества различных цветов на изображении после фрагментирования и настроек обработки, может занимать от нескольких секунд до нескольких десятков минут
6. При воспроизведении GIF действуют следующие управляющие клавиши:
  * `Space` - переход к следующему кадру (вне режима автовоспроизведения)
  * `Tab` - переключение автовоспроизведения
  * `R` - переход к первому кадру
  * `Arrows` - изменение инервала воспроизведения (влево/вправо - на 1мс, вниз/вверх - в 2 раза)
7. Изменение цветовой палитры требует дополнительных временных затрат, поэтому при автовоспроизведении GIF рекомендуется использовать режим палитры PCM, вместо PCX
