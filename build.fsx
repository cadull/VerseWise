#r "paket:
storage: none
nuget Fake.Core.Target
nuget Fake.DotNet.Cli
nuget Fake.DotNet.Paket
nuget Fake.IO.FileSystem
nuget FSharp.Data //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open FSharp.Data

let buildDir = "./build/"
let solution = "VerseWise.sln"

type GitVersion = JsonProvider<""" {
  "Major":0,
  "Minor":1,
  "Patch":0,
  "PreReleaseTag":"fake-build.1",
  "PreReleaseTagWithDash":"-fake-build.1",
  "PreReleaseLabel":"fake-build",
  "PreReleaseNumber":1,
  "WeightedPreReleaseNumber":30001,
  "BuildMetaData":12,
  "BuildMetaDataPadded":"0012",
  "FullBuildMetaData":"12.Branch.feature-fake-build.Sha.bf48b31855eea2ad00e7eac61f0778ddb20d6333",
  "MajorMinorPatch":"0.1.0",
  "SemVer":"0.1.0-fake-build.1",
  "LegacySemVer":"0.1.0-fake-build1",
  "LegacySemVerPadded":"0.1.0-fake-build0001",
  "AssemblySemVer":"0.1.0.0",
  "AssemblySemFileVer":"0.1.0.0",
  "FullSemVer":"0.1.0-fake-build.1+12",
  "InformationalVersion":"0.1.0-fake-build.1+12.Branch.feature-fake-build.Sha.bf48b31855eea2ad00e7eac61f0778ddb20d6333",
  "BranchName":"feature/fake-build",
  "EscapedBranchName":"feature-fake-build",
  "Sha":"bf48b31855eea2ad00e7eac61f0778ddb20d6333",
  "ShortSha":"bf48b31",
  "NuGetVersionV2":"0.1.0-fake-build0001",
  "NuGetVersion":"0.1.0-fake-build0001",
  "NuGetPreReleaseTagV2":"fake-build0001",
  "NuGetPreReleaseTag":"fake-build0001",
  "VersionSourceSha":"81a40f8e96141bbacc71785b177fd275e2daa637",
  "CommitsSinceVersionSource":12,
  "CommitsSinceVersionSourcePadded":"0012",
  "CommitDate":"2020-03-12"
} """>

Target.create "Clean" (fun _ ->
    buildDir
    |> Shell.cleanDir
)

Target.create "Restore" (fun _ ->
    Paket.restore (fun options ->
        { options with 
            ToolType = ToolType.CreateLocalTool ()
        }
    )
    solution |> DotNet.restore id
)

Target.create "Version" (fun _ ->
    let r =
        DotNet.exec (fun options ->
            { options with
                RedirectOutput = true
            }
        ) "gitversion" ""

    r.Messages
    |> String.concat ""
    |> GitVersion.Parse
    |> FakeVar.set "GitVersion"
)

let buildParams (p : MSBuild.CliArguments) =
    let v : GitVersion.Root = FakeVar.getOrFail "GitVersion"
    { p with
        Properties =
            ("Version", v.AssemblySemVer)
            :: ("FileVersion", v.AssemblySemFileVer)
            :: ("InformationalVersion", v.InformationalVersion)
            :: p.Properties
    }

Target.create "Build" (fun _ ->
    solution
    |> DotNet.build (fun options ->
        { options with
            OutputPath = Some buildDir
            MSBuildParams = buildParams options.MSBuildParams
        }
    )
)

Target.create "Test" (fun _ ->
    solution
    |> DotNet.test (fun options ->
        { options with
            NoBuild = true
            Output = Some buildDir
            MSBuildParams = buildParams options.MSBuildParams
        }
    )
)

Target.create "Default" ignore

"Clean"
    ==> "Restore"
    ==> "Version"
    ==> "Build"
    ==> "Test"
    ==> "Default"

Target.runOrDefault "Default"
