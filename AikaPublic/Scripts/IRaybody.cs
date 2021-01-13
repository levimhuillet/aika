using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * The interface for the Raybody class.
 * A Raybody often works in tandem with a Raycaster. It is responsible for the movement side of things.
 * In comparison, the Raycaster detects collisions.
 * For example, an object that needs to handle collisions but never moves (such as a Key) only needs a Raycaster.
 * Together these are my alternative to using Unity's rigidbodies, which were causing me issues.
 */
public interface IRaybody
{
    // ---------- Member Functions ----------

    /*
     * Move's the object to its new position after all physics calculations have been performed
     */
    void Move(Vector2 dir);

    // ---------- Raycasts ---------- 

    /*
     * One succinct function which calls each relevant raycasting function below
     * for identifying nearby objects with the tag "Structures".
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */ 
    void RaycastStructures();

    /*
     * Raycasts for objects with the tag "Structures" above this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    void RaycastStructuresAbove();

    /*
     * Raycasts for objects with the tag "Structures" to both sides of this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    void RaycastStructuresSide();

    /*
     * Raycasts for objects with the tag "Structures" below this Raybody 
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    void RaycastStructuresBelow();

    /*
     * One succinct function which calls each relevant raycasting function below
     * for identifying nearby objects with the tag "MovingStructures".
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    void RaycastMovingStructures();

    /*
     * Raycasts for objects with the tag "MovingStructures" above this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    void RaycastMovingStructuresAbove();

    /*
     * Raycasts for objects with the tag "MovingStructures" to both sides of this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    void RaycastMovingStructuresSide();

    /*
     * Raycasts for objects with the tag "MovingStructures" below this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    void RaycastMovingStructuresBelow();

    /*
     * One succinct function which calls each relevant raycasting function below
     * for identifying nearby objects with the tag "Button".
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    void RaycastButtons();

    /*
     * Raycasts for objects with the tag "Button" to both sides of this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    void RaycastButtonsSide();

    /*
     * Raycasts for objects with the tag "Button" below this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    void RaycastButtonsBelow();

    /*
     * One succinct function which calls each relevant raycasting function below
     * for identifying nearby objects with the tag "NewBlock".
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    void RaycastNewBlocks();

    /*
     * Raycasts for objects with the tag "NewBlock" above this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    void RaycastNewBlocksAbove();

    /*
     * Raycasts for objects with the tag "NewBlock" to both sides of this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    void RaycastNewBlocksSide();

    /*
     * Raycasts for objects with the tag "NewBlock" below this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    void RaycastNewBlocksBelow();

    /*
     * Returns this Raybody's velocity
     */
    Vector2 GetVelocity();

    /*
     * Adds a velocity to this Raybody's velocity
     */ 
    void AddVelocity(Vector2 velocity);
}
