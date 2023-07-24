#define _main;

#include <GL/glew.h>
#include <GLFW/glfw3.h>

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

#ifndef _mapdata
    #include "src/mapdata.c"
    #define _mapdata
#endif

#if defined(PLATFORM_WEB)
#include <emscripten/emscripten.h>
#endif

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  MAIN LINE
// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

typedef struct {
    int id;
    RenderTexture2D target;
    Vector2 anchor;
    Rectangle rangle;
    Camera camera;
} TargetStruct;

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  HEADERS
// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

int InitializeMapDat(int bc);

int Viewport_3D_MapView_Init(TargetStruct* target);
int Viewport_3D_MapView(TargetStruct* target);

const Vector3 VECTOR_Y = {0, 1, 0};
const Vector3 VECTOR_X = {1, 0, 0};
const Vector3 VECTOR_Z = {0, 0, 1};

const int TARGET_COUNT = 4;
TargetStruct* targets;

Camera camera = {0};

int main(int argc, char **args)
{
    const int screenWidth = 1920;
    const int screenHeight = 1080;

    InitWindow(screenWidth, screenHeight, "mapeditor");

    InitializeMapDat(8096);

    targets = malloc(sizeof(TargetStruct) * TARGET_COUNT);

    for (int i = 0; i < TARGET_COUNT && i < 4; i++) {
        TargetStruct* t = targets + i;

        t->id = i;

        t->target = LoadRenderTexture(640, 400);
        t->anchor = (Vector2){640 * (i%2), 400 * (i/2)};
        t->rangle = (Rectangle){0, 0, (float)t->target.texture.width, (float)-t->target.texture.height};
        t->camera = (Camera){0};
    }

    Viewport_3D_MapView_Init(targets);

    camera.position = (Vector3){10.0f, 10.0f, 8.0f};
    camera.target = (Vector3){0.0f, 0.0f, 0.0f};
    camera.up = VECTOR_Y;
    camera.fovy = 90.0f;
    camera.projection = CAMERA_ORTHOGRAPHIC;

    SetTargetFPS(60);

    while (!WindowShouldClose())
    {
        UpdateCamera(&camera, CAMERA_FREE);

        //Process all renderports
        Viewport_3D_Mapview(targets + 0);

        BeginDrawing();

        ClearBackground((Color){0, 0, 0});

        TargetStruct* target;

        for (int i = 0; i < TARGET_COUNT; i++) {
            target = targets + i;

            DrawTextureRec(target->target.texture, target->rangle, target->anchor, WHITE);
        }

        EndDrawing();
    }

    CloseWindow();

    return 0;
}

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  MAP DATA
// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

//A list of the physical brushes to render
Brush** brushes;
int brushCount;
int brushMax;

Model cubeMesh;

int InitializeMapDat(int bc) {
    brushMax = bc;
    brushCount = 0;
    brushes = malloc(sizeof(Brush*) * brushMax);

    cubeMesh = LoadModelFromMesh(GenMeshCube(1, 1, 1));
}

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  VIEWPORT CODE
// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

int ActiveViewport = -1;

// ~~~~~~~~~~~~~~~~~~~~~~
// 3D MapView
// ~~~~~~~~~~~~~~~~~~~~~~

Vector3 camrot;
int Viewport_3D_MapView_Init(TargetStruct* target) {

    camrot = (Vector3){1, -1, 0};

    target->camera.position = (Vector3){-10, 10, 0};

    target->camera.target = Vector3Add(target->camera.position, camrot);
    target->camera.up = VECTOR_Y;
    target->camera.fovy = 90;
    target->camera.projection = CAMERA_PERSPECTIVE;


}
int Viewport_3D_MapView(TargetStruct* target) {

    if (ActiveViewport == target->id) { //Perform on-frame logic here

    }

    //Render everything

    UpdateCamera(&(target->camera), CAMERA_PERSPECTIVE);

    BeginTextureMode(target->target);

    ClearBackground(RAYWHITE);

    Brush* brush;
    for (int i = 0; i < brushCount; i++) {
        brush = *(brushes + i);

        Vector3 rot = brush->rotation.x;

        if (brush->visible) {
            DrawModelEx(cubeMesh, brush->position, rot, GetAngleToX(rot), brush->scale, WHITE);
        }
        else {
            DrawModelWiresEx(cubeMesh, brush->position, rot, GetAngleToX(rot), brush->scale, WHITE);
        }
    }

    EndTextureMode();
}