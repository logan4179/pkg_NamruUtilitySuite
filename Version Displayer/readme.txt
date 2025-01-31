Displays version information on the canvas via button-activated dropdown.

----[Placing in Scene]----
* Inside the package folder, drag the '[Version Displayer]' prefab onto the canvas you want to display the version, and position/scale it. 

----[Setting up the Version Displayer]----
* It needs a File Path, and a Version separator string, which you can set up as serialized properties in the inspector, or through your code.
* LVD has only one major method, "TryGetAndParseChangeLogFile()". Aside from this, it only has a couple of simple UI methods for flipping/setting text body visibility.
By default, it doesn't automatically call TryGetAndParseChangeLogFile() on Start(), so you need to make a reference to the LVD object somewhere 
in your scripts, and call that when appropriate. You can also set the directory via script and set it's body visibility to a default state.
A typical use case would be the following:

public class DataManager : Monobehaviour
{
    [SerializeField] LogansVersionDisplayer versionDisplayer;

    void Start()
    {
        versionDisplayer.SetBodyActiveState(false);
        versionDisplayer.FilePath = Path.Combine(NamruSessionManager.Instance.DirPath_NamruDirectory, "changelog.txt");
        versionDisplayer.TryGetAndParseChangeLogFile();
    }
}

----[The Changelog File]----
* The changelog file should be a simple .txt file.
* For each entry, assuming "Version" is the version separator string you set, it should have the following format:
Version
0.01
1/31/2025
* Made the first build