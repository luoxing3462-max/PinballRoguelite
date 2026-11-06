# Repository and Asset Ownership

- Assets, Packages, and ProjectSettings are the canonical Unity project inputs.
- `ProjectSettings/ProjectSettings.asset` owns portrait WebGL dimensions, hashed WebGL filenames, Brotli compression, and decompression fallback for static hosting.
- Assets/Resources/Prefabs and Assets/Resources/Fonts are maintained runtime assets.
- `Assets/Editor/WebGlBuild.cs` builds the retained browser demo into `Builds/WebGL`; it refuses to overwrite a non-empty publish folder.
- `.github/workflows/deploy-pages.yml` publishes only `Builds/WebGL` through GitHub Pages after the project is connected to a user-authorized repository. `.gitignore` excludes Unity caches while allowing that one browser-build directory to be versioned.
- Library, Temp, Logs, and UserSettings are Unity-generated or machine-local and must not be treated as maintained deliverables.
- Cache/temp-* contains disposable task support and test output; final source and assets stay in their normal project owners.
- Tools/External contains retained third-party source references requested by the user. Do not load it as runtime content or edit upstream copies as part of gameplay work.
- The external Godot project is read-only reference material and is not an output target.
- No Git metadata is currently present in this project root. Preserve files in place and report local delivery without inventing a branch, commit, upstream, or push.
