# CONVERT ATELIER XML

A command-line tool for extracting and reinserting .xml files from Atelier series games into .txt files

The aim is to translate games, which can be complicated via .xml files.

Building
========

In console, just type `dotnet build` (Powershell, VSC...)

Usage
=====

In console, type :

`ConvertXML.exe filename.xml` for extract ; A .txt file will be created next to it

`ConvertXML.exe filename.txt` for insert ; The .xml file must be present in the same folder as a NEW.xml file will be created

If a "saves" folder containing the sub-folder and the .xml file in question exists in the same folder, the .xml file will also be replaced! All you need to do is recompile it to .e (Via Gust Tools).

Example :

`ConvertXML.exe strcombineall.xml` from Atelier Ryza 3 will create a strcombineall.txt with all texts that need translating (some empty lines or lines with no dialogue to translate are ignored)

`ConvertXML.exe strcombineall.txt` will reinsert the strcombineall.txt file into strcombineall.xml, and save it as strcombineall NEW.xml (the original file is preserved).

If a "saves" folder is present in the executable folder, and it contains one or more subfolders containing the file (and its folders), the .xml file will be replaced!

So if you insert "translate/rumor/rumor.txt" into rumor.xml (from Atelier Sophie), and "saves/rumor/rumor/xml" is where the executable is, the file will be copied!

Highly recommended :

Create an "XML" subfolder containing all your .xml files to be translated, and run extractAllXML.bat to extract all your .xml files at once!

Translate them, then run insertAllXML.bat, and your entire translation will be inserted!

License
=======

[GPLv3](https://www.gnu.org/licenses/gpl-3.0.html) or later.

Thanks
======

* VitaSmith for his [Gust Tools](https://github.com/VitaSmith/gust_tools) that make it extremely easy to modify Atelier games!
