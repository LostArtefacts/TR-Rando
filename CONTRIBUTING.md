# Development guidelines

The solution is built using Visual Studio. When you fork or copy the repository
and then build the solution for the first time, Visual Studio should restore all
dependencies, but these are listed as follows for reference.

### NuGet packages
* https://github.com/icsharpcode/SharpZipLib
* https://github.com/JamesNK/Newtonsoft.Json
* https://github.com/Nominom/BCnEncoder.NET

### Additional dependencies
* https://github.com/LostArtefacts/TRGameflowEditor/releases/latest
* https://github.com/lahm86/RectanglePacker/releases/latest

## Coding conventions

We aim to conform to [common C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).
In all cases, there should be no errors, warnings or messages displayed in Visual
Studio before committing changes for review. Use `Analyze` | `Run Code Analysis`
| `On Solution` to check for any conflicts.

## Testing guidelines

Automated tests will be run when a pull request is created, but there are also
offline-only tests which you should run manually in Visual Studio. These rely on
the original level files, which cannot legally be stored in the repository. Refer
to the test projects within the solution.

In addition, some manual checks should be performed depending on the nature of
your change. In most cases, running a full randomization of each supported game
should be the minimum requirement, ensuring that no errors occur and that the
outcomes are as expected.

All tests should pass before marking a pull request ready for review.

Automated tests will continue to be enhanced as the project evolves.

## Submitting changes

We commit via pull requests and avoid committing directly to `master`, which
is a protected branch. Each pull request gets peer-reviewed and should have at
least one approval from the code developer team before merging. We never merge
until all discussions are marked as resolved and generally try to test things
before merging. When a remark on the code review is trivial and the PR author
has pushed a fix for it, it should be resolved by the pull request author.
Otherwise we don't mark the discussions as resolved and give a chance for the
reviewer to reply. Once all change requests are addressed, we should re-request
a review from the interested parties.

## Changelog

We keep a changelog in [CHANGELOG.md](CHANGELOG.md). Anything other than an
internal change or refactor needs an entry there.
