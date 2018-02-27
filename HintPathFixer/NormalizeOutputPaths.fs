module NormalizeOutputPaths
open System
open System.Xml

open MsBuildUtils

type OutputPathState = 
    | Ok of assemblyName:string * outputPath:string
    | NotOk of assemblyName:string * outputPath1:string * outputPath2:string
    | Error of assemblyName:string


let private isSilverlight (doc: XmlDocument) (nms: XmlNamespaceManager) = 
    let projectTypesNode = doc.DocumentElement.SelectSingleNode ("//foo:PropertyGroup/foo:ProjectTypes", nms)
    if projectTypesNode <> null then
        let projectTypes = projectTypesNode.InnerText.Split(';')
        projectTypes |> Array.contains "A1591282-1198-4647-A2B1-27E5FF5F6F3B"
    else
        let node = doc.DocumentElement.SelectSingleNode ("//foo:PropertyGroup/foo:TargetFrameworkIdentifier", nms)
        if node <> null then 
            node.InnerText = "Silverlight"
        else false
    
    


let private isTestProject (doc: XmlDocument) (nms: XmlNamespaceManager) = 

    let projectTypesNode = doc.DocumentElement.SelectSingleNode ("//foo:PropertyGroup/foo:ProjectTypes", nms)
    if projectTypesNode <> null then
        let projectTypes = projectTypesNode.InnerText.Split(';')
        projectTypes |> Array.contains "3AC096D0-A1C2-E12C-1390-A8335801FDAB"
    else
        let node = doc.DocumentElement.SelectSingleNode ("//foo:PropertyGroup/foo:TestProjectType", nms)
        node <> null

let private normalizeOutputPaths (outputPath: string) (slOutputPath: string) (csprojFileName: string) = 

    let projectPath = csprojFileName.Substring(0, csprojFileName.LastIndexOf('\\'))
   
    let doc = XmlDocument()
    doc.Load csprojFileName
        
    let nms = XmlNamespaceManager(doc.NameTable)
    nms.AddNamespace("foo", doc.DocumentElement.NamespaceURI)
    

    let relativePath = 
        if isSilverlight doc nms then
            makeRelativePath projectPath slOutputPath
        else 
            makeRelativePath projectPath outputPath
            
    let outputPathNodes = doc.DocumentElement.SelectNodes ("//foo:PropertyGroup[@Condition]/foo:OutputPath", nms)
    
    outputPathNodes 
        |> Seq.cast<XmlNode>
        |> Seq.iter(fun referenceNode ->
            referenceNode.InnerText <- relativePath
        )

    doc.Save(csprojFileName)
 
let NormalizeOutputPaths outputPath slOutputPath path =
    ListProjectFiles path
        |> Array.filter (fun path -> not (path.Contains "Test")) 
        |> Array.filter (fun path -> not (path.Contains "Testing"))
        |> Array.iter (normalizeOutputPaths outputPath slOutputPath)
    () 