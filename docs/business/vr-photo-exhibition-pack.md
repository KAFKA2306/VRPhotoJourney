# VR Photo Exhibition Pack

## Product boundary

This product is a **build-time Unity Editor production aid** for an event organizer or world creator who already has permission to use the supplied images. It does not provide a VRChat runtime feature that enumerates or reads arbitrary folders on an attendee's PC.

The delivery unit is one exhibition/event pack. The client supplies an approved manifest and `.jpg`, `.jpeg`, or `.png` images before the world build.

## Inputs

- versioned `exhibition.json`
- exhibition ID, title, organizer display name
- source folder inside the Unity project
- ordered photo IDs and source file names
- optional display title, caption, and author display name
- explicit rights status: `CONFIRMED_BY_CLIENT` or `SAMPLE_LICENSED`
- a Photo Frame prefab with a Renderer

Customer photos are not repository fixtures and are not analytics payloads.

## Generated delivery

`Tools > Slideshow Generator > Generate Exhibition Pack` creates a dedicated `Assets/.../Generated/<exhibition-id>/` tree containing:

- copied texture assets; the source files are not renamed, deleted, or overwritten
- one generated material per accepted photo
- one Photo Frame prefab per accepted photo
- one root exhibition prefab in manifest order
- `exhibition-report.json`
- `exhibition-report.md`

Each accepted input is traceable by SHA-256. Unsupported formats, missing files, source-folder path escape, duplicate checksum, Unity import failure, missing Renderer/shader, and prefab save failure are kept as separate reason codes. Zero successfully generated photos is a failed pack, not a successful empty exhibition.

The output copies are generated delivery assets and may be overwritten on a subsequent generation of the same exhibition ID. The original files under `sourceFolder` are never intentionally modified by this generator.

## Human review required

Generation is not publication approval. Before delivery, a human must verify:

1. every displayed image, caption, and author against the approved client manifest;
2. image usage rights and event/world publication permission;
3. actual layout and readability in the target Unity project;
4. VRChat world performance and SDK/build requirements applicable to that project.

The tool does not guarantee VRChat publication, performance, moderation approval, event attendance, image licensing, or commercial outcome.

## Free sample vs paid PoC

The free sample may use only self-created or otherwise redistribution-permitted images. The repository intentionally ships only a manifest example, not customer images.

A paid PoC is one event / one exhibition set with approximately 20–100 client-approved images, manifest normalization, generation, and delivery checklist review. Any second room, theme variant, later image replacement, or next event is a separate continuation request unless explicitly included in the engagement.

## CTA and evidence states

User-facing or sales surfaces may use these calls to action:

- `サンプル写真展を見る`
- `自分の写真展を作る`
- `次回イベントでPoCを相談する`

Commercial evidence is recorded only after the event actually occurs, using distinct states:

- `sample_exhibition_opened`
- `exhibition_inquiry_started`
- `qualified_inquiry`
- `customer_demo_completed`
- `paid_pilot`
- `repeat_exhibition_requested`

Do not infer `qualified_inquiry`, `paid_pilot`, or repeat intent from page views or repository activity.

## Technical provenance

The report records the Unity version, semantic tool revision, generation timestamp, manifest identifiers, input SHA-256 values, generated asset paths, and reason codes. When `GITHUB_SHA` is available in the editor environment it is also recorded as the source commit; otherwise the report truthfully records `UNAVAILABLE` rather than inventing a commit.

Primary Unity APIs used by the generator:

- `AssetDatabase.LoadAssetAtPath`: https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AssetDatabase.LoadAssetAtPath.html
- `AssetDatabase.CreateAsset`: https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AssetDatabase.CreateAsset.html
- `PrefabUtility.SaveAsPrefabAsset`: https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PrefabUtility.SaveAsPrefabAsset.html
