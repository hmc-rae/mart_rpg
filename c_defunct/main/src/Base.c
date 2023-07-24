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

/*  --------------------------------------------------------------------------------------
        GAME ENTITIES
    --------------------------------------------------------------------------------------
*/

unsigned long ENTITY_ID = 0;

typedef struct entity_generic {
    unsigned long ID;

    Vector3 position;
    Vector3 rotation;

    int health;
    char IsAlive;
    
} entity_generic;


/*  --------------------------------------------------------------------------------------
        GAME SCENE
    --------------------------------------------------------------------------------------
*/

typedef struct gamescene_generic {

    

} GameScene;