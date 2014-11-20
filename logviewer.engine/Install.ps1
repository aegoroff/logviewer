param($installPath, $toolsPath, $package, $project)

Edit-XmlNodes -project $project -name "grok.patterns"
Edit-XmlNodes -project $project -name "webservers.patterns"

function Edit-XmlNodes {
param (
    $project = $(throw "project is a required parameter"),
    $name = $(throw "name is a required parameter")
)   
    # Load project XML.
    $doc = New-Object System.Xml.XmlDocument
    $doc.Load($project.FullName)
    $namespace = 'http://schemas.microsoft.com/developer/msbuild/2003'

    # Find the node containing the file. The tag "Content" may be replace by "None" depending of the case, check your .csproj file.    
    $xmlHode = Select-Xml "//msb:Project/msb:ItemGroup/msb:None[@Include='$name']" $doc -Namespace @{msb = $namespace}
    
    if(!$xmlHode) {
        $xmlHode = Select-Xml "//msb:Project/msb:ItemGroup/msb:Content[@Include='$name']" $doc -Namespace @{msb = $namespace}
    }
    
    #check if the node exists.
    if($xmlHode -ne $null)
    {
        $nodeName = "CopyToOutputDirectory"

        #Check if the property already exists, just in case.
        $property = $xmlHode.Node.SelectSingleNode($nodeName)
        if($property -eq $null)
        {
            $property = $doc.CreateElement($nodeName, $namespace)
            $property.AppendChild($doc.CreateTextNode("Always"))
            $xmlHode.Node.AppendChild($property)

            # Save changes.
            $doc.Save($project.FullName)
        }
    }
}