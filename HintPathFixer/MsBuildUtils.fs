module MsBuildUtils
open System
open System.IO
open System.Xml
open System.IO
    
let addSlash (dir: string) = 
    if not (dir.EndsWith(".dll")) && not (dir.EndsWith("\\")) then dir + "\\" else dir

let toAbsolutePath projectPath outputPath assemblyName =
    let absolutePath = Path.Combine(projectPath, outputPath)
    addSlash (Path.GetFullPath((new Uri(absolutePath)).LocalPath)) + assemblyName + ".dll"

let makeRelativePath projectPath outputPath =
    let projectUri = Uri(addSlash projectPath)
    let outputUri = Uri(addSlash outputPath)
    let relativePath = projectUri.MakeRelativeUri(outputUri)
    relativePath.ToString().Replace("/", "\\");

let ListProjectFiles (path: string) = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories)