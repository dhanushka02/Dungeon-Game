CastleInterior roof

The Roof root contains a stone ceiling, timber supports, two small grated skylights, subtle ceiling fill lights and a lifting pocket above the portcullis.

HIDE WHILE EDITING
Use Tools > Castle Roof > Toggle Roof Visibility in Scene View, or the Hierarchy eye beside Roof. Scene Visibility does not hide the roof in Play mode or a build.

REMOVE
Disable or delete the single Roof root. Existing walls, floors, lights, player and gameplay components are independent.

TECHNICAL NOTES
The ceiling underside is approximately 3.02 metres above the floor. Timber beams sit below it; colliders prevent jumping through the ceiling. The gate pocket extends above the portcullis's full configured 3.8-metre lift. Ceiling sections share materials but have separate renderers so nearby point lights illuminate the appropriate area. Roof meshes include normals and tangents for the stone normal map. Ceiling footprint is generated from the union of the existing 44 flat floor tiles; vertical wooden barricades are excluded.

Original project wall and wood materials were copied into this folder. No external packages or downloads were added.
