#define _mapdata

#ifndef _stdio
    #include <stdio.h>
    #define _stdio
#endif

#ifndef _malloc
    #include <malloc.h>
    #define _malloc
#endif

#ifndef _raylib
    #include "raylib.h"
    #define _raylib
#endif

#ifndef _raymath
    #include "raymath.h"
    #define _raymath
#endif

#ifndef _bsp
    #include "src/BSP.c"
    #define _bsp
#endif

#ifndef _main
    #include "main.c"
    #define _main
#endif

typedef struct primitive_brush{
    Vector3 position;
    Vector3 scale;
    
    RMatrix rotation;

    char solid;
    char visible;
} Brush;