open System
open BuildReferenceList
open NormalizeReferences
open NormalizeOutputPaths

[<EntryPoint>]
let main argv =

    if argv.[0] = "-fixOutputPaths" then
        let path = argv.[1]
        let outputPath = argv.[2]
        let slOutputPath = argv.[3]
        NormalizeOutputPaths outputPath slOutputPath path             

    elif argv.[0] = "-fixReferences" then
        let path = argv.[1]
        let referenceList = ListReferences path
        NormalizeReferences referenceList path   
    else 
        
        let path = argv.[0]
        let outputPath = argv.[1]
        let slOutputPath = argv.[2]
        NormalizeOutputPaths outputPath slOutputPath path 
        
        let referenceList = ListReferences path
        NormalizeReferences referenceList path   

        printfn "HintPathFixer fixes broken OutputPaths HintPaths in csproj references."

    0
