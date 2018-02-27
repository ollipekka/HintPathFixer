# HintPathFixer

Improved and more complex version of the CopyLocalFixer.

`-fixOutputPaths` takes a path with C# project files and desired output path. The command then rewrites the outputpaths with relative paths from the project file location to the given outputpath.

`-fixReferences` builds an authoritive list of assembly locations based on the outputpaths given in the project files. It then goes through all the references of the project files in the path and correct reference HintPaths based on the list.

## ToDo

* Do not write HintPath if reference assembly is in the same path as the project.
* Add excluded words in project names parameter.
* Figure out what to do with different project types, such as testing and silverlight.
