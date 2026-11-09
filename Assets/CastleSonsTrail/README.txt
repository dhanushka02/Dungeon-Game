CASTLE INTERIOR — THE SON'S TRAIL

Scene group: Castle_SonsTrail_Puzzle

Play from the existing temporary first-person player. Complete the earlier
axe/key gate section and break WoodWall1 (currently five hits).

1. Just past that wall, look down near the left side of the passage for the
   wooden horse wearing the red scarf. Look at it and press E to inspect.
2. Follow the passage to the original barrels. The drawing is mounted above
   them. Look at it and press E to read it. It can be read again at any time.
3. Look at a watcher torch and press E to light it. The correct order is
   MOON, SUN, STAR. A wrong choice extinguishes all flames so you can retry.
   Pressing an already-lit torch does not count again.
4. The portcullis rises automatically after the third correct torch. The
   objective changes to continue searching beyond the gate. Knocking is
   currently disabled; Enable Knocking on Castle Son Puzzle restores it.

There is no additional scene transition or final rescue in this section.
Puzzle progress resets when Play mode restarts, like the existing key puzzle.

TO DISABLE OR REMOVE
Disable the single Castle_SonsTrail_Puzzle root in the hierarchy to temporarily
remove this entire section, including its gate, sounds and UI. Delete that root
to remove the scene additions permanently. Existing walls, floors, barrels,
axe, player and earlier gate are independent of this group.

ASSETS AND IMPLEMENTATION
CastleSonPuzzle.cs owns progression, UI and gate motion.
CastlePuzzleTarget.cs identifies interactable clues and torches.
Materials and symbol meshes are unique to this group. The horse is built from
simple carved-block shapes. Existing project stool/torch prefabs and TomMusic
sound effects are reused. DistantKnocking.wav is an original synthesized effect.
No new packages or external downloads were added.

VERIFICATION (2026-11-06)
38 real-input Play-mode checks passed, including range and wall occlusion,
wrong-order retry, repeated/held E, gate animation and controller collision,
walking the new route, and regression of the original axe, five-hit wall,
key barrel, key pickup and earlier gate. No runtime errors during the run.
Before/after comparison: zero existing objects removed or transforms changed.
No build/export was performed.

Unity API references used:
https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Physics.Raycast.html
https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AudioSource-spatialBlend.html
https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/Documentation~/read-devices-directly.md
