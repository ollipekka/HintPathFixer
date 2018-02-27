module NormalizeReferences

open System
open System.Xml

open MsBuildUtils
 
 
let fixReferences (references: Map<string, string>) (csprojFileName: string) =
    let projectPath = csprojFileName.Substring(0, csprojFileName.LastIndexOf('\\'))
   
    
    let doc = XmlDocument()
    doc.Load csprojFileName
        
    let nms = XmlNamespaceManager(doc.NameTable)
    nms.AddNamespace("foo", doc.DocumentElement.NamespaceURI)

    let getAssemblyName (node: XmlNode) =
        let assemblyNode = node.SelectSingleNode ("@Include", nms)
        let text = assemblyNode.InnerText
        let cutOff = text.IndexOf(',')

        if cutOff > -1 then text.Substring(0, cutOff) 
        else text
        
    let assemblyNameNode = doc.DocumentElement.SelectSingleNode("//foo:PropertyGroup/foo:AssemblyName", nms)
    
    let outputPathNodes = doc.DocumentElement.SelectNodes ("//foo:PropertyGroup[@Condition]/foo:OutputPath", nms)

        
    if assemblyNameNode <> null && outputPathNodes.Count > 0 && outputPathNodes.[0].InnerText = outputPathNodes.[1].InnerText then
        let referenceNodes = doc.DocumentElement.SelectNodes ("//foo:Reference", nms)
        referenceNodes
            |> Seq.cast<XmlNode>
            |> Seq.filter(fun referenceNode ->
                let referenceAssemblyName = getAssemblyName referenceNode
                references |> Map.containsKey referenceAssemblyName)
            |> Seq.iter(fun referenceNode ->
                let referenceAssemblyName = getAssemblyName referenceNode
                let path = references.[referenceAssemblyName]
                let hintPathNode = referenceNode.SelectSingleNode ("foo:HintPath", nms)

                if hintPathNode = null then
                    let hintPathNode = doc.CreateNode(XmlNodeType.Element, "HintPath", doc.DocumentElement.NamespaceURI)
                    hintPathNode.InnerText <- makeRelativePath projectPath path
                    referenceNode.AppendChild(hintPathNode) |> ignore
                else
                    hintPathNode.InnerText <- makeRelativePath projectPath path
            )
        doc.Save(csprojFileName)


let NormalizeReferences (referenceList: Map<string, string>) (path: string) =
    let projFiles = ListProjectFiles path
    projFiles |> Array.iter (fixReferences referenceList)