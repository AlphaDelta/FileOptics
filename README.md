# Overview
FileOptics is a file structure viewer designed to create a tree of the entire contents of a file, with sections of data being either critical (critical to the primary purpose of the file), ancillary (providing some constructive value to the primary purpose of the file), metadata (provides no purpose other than information), and useless (contains no determinable purpose).

# Modules
FileOptics works by taking in DLL files and searching for methods that implement the `IModule` interface in `FileOptics.Interface`.

The `IModule` interface exposes the methods `CanRead` and `Read` so that the primary program may iterate through a large list of classes that implement the interface to find a class that can read an inputted file. Each class that implements `IModule` is designed to read and parse a specific type of file, for example `FileOptics.Basic.PNG` is designed to parse PNG image files.

Compiled DLLs are to be placed in the module folder. When the main program is run FileOptics iterates through each DLL in the module folder, hashes it in SHA256 written by Nayuki (https://www.nayuki.io/page/fast-sha2-hashes-in-x86-assembly), and compares it to a list of trusted checksums (`modules/trusted`) before reading and creating instances.

# FileOptics.Basic
The `FileOptics.Basic` project provides classes to read the following files

* PNG image files (https://www.w3.org/TR/PNG/)
* JPEG image files (JFIF and EXIF)
* Windows Thumbcache files