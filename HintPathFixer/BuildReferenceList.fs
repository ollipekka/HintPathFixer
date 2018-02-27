module BuildReferenceList

open System.Xml
open MsBuildUtils

type OutputPathState = 
    | Ok of assemblyName:string * outputPath:string
    | NotOk of assemblyName:string * outputPath1:string * outputPath2:string
    | Error of assemblyName:string
    | InvalidDocument

let private calculateOutputPaths (csprojFileName: string) = 

    let projectPath = csprojFileName.Substring(0, csprojFileName.LastIndexOf('\\'))
    
    let doc = XmlDocument()
    doc.Load csprojFileName
        
    let nms = XmlNamespaceManager(doc.NameTable)
    nms.AddNamespace("foo", doc.DocumentElement.NamespaceURI)
    
    let assemblyNameNode = doc.DocumentElement.SelectSingleNode("//foo:PropertyGroup/foo:AssemblyName", nms)
    if assemblyNameNode <> null then
        let assemblyName = assemblyNameNode.InnerText
        let outputPathNodes = doc.DocumentElement.SelectNodes ("//foo:PropertyGroup[@Condition]/foo:OutputPath", nms)
        if outputPathNodes.Count >= 2 && outputPathNodes.[0].InnerText = outputPathNodes.[1].InnerText then
            let outputPath = outputPathNodes.[0].InnerText
            Ok(assemblyName, (toAbsolutePath projectPath outputPath assemblyName))
        else 
            if outputPathNodes.Count >= 2 then
                NotOk(assemblyName, (toAbsolutePath projectPath outputPathNodes.[0].InnerText assemblyName), (toAbsolutePath projectPath outputPathNodes.[1].InnerText assemblyName))
            else 
                Error(assemblyName)
    else InvalidDocument



let ListReferences path =
    let projFiles = ListProjectFiles path
    let outputPathsByAssemblyName = projFiles |> Array.map calculateOutputPaths
    
    outputPathsByAssemblyName 
        |> Array.map (fun result -> 
            match result with
            | Ok(assemblyName, outputPath) -> Some (assemblyName, outputPath)
            | _ -> None
                            ) 
        |> Array.choose id
        |> Map.ofSeq