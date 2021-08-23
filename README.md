# LomontSharp

Chris Lomont's library of commonly used items

## Introduction

This is to make my commonly used pieces of code easier for me to reuse.

## Included



### LomontSharp

Main piece should be as portable as possible, currently .NET 5.0, hopefully to 6.0 soon so I can use INumeric to make nicer math pieces.

Base classes. Includes:

- **Algorithms**: Aho Corasick and Boyer Moore string matching, Combinatorics, Dancing Links (DLX), Shuffle, Resevoir sampling, stable sorting, more.
- **Containers**: Shuffle bag, Fenwick tree, Rangeset. Many more coming # priority queue, stable priority queue
- **Formats**: PLY, hex dump, tree formatting, # WAV, DXF, 
- **Geometry**: coming soon. I have decades of code to merge and cleanup. # mesh, half edge mesh, mesh tools
- **Graphics**: color spaces galore: RGB, sRGB, HSL, HSV, CIELAB, CIEXYZ, YIQ, Sepia, Grayscale
- **Information**: various CRCs, # Bytestream, bitstream
- **Numerical**: 2d/3d vector and matrix stuff, random number generation
- **Stats**: Sequence, Histogram, Tally, correlation grid, # gather, histogram, basic stats
- **Utility**: colored test formatting for multiple uses, Colored trace listener, functor based trace listener, lots of reflection helpers, ability to reflect on XML documentation.

### LomontWin

Windows specific stuff. Split off since compiling and needs are different.

* Process capture - capturing console processes into .NET for consumption.
* (#) RunDll, process capture

### LomontWPF

WPF specific stuff. Split off since compiling and needs are different.

* Converters
* Clipboard helper
* Much more coming soon: zoom and pan, image stuff, 3d stuff, arcball 3d controls, etc.

### TestLomontSharp

Testing for all the above.



## TODO

Much more listed in the code TODO section. I have decades of code to pull pieces from and clean them up and generalize them.



## History

Release 0.2 - 8/23/2021 - Added things marked (#)

Release 0.1 - 8/19/2021 - Initial release