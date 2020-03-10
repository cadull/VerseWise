#r "paket:
storage: none
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli
nuget Fake.Core.Target //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO

let buildDir = "./bld/"
let solution = "VerseWise.sln"

Target.create "Clean" (fun _ ->
    buildDir
    |> Shell.cleanDir
)

Target.create "Build" (fun _ ->
    solution
    |> DotNet.build (fun options ->
        { options with
            OutputPath = Some buildDir
        }
    )
)

Target.create "Test" (fun _ ->
    solution
    |> DotNet.test (fun options ->
        { options with
            NoBuild = true
            Output = Some buildDir;
        }
    )
)

Target.create "Default" ignore

"Clean"
    ==> "Build"
    ==> "Test"
    ==> "Default"

Target.runOrDefault "Default"
