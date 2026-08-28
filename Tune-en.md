# Tune

## Definition
Tune is a professional music player that lets you play music like a sound engineer or DJ. Adjust the EQ, rewind on a virtual turntable, or even remix music through audio analysis... Everything is under your control. You can record and/or save your work and enjoy it anytime.

## Features
- Multi-format audio support (MP3, FLAC, WAV, AAC, etc.)

- Graphical 8-band equalizer, EQ import/export

- Freely combinable controls (turntables, icons, covers, etc.)

- Acoustic and music theory analysis (BPM, key, chorus segment prediction, etc.)

- Playlist management (create, import, sort, favorite)

- Plugin functionality not yet supported

## Platform
Can only run on Windows devices that support .NET 10.

The sound card should support WASAPI / DirectSound to enable certain features.

## Build and Run
### Build
```
git clone <.Git Address>
cd Tune
dotnet restore
dotnet build --configuration Release
```

### Run
```
dotnet run --project Tune.csproj
```
You can also open the solution directly in Visual Studio to build and run it.

## Structure
```
Tune/
├── Appearance/         # Pages
├── Assets/             # Assets
├── Client/             # Playback control layer and Presenter layer
├── Components/         # Custom controls
├── Engine/             # Player
├── Generic/            # Generic classes
├── Helpers/            # Helper methods (converters, extension methods, etc.)
├── Local               # Local data access layer
├── Models/             # Data entities (music, playlists, lyrics, etc.)
├── Services/           # Services
├── Utils/              # Utility classes (parsing and IO, etc.)
├── Values/             # Constants
├── ViewModel/          # ViewModel
```

## Third-Party Libraries
| Library | Purpose | Notes | License |
|---|---|---|---|
| CommunityToolkit.Mvvm | MVVM | - | MIT |
| DevBase.Format | Lyric parsing | crap | MIT |
| Microsoft.Extensions.DependencyInjection | DI | - | MIT |
| NAudio | Music playback | - | MIT |
| NWave | Music parsing | - | MIT |
| Newtonsoft.Json | JSON parsing | - | MIT |
| TagLibSharp | File tag parsing | - | LGPL-2.1-only |
| WPFLocalizeExtension | Localization | - | Ms-PL |

Tune will strictly comply with open-source license agreements.

## Contribution Guidelines
Any form of contribution is welcome, including but not limited to reporting issues, submitting feature suggestions, improving documentation, or providing code.

Tink will open localization contributions after the preparation work is complete. People who are good at translation are welcome!

1. Fork this repository

2. Create your feature branch (`git checkout -b feature/your-feature-name`)

3. Commit your changes (`git commit -m 'your-commit-description'`) **Please strictly follow the commit standard requirements here.**

4. Push to the branch (`git push origin feature/amazing-feature`)

5. Open a Pull Request

Please at least ensure that the code style is consistent with the existing code, that there are no functional issues, and add the necessary unit tests.

## License
This project is licensed under the MIT License.

See the LICENSE file for details.

## Contact
Project homepage: [...]

Issue feedback: [...]

Email: f12fds@163.com