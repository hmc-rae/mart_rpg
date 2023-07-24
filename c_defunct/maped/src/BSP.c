#define _bsp

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

#ifndef _mapdata
    #include "src/mapdata.c"
    #define _mapdata
#endif

#ifndef _main
    #include "main.c"
    #define _main
#endif

#ifndef CONTENTS_EMPTY
    #define CONTENTS_EMPTY -1;
    #define CONTENTS_SOLID -2;
#endif

typedef struct bsp_node {
    int num;
    int left;
    int right;

    Vector3 normal;
    Vector3 position;
} BSPNode;

typedef struct rotation_matrix {
    Vector3 x;
    Vector3 y;
    Vector3 z;
} RMatrix;

typedef union rotation_union {
    float a;
    float d;
    float g;
    float b;
    float e;
    float h;
    float c;
    float f;
    float i;
} r_matrix_b;

typedef union {
    RMatrix vec;
    r_matrix_b std;
} RMatrix_U;

RMatrix RMatrix_Diagonal(Vector3 r) {
    RMatrix m;
    m.x.x = r.x;
    m.y.y = r.y;
    m.z.z = r.z;
    return m;
}

Vector3 RMatrix_FromDiagonal(RMatrix m) {
    Vector3 r;
    r.x = m.x.x;
    r.y = m.y.y;
    r.z = m.z.z;
    return r;
}

float GetAngleToX(Vector3 n) {
    float dot = Vector3DotProduct(n, VECTOR_X);
    dot /= Vector3Length(n);
    return dot;
}

RMatrix RMatrix_Multiply(RMatrix a, RMatrix b) {
    RMatrix_U k;
    RMatrix_U A;
    RMatrix_U B;
    A.vec = a;
    B.vec = b;

    k.std.a = (A.std.a * B.std.a) + (A.std.b * B.std.d) + (A.std.c * B.std.g); //aa*ba + ab*bd + ac*bg
    k.std.b = (A.std.a * B.std.b) + (A.std.b * B.std.e) + (A.std.c * B.std.h); //aa*bb + ab*be + ac*bh
    k.std.c = (A.std.a * B.std.c) + (A.std.b * B.std.f) + (A.std.c * B.std.i); //aa*bc + ab*bf + ac*bi

    k.std.d = (A.std.d * B.std.a) + (A.std.e * B.std.d) + (A.std.f * B.std.g); //ad*ba + ae*bd + af*bg
    k.std.e = (A.std.d * B.std.b) + (A.std.e * B.std.e) + (A.std.f * B.std.h); //ad*bb + ae*be + af*bh
    k.std.f = (A.std.d * B.std.c) + (A.std.e * B.std.f) + (A.std.f * B.std.i); //ad*bc + ae*bf + af*bi

    k.std.g = (A.std.g * B.std.a) + (A.std.h * B.std.d) + (A.std.i * B.std.g); //ag*ba + ah*bd + ai*bg
    k.std.h = (A.std.g * B.std.b) + (A.std.h * B.std.e) + (A.std.i * B.std.h); //ag*bb + ah*be + ai*bh
    k.std.i = (A.std.g * B.std.b) + (A.std.h * B.std.f) + (A.std.i * B.std.i); //ag*bc + ah*bf + ai*bi
    
    return k.vec;
}

//Checks the type of space a given position is in.
int bsp_navigate(BSPNode* nodes, int num, Vector3 pos) {

    Vector3 dist;
    float dot;
    BSPNode* node;

    while (num >= 0) {
        node = nodes + num;

        dist = Vector3Subtract(pos, node->position);
        dot = Vector3DotProduct(node->normal, dist);

        if (dot > 0) { //Check forward node (left)
            num = node->left;
        } 
        else { //Check backward node (right)
            num = node->right;
        }
    }

    return num;
}

//Checks the type of space a given box, at a given position, is in.
//The given matrix corresponds to the dimensions of the shape. It can be rotated.
int bsp_navigate(BSPNode* nodes, int num, Vector3 pos, RMatrix matrix) {
    Vector3 dist;
    float dot;
    BSPNode* node;
    RMatrix mutate;
    Vector3 sum;

    while (num != 0) {
        node = nodes + num;

        //Mutate the normal and add it to the position to get the new point for distance
        mutate = RMatrix_Diagonal(node->normal);
        mutate = RMatrix_Multiply(matrix, mutate);
        sum = RMatrix_FromDiagonal(mutate);

        sum = Vector3Add(node->position, sum);

        dist = Vector3Subtract(pos, sum);
        dot = Vector3DotProduct(node->normal, dist);

        if (dot > 0) { //Check forward node (left)
            num = node->left;
        } 
        else { //Check backward node (right)
            num = node->right;
        }
    }

    return num;
}