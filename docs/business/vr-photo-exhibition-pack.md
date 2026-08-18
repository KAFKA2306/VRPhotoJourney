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

The repository includes [`examples/exhibition.sample.json`](../../examples/exhibition.sample.json) as an input example. It does **not** include sample or customer photos, so the repository does not claim that a viewable sample exhibition is currently published.

A paid PoC is one event / one exhibition set with approximately 20–100 client-approved images, manifest normalization, generation, and delivery checklist review. Any second room, theme variant, later image replacement, or next event is a separate continuation request unless explicitly included in the engagement.

## PoC inquiry

[次回イベントでPoCを相談する](https://github.com/KAFKA2306/VRPhotoJourney/issues/new?title=%5BPoC%E7%9B%B8%E8%AB%87%5D+VR%E5%86%99%E7%9C%9F%E5%B1%95%E7%94%9F%E6%88%90&body=-+%E4%B8%BB%E5%82%AC%E8%80%85%E3%83%BB%E5%9B%A3%E4%BD%93%E5%90%8D%3A%0A-+%E3%82%A4%E3%83%99%E3%83%B3%E3%83%88%E5%90%8D+%2F+%E9%96%8B%E5%82%AC%E4%BA%88%E5%AE%9A%E6%99%82%E6%9C%9F%3A%0A-+%E5%86%99%E7%9C%9F%E6%9E%9A%E6%95%B0%E3%81%AE%E7%9B%AE%E5%AE%89%3A%0A-+Unity+%2F+VRChat%E3%83%AF%E3%83%BC%E3%83%AB%E3%83%89%E5%88%B6%E4%BD%9C%E7%8A%B6%E6%B3%81%3A%0A-+%E7%9B%B8%E8%AB%87%E3%81%97%E3%81%9F%E3%81%84%E5%86%85%E5%AE%B9%3A%0A%0A%E3%81%93%E3%81%AEIssue%E3%81%AF%E5%85%AC%E9%96%8B%E3%81%95%E3%82%8C%E3%81%BE%E3%81%99%E3%80%82%E5%80%8B%E4%BA%BA%E6%83%85%E5%A0%B1%E3%80%81%E9%9D%9E%E5%85%AC%E9%96%8B%E5%86%99%E7%9C%9F%E3%80%81%E8%AA%8D%E8%A8%BC%E6%83%85%E5%A0%B1%E3%80%81%E5%A5%91%E7%B4%84%E4%B8%8A%E3%81%AE%E7%A7%98%E5%AF%86%E6%83%85%E5%A0%B1%E3%81%AF%E8%A8%98%E8%BC%89%E3%81%97%E3%81%AA%E3%81%84%E3%81%A7%E3%81%8F%E3%81%A0%E3%81%95%E3%81%84%E3%80%82)

The inquiry opens a new public GitHub Issue with fields for organizer/group name, event timing, approximate photo count, current Unity/VRChat world-production state, and the requested support. Do not include personal information, unpublished photos, credentials, or confidential contract information in the public Issue.

Commercial results must be recorded only after they occur. Useful ordinary measures are sample-manifest visits, inquiries, inquiries that identify a real event/timing/photo count, customer demos, paid pilots, and repeat requests. Do not infer sales or purchase intent from repository activity or page views.

## Technical provenance

The report records the Unity version, semantic tool revision, generation timestamp, manifest identifiers, input SHA-256 values, generated asset paths, and reason codes. When `GITHUB_SHA` is available in the editor environment it is also recorded as the source commit; otherwise the report truthfully records `UNAVAILABLE` rather than inventing a commit.

Primary Unity APIs used by the generator:

- `AssetDatabase.LoadAssetAtPath`: https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AssetDatabase.LoadAssetAtPath.html
- `AssetDatabase.CreateAsset`: https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AssetDatabase.CreateAsset.html
- `PrefabUtility.SaveAsPrefabAsset`: https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PrefabUtility.SaveAsPrefabAsset.html
