# 🔌 Plugin-Based Architecture in .NET

## 🚀 Overview
This project is a **dynamic plugin system in .NET** that allows you to load and execute plugins based on the currently active application. It follows a fully **decoupled architecture**, meaning plugins **do not** reference the core application. Instead, they implement a common interface from an external DLL (`PluginInterface.dll`).

### ✨ How It Works
1. **PluginInterface**: Defines a common interface (`IPlugin`) that all plugins must implement.
2. **Plugins (PluginA, PluginB, etc.)**: Implement `IPlugin` and provide their functionality.
3. **Core (FocusPluginApp)**:
   - **Loads all plugins dynamically** at startup.
   - **Monitors the active application**.
   - **Executes the appropriate plugin** based on mappings in `appsettings.json`.

---

## 🏗️ Project Structure
```
/FocusPluginApp       (🔹 Core application)
  ├── /Plugins        (📂 Stores dynamically loaded plugin DLLs)
  │      ├── PluginInterface.dll
  │      ├── PluginA.dll
  │      ├── PluginB.dll
  ├── /Core          (🧠 Contains PluginLoader & FocusDetector)
  ├── /Services      (🔄 Runs background monitoring service)
  ├── appsettings.json (⚙️ Plugin mappings & settings)
  ├── Program.cs     (🔥 Application entry point)
  
/PluginInterface      (🌎 Defines the plugin interface)
  ├── IPlugin.cs     (📝 Base interface for all plugins)
  
/PluginA             (🔌 Example plugin A)
  ├── PluginA.cs     (⚡ Implements IPlugin)
  ├── PluginA.dll    (✅ Compiled plugin DLL)
  
/PluginB             (🔌 Example plugin B)
  ├── PluginB.cs     (⚡ Implements IPlugin)
  ├── PluginB.dll    (✅ Compiled plugin DLL)
```

---

## 🔨 How to Set Up and Use

### **1️⃣ Define the Plugin Interface**
- Inside `PluginInterface`, create `IPlugin.cs`:
  ```csharp
  namespace PluginInterface
  {
      public interface IPlugin
      {
          void Execute();
      }
  }
  ```
- **Build** the `PluginInterface` project.
- **Copy `PluginInterface.dll` to `/Plugins/`** (This is the shared interface DLL that all plugins will use).

### **2️⃣ Create a New Plugin (PluginA, PluginB, etc.)**
- **Reference `PluginInterface.dll` in your plugin project.**
- Implement the interface:
  ```csharp
  using PluginInterface;
  
  namespace PluginA
  {
      public class PluginA : IPlugin
      {
          public void Execute()
          {
              Console.WriteLine("🔹 PluginA Executed Successfully!");
          }
      }
  }
  ```
- **Build the plugin** → Copy `PluginA.dll` to `/Plugins/`.

### **3️⃣ Update `appsettings.json` to Map Plugins**
Inside `FocusPluginApp`, update `appsettings.json`:
```json
{
  "PluginMappings": {
    "notepad.exe": "PluginA.dll",
    "chrome.exe": "PluginB.dll"
  },
  "PluginDirectory": "Plugins"
}
```
✅ This tells the system **which plugin should run for each application**.

### **4️⃣ Modify the Core to Load Plugins Dynamically**
Inside `PluginLoader.cs`:
```csharp
Assembly interfaceAssembly = Assembly.LoadFrom(Path.Combine(_pluginDirectory, "PluginInterface.dll"));
Type pluginInterfaceType = interfaceAssembly.GetType("PluginInterface.IPlugin");

foreach (var file in Directory.GetFiles(_pluginDirectory, "*.dll"))
{
    if (Path.GetFileName(file).Equals("PluginInterface.dll")) continue; // Skip the interface DLL

    Assembly assembly = Assembly.LoadFrom(file);
    foreach (var type in assembly.GetTypes())
    {
        if (pluginInterfaceType.IsAssignableFrom(type) && !type.IsInterface)
        {
            IPlugin pluginInstance = (IPlugin)Activator.CreateInstance(type);
            _loadedPlugins[type.Name.ToLower()] = pluginInstance;
        }
    }
}
```
✅ This **scans the `/Plugins/` folder**, **loads all DLLs dynamically**, and **registers them at runtime**.

### **5️⃣ Monitor the Active Window & Execute the Plugin**
Inside `FocusMonitorService.cs`:
```csharp
string activeApp = FocusDetector.GetActiveProcessName();
if (_pluginMappings.TryGetValue(activeApp.ToLower(), out var pluginFile))
{
    if (_loadedPlugins.TryGetValue(pluginFile.ToLower(), out var plugin))
    {
        plugin.Execute();
    }
}
```
✅ This service **continuously checks which application is in focus** and **executes the mapped plugin**.

---



## 🛠️ Running the Project
1. **Ensure all `.dll` files are inside `/Plugins/`**:
   ```
   /Plugins
      - PluginInterface.dll
      - PluginA.dll
      - PluginB.dll
   ```
2. **Run `FocusPluginApp`**.
3. **Open Notepad, Chrome, etc.** → It should trigger the correct plugin automatically.


---

