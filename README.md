# LomontSharp

Chris Lomont's library of commonly used items

## Introduction

This is to make my commonly used pieces of code easier for me to reuse.

## Included



### LomontSharp

Main piece should be as portable as possible, currently .NET 5.0, hopefully to 6.0 soon so I can use INumeric to make nicer math pieces. (Update: Now 7.0, uses INumeric)

Base classes. Includes:

- **Algorithms**: Aho-Corasick and Boyer Moore string matching, Combinatorics, Dancing Links (DLX), Shuffle, Reservoir sampling, stable sorting, shuffle, permutations, matching problems (Hungarian Algorithm).
- **Containers**: Shuffle bag, Fenwick tree, Rangeset. Many more coming # priority queue, stable priority queue, kd-tree, 
- **Formats**: PLY, hex dump, tree formatting, # WAV, DXF, 
- **Geometry**: coming soon. I have decades of code to merge and cleanup. # mesh, half edge mesh, mesh tools, intersections, point in polygon
- **Graphics**: color spaces galore: RGB, sRGB, HSL, HSV, CIELAB, CIEXYZ, YIQ, Sepia, Grayscale, simple bitmap, 
- **Fonts:** bitmap fonts
- **Information**: various CRCs, # Bytestream, bitstream
- **Numerical**: 2d/3d vector and matrix stuff, random number generation, quaternion (3), slerp (3), linear interpolation, Binomial coefficients, Factorials, Multinomial
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
* simple MVVM library: notifiable base, viewmodel base, dispatcher, relay command, property observer, mediator

### TestLomontSharp

Testing for all the above.



## TODO

Much more listed in the code TODO section. I have decades of code to pull pieces from and clean them up and generalize them.

## History

Release 0.6.6 - 2/19/2024 - fixed filter edge condition, updated to .NET 8.0, updated Skia sharp past a recent one with a security hole, fixed error in mat3x3 * vec2 behavior.

Release 0.6.5 - 1/18/2023 - added BinaryTree, fixed bitmap colors, added bilateral and Gaussian filters

Release 0.6.4 - 8/6/2022 - bug fixes, linear algebra unification

Release 0.6.3 - 8/5/2022 - added Vec4, matrix Trace & Cofactors & scalar & add/sub, Gaussian rand, LU factorization and solver, small degree (2,3,4) polynomial direct root solvers, Horn point cloud alignment

Release 0.6.2 - 6/17/2022 - added b-spline support, moved to dotnet 7.0 preview, added some INumeric generic math support.

Release 0.6.1 - 5/19/2022 - bug fixes, more rotation vector support, quaternion metrics

Release 0.6   - 5/16/2022 - Added singular value decomposition for 3xn, added best fit plane, lots of mat3/vec3/mat4/rotation helpers.

Release 0.5   - 5/06/2022 - Fixed some matrix math problems, fixed CRC24Q, added polyhedra, misc fixes

Release 0.4   - ??

Release 0.3   - 9/06/2021 - Added quaternions, slerp, binomial coefficients, factorial, multinomial, point in polygon, segment intersections, more geometry stuff, simple bitmaps, bitmap fonts, Hungarian algorithm for matching problems, kd-tree, nicer CRCs, mvvm lib

Release 0.2   - 8/23/2021 - Added things marked (#)

Release 0.1   - 8/19/2021 - Initial release