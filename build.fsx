#r "paket:
storage: none
nuget Fake.Core.Target
nuget Fake.DotNet.Cli
nuget Fake.DotNet.Paket
nuget Fake.IO.FileSystem //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO

let buildDir = "./build/"
let solution = "VerseWise.sln"

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
    ==> "Restore"
    ==> "Build"
    ==> "Test"
    ==> "Default"

Target.runOrDefault "Default"
