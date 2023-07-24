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

#define BSP_EMPTY -1
#define BSP_SOLID -2

typedef struct bsp_node {
    int num;
    int left;
    int right;

    Vector3 normal;
} bsp_node;

int bspCheckPoint(bsp_node *nodes, int node_num, Vector3 pos) {

    Vector3 angle;
    bsp_node *node;
    float dot;
    int num = node_num;

    while (num >= 0) {
        node = nodes;
        node += (num * sizeof(bsp_node));

        angle = Vector3Normalize(Vector3Subtract(pos, node->origin));
        dot = Vector3DotProduct(node->normal, angle); //1 = forward, -1 = backward

        if (dot < 0) { //behind: check left
            num = node->left;
        }
        else { //forward: check right
            num = node->right;
        }
    }

    return num;
}

//Returns true if the line intersects a solid, and sets the intersect vector to the point of intersection.
char bspCheckLine(bsp_node *nodes, int node_num, Vector3 p1, Vector3 p2, Vector3 *intersect) {

    //Leaves
    if (node_num == BSP_SOLID) { //Solid node, set intersect to p1 and return 
        *intersect = p1;
        return 1;
    }
    else if (node_num == BSP_EMPTY) {
        return 0; //Empty node, does not hit here
    }
}