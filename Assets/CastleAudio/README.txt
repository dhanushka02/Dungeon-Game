CASTLE INTERIOR SOUND EFFECTS

AxeSwing: starts as the axe moves forward, including missed swings.
WoodHit1/2/3: trimmed chopping impacts; randomly varied slightly in pitch.
BarrelBreak: layered wood crack, body thump and falling debris on final hit.
WoodenWallBreak: heavier, longer wood collapse when the wall gives way.
HingedGateOpen: matches the existing key gate opening.
SlidingGateOpen: four-second portcullis movement, matched to the gate duration.

Volume controls are on Axe Interaction (player), Breakable Barrel,
Breakable Wooden Wall, and Locked Gate Prompt. Sliding-gate volume is 0.7.
Impact and breaking sounds originate at the object; swing sound stays close
to the player. Detached audio voices finish even when a barrel disappears,
then clean themselves up. No sounds play automatically on scene load.

The existing four-second sliding-gate speed, hit requirements, object layout,
key progression and disabled knocking setting have been retained.

CREDITS
Derived from the already installed Free Fantasy SFX Pack By TomMusic.
The original pack and its ReadMe remain in Assets/Custom Asset/SFX.
audio-sources.json records each source and edit. Timber thumps and debris
layers are original synthesized additions. No new packages were installed.

VERIFICATION — 2026-11-07
59 Play-mode checks passed using actual E and mouse input, including audio
voice timing, non-silent source PCM and listener mix, missed/rejected hits,
final-hit breaks, key pickup, both gate openings and automatic voice cleanup.
All eight edited audio files are mono with peak levels below clipping.
No existing scene objects or transforms were removed or moved.

Unity playback APIs:
https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AudioSource.PlayClipAtPoint.html
https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AudioSource.PlayOneShot.html
