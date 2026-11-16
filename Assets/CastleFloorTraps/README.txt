CASTLE INTERIOR — ROLLING DRUM AND ALTERNATING FLOOR SPIKES

Scene group: Castle_FloorTraps
The hazards occupy the upper room after the furnished guards' room and before
the first of the two wooden barricades. All new props, UI, sound sources,
and checkpoint are descendants of this single removable group.

ROUTE
Enter from the bottom-left room. Wait on the clear stone at the entrance,
watch the rolling drum pass, then cross behind it while it pauses at the far
end. Turn right beyond the rail. Cross floor bank A when it retracts, wait on
the clear stone between the banks, then cross B when it retracts to reach the wooden barrier.
Normal walking is sufficient; jumping is not required. Floor holes, moving
spikes, worn tracks, endpoint pauses and mechanical sounds signal the hazards.

TIMING
Drum: 2.2 seconds each way, with a 0.75-second pause at each endpoint.
Floor banks: 7.6-second cycle, offset from one another by 3.8 seconds.
Each cycle: 3.6 seconds down, 0.8 seconds warning/click, 0.4 seconds rising,
2.4 seconds up, 0.4 seconds retracting. A and B are never dangerously raised
together. Only raised spikes can hit the player; the sockets are safe when down.

CHECKPOINT
A hit briefly fades the screen and returns the player to the room entrance.
The scene is not reloaded. The axe, key, guard notes, map, family drawing,
broken props, torch puzzle and open gates retain their
current session state. A short immunity period prevents immediate repeat hits.
FirstPersonController.ResetAfterTeleport clears movement/jump momentum and
camera pitch only when explicitly called by the checkpoint. Normal movement
behaviour is unchanged. Camera warping uses the installed Cinemachine API.
The checkpoint does not add persistent save-game storage.

REMOVE OR DISABLE
Disable Castle_FloorTraps to remove all hazards, rails, prompts and sounds
at once. Delete the group to remove it from the scene. Disabling during
a checkpoint response restores the player's previous control-enabled states.
The guards' room and all original puzzles remain independent.

ASSETS
The forged spike, perforated platform mesh and new materials are original assets.
RollingRumble.wav, SpikeWarning.wav, SpikeMovement.wav and TrapHit.wav are
original synthesized effects. No packages or global lighting settings were changed.

VERIFICATION
The real-input Play-mode check covers actual character collisions, down-spike
safety, drum travel/rotation/pauses, alternating phases, fade and respawn,
inventory/clue preservation, held-jump reset, normal first-person grounding,
disable-during-respawn recovery, an entire walk at normal speed without hits,
existing guard-room interactions and original puzzle settings.
Temporary editor setup/test tools are archived outside the project after use.

VISUAL CORRECTION — 2026-11-16
Removed the son's scarf, its peg, its inspection prompt and its dedicated mesh.
Replaced the coplanar metal panels with raised perforated decks and real holes.
The warning moves the deck only 8mm; the socket wells remain fixed above the
stone throughout the cycle. A fixed 6cm support under each deck lets the player
walk onto it without jumping. Existing hazard timing and damage are unchanged.

All 63 updated Play-mode checks passed, including rendered hole visibility
through every sampled warning phase on both banks, real mesh apertures, deck
clearance through the entire cycle, and a full walk without jumping or damage.
No commit, push or build/export was performed for this correction.
