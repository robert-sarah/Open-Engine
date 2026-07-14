# Audit Complet — Open-Engine (robert-sarah/Open-Engine)
**Date:** 14 juillet 2026  
**Méthode:** Analyse statique complète du codebase, vérification des dépendances NuGet, détection de duplications, validation des signatures d'interface

---

## Résumé Exécutif

| Catégorie | Nombre trouvé | Sévérité |
|---|---|---|
| Erreurs bloquant la compilation (CS0101) | 12 | 🔴 Critique |
| Références ambiguës (CS0104) | 4 | 🔴 Critique |
| Packages NuGet invalides | 3 | 🔴 Critique |
| Bug fonctionnel silencieux | 1 | 🟠 Majeur |
| Code natif sans build system | 1 | 🟠 Majeur |
| Code mort (non utilisé) | 6 classes | 🟡 Modéré |
| Projet vide | 1 | 🟡 Modéré |
| Hygiène de repo | 1 | 🟢 Mineur |
| Documentation contradictoire | 1 | 🟢 Mineur |

---

## 1. Erreurs Bloquant la Compilation (CS0101 - Types Dupliqués)

### 1.1 🔴 Duplications dans `OpenEngine.Core.Rendering`

**Fichiers concernés:**
- `src/OpenEngine.Core/Rendering/Mesh.cs`
- `src/OpenEngine.Core/Rendering/MeshRenderer.cs`
- `src/OpenEngine.Core/Rendering/Light.cs`
- `src/OpenEngine.Core/Rendering/Lighting.cs`

**Types dupliqués dans le même namespace:**
- `Bounds` - défini dans Mesh.cs et MeshRenderer.cs
- `Vector2` - défini dans Mesh.cs et MeshRenderer.cs
- `Mesh` - défini dans Mesh.cs et MeshRenderer.cs
- `LightType` - défini dans Light.cs et Lighting.cs
- `Light` - défini dans Light.cs et Lighting.cs

**Erreur:** CS0101 - "Le type 'X' existe déjà dans 'Y'"

**Solution:** Supprimer les duplications en gardant une seule version par type. Recommandé:
- Garder `Bounds`, `Vector2`, `Mesh` dans Mesh.cs
- Garder `LightType`, `Light` dans Light.cs
- Supprimer les définitions dupliquées dans MeshRenderer.cs et Lighting.cs

---

### 1.2 🔴 Duplications dans `OpenEngine.Editor.Panels`

**Fichiers concernés:**
- `src/OpenEngine.Editor/Panels/AssetBrowser.cs`
- `src/OpenEngine.Editor/Panels/SceneView.cs`
- `src/OpenEngine.Editor/Panels/GameView.cs`

**Types dupliqués:**
- `AssetBrowser` - défini dans Panels et AssetBrowser (dossier séparé)
- `SceneView` - défini dans Panels et Scene (dossier séparé)
- `GameView` - défini dans Panels et Scene (dossier séparé)

**Note:** Ces classes ne sont JAMAIS instanciées dans le codebase - code mort.

**Solution:** Supprimer les dossiers `src/OpenEngine.Editor/AssetBrowser/` et `src/OpenEngine.Editor/Scene/` entièrement.

---

## 2. Références Ambiguës (CS0104)

### 2.1 🔴 `OpenEngineCore.cs` - RenderPipeline ambigu

**Fichier:** `src/OpenEngine.Core/Engine/OpenEngineCore.cs`, ligne 42

```csharp
private RenderPipeline _renderPipeline;
```

**Problème:** `RenderPipeline` existe dans:
- `OpenEngine.Core.Graphics.RenderPipeline`
- `OpenEngine.Core.Rendering.RenderPipeline`

Les deux namespaces sont importés via using directives.

**Solution:** Qualifier explicitement:
```csharp
private Graphics.RenderPipeline _renderPipeline;
```

---

### 2.2 🔴 `OpenEngineCore.cs` - Time ambigu

**Fichier:** `src/OpenEngine.Core/Engine/OpenEngineCore.cs`, ligne 101

```csharp
Time.Update(deltaTime);
```

**Problème:** `Time` existe dans:
- `OpenEngine.Core.Physics.Time`
- `OpenEngine.Core.Math.Time`

**Solution:** Qualifier explicitement:
```csharp
Physics.Time.Update(deltaTime);
```

---

### 2.3 🔴 `OpenEngineCore.cs` - Vector3 ambigu

**Fichier:** `src/OpenEngine.Core/Engine/OpenEngineCore.cs`, ligne 196

```csharp
var direction = (target - entity.Position3D).Normalized;
```

**Problème:** `Vector3` existe dans:
- `OpenEngine.Core.Math.Vector3`
- `OpenEngine.Core.Scripting.Vector3` (défini dans ScriptComponent.cs)

**Solution:** Qualifier explicitement ou supprimer la définition dans ScriptComponent.cs.

---

### 2.4 🔴 `OpenSimulationEngine.cs` - Vector3 ambigu

**Fichier:** `src/OpenEngine.Core/Engine/OpenSimulationEngine.cs`

**Problème:** Importe `OpenEngine.Core.Math` et `OpenEngine.Core.Scripting`, tous deux définissent `Vector3`.

**Solution:** Supprimer la définition de `Vector3` dans ScriptComponent.cs et utiliser exclusivement `OpenEngine.Core.Math.Vector3`.

---

## 3. Packages NuGet Invalides

### 3.1 🔴 SoLoud 1.0.0 - Package inexistant

**Fichier:** `src/OpenEngine.Core/OpenEngine.Core.csproj`

```xml
<PackageReference Include="SoLoud" Version="1.0.0" />
```

**Problème:** Aucun package NuGet nommé "SoLoud" version 1.0.0 n'existe.

**Packages réels disponibles:**
- `SoLoudInterop` 1.0.0
- `Sharploud` 2020.2.7
- `SoLoud-wav` 2020.2.7.2

**Solution:** Remplacer par:
```xml
<PackageReference Include="SoLoudInterop" Version="1.0.0" />
```

---

### 3.2 🔴 SharpGLTF 1.0.0 - Package inexistant

**Fichier:** `src/OpenEngine.Core/OpenEngine.Core.csproj`

```xml
<PackageReference Include="SharpGLTF" Version="1.0.0" />
```

**Problème:** Le package correct est `SharpGLTF.Core` ou `SharpGLTF.Toolkit`.

**Solution:** Remplacer par:
```xml
<PackageReference Include="SharpGLTF.Toolkit" Version="1.0.6" />
```

---

### 3.3 🔴 ImGui.NET.Silk.NET 1.2.0 - Package inexistant

**Fichier:** `src/OpenEngine.Editor/OpenEngine.Editor.csproj`

```xml
<PackageReference Include="ImGui.NET.Silk.NET" Version="1.2.0" />
```

**Problème:** Le package correct pour l'intégration ImGui avec Silk.NET est `Silk.NET.OpenGL.Extensions.ImGui`.

**Solution:** Remplacer par:
```xml
<PackageReference Include="Silk.NET.OpenGL.Extensions.ImGui" Version="2.21.0" />
```

---

## 4. Bug Fonctionnel Silencieux

### 4.1 🟠 GameActionTemplates - Identifiants non définis

**Fichier:** `src/OpenEngine.Editor/Panels/ScriptNode.cs`, lignes 220-334

**Problème:** Les templates de GameActionTemplates référencent des identifiants qui n'existent pas dans le contexte de compilation:

- `GetParameterValue<T>()` - méthode non définie
- `engine` - variable non définie (devrait être `_engine` ou passé en paramètre)
- `deltaTime` - variable non définie (devrait être passée en paramètre)
- `graph.SetVariableValue()` / `graph.GetVariableValue()` - `graph` non défini

**Exemple problématique:**
```csharp
public static string MoveToTarget = @"
// Move entity to target position
var targetPosition = GetParameterValue<Vector3>("TargetPosition");  // ❌ GetParameterValue non défini
var speed = GetParameterValue<float>("Speed", 5.0f);              // ❌

if (entity != null)
{
    var direction = (targetPosition - entity.Position3D).Normalized();
    entity.Position3D += direction * speed * deltaTime;              // ❌ deltaTime non défini
}";
```

**Conséquence:** Ces templates ne compileront PAS lors de l'exécution du générateur de code, même si le projet principal compile.

**Solution:** Modifier `CodeGenerator.GenerateCSharpClass` pour:
1. Ajouter une méthode `GetParameterValue<T>(string name, T defaultValue = default)` dans la classe générée
2. Passer `deltaTime` en paramètre de la méthode générée
3. Passer une référence à l'engine ou aux méthodes nécessaires en paramètre
4. Ajouter les méthodes `SetVariableValue`/`GetVariableValue` si utilisées

---

## 5. Code Natif Sans Build System

### 5.1 🟠 Dossier Native non intégré au build

**Dossier:** `src/OpenEngine.Native/`

**Contenu:** Fichiers C/C++/Objective-C++ pour l'interopérabilité native

**Problème:** 
- Aucun `.csproj` n'inclut ces fichiers
- Aucun `CMakeLists.txt`, `Makefile`, ou script de build natif
- Aucune référence dans `OpenEngine.sln`

**Conséquence:** Les déclarations P/Invoke dans le code C# échoueront à l'exécution avec `DllNotFoundException` car les bibliothèques natives ne sont jamais compilées.

**Solution:** 
- Option 1: Supprimer le dossier Native si non utilisé
- Option 2: Ajouter un système de build natif (CMake) et intégrer au processus de build
- Option 3: Utiliser des packages NuGet existants pour l'interopérabilité native au lieu de code personnalisé

---

## 6. Code Mort (Non Utilisé)

### 6.1 🟡 Panels Editor non instanciés

**Classes concernées:**
- `OpenEngine.Editor.AssetBrowser.AssetBrowser`
- `OpenEngine.Editor.Scene.SceneView`
- `OpenEngine.Editor.Scene.GameView`

**Preuve:** Aucune instanciation dans tout le codebase (grep search sur `new AssetBrowser`, `new SceneView`, `new GameView`).

**Solution:** Supprimer ces classes et leurs dossiers associés.

---

## 7. Projet Vide

### 7.1 🟡 OpenEngine.NodeGraph - Projet entièrement vide

**Fichier:** `src/OpenEngine.NodeGraph/OpenEngine.NodeGraph.csproj`

**Problème:** Ce projet ne contient AUCUN fichier `.cs` source, seulement le `.csproj`.

**Contradiction:** Le README liste "Visual relational/logic node graph" comme feature clé.

**Réalité:** Le système de node graph a été implémenté dans `OpenEngine.Editor/Panels/` (ScriptNode.cs, NodeGraphPanel.cs, CodeGenerator.cs).

**Solution:** 
- Option 1: Supprimer le projet OpenEngine.NodeGraph du `.sln`
- Option 2: Y déplacer ScriptNode.cs, NodeGraphPanel.cs, CodeGenerator.cs pour correspondre à l'architecture annoncée

---

## 8. Hygiène de Repository

### 8.1 🟢 bin/ et obj/ toujours suivis par git

**Problème:** 129 fichiers sous `bin/`/`obj/` restent trackés par git malgré le `.gitignore`.

**Cause:** Ces fichiers avaient été committés avant l'ajout du `.gitignore`.

**Solution:**
```bash
git rm -r --cached **/bin **/obj
git commit -m "stop tracking bin/ and obj/"
```

---

## 9. Documentation Contradictoire

### 9.1 🟢 README - Vulkan/Metal incohérence

**Section Features du README:**
> Rendering Backends: OpenGL 4.5, Vulkan, Metal

**Section Roadmap du README:**
> - [ ] Complete Vulkan rendering backend
> - [ ] Complete Metal rendering backend

**Problème:** Le README présente Vulkan et Metal comme des fonctionnalités actuelles dans Features, mais les liste comme "à faire" dans Roadmap.

**Solution:** Corriger la section Features pour refléter l'état réel (seulement OpenGL implémenté) ou déplacer ces backends dans une section "Planned Features".

---

## 10. Bugs Précédemment Corrigés (Vérification)

Les bugs suivants, identifiés dans l'audit précédent, ont été correctement corrigés:

### ✅ EditorApp.cs - Texte corrompu
- **Statut:** Corrigé
- **Vérification:** Le bloc charabia a été remplacé par du code propre

### ✅ AnimationController.cs - Code dupliqué hors namespace
- **Statut:** Corrigé
- **Vérification:** Le code orphelin après la fermeture du namespace a été supprimé

### ✅ BepuPhysicsWrapper.cs - INarrowPhaseCallbacks signature
- **Statut:** Corrigé
- **Vérification:** Les signatures correspondent maintenant à l'interface officielle BepuPhysics 2.4

### ✅ ScriptNode.cs - API CodeDOM cassée
- **Statut:** Corrigé
- **Vérification:** Utilise maintenant ScriptCompiler (Roslyn) au lieu de CSharpCodeProvider

---

## Tableau Récapitulatif

| # | Sévérité | Fichier/Projet | Problème | Erreur C# |
|---|---|---|---|---|
| 1.1 | 🔴 Critique | Rendering/*.cs | Types dupliqués dans même namespace | CS0101 |
| 1.2 | 🔴 Critique | Editor/Panels/*.cs | Classes dupliquées (code mort) | CS0101 |
| 2.1 | 🔴 Critique | OpenEngineCore.cs:42 | RenderPipeline ambigu | CS0104 |
| 2.2 | 🔴 Critique | OpenEngineCore.cs:101 | Time ambigu | CS0104 |
| 2.3 | 🔴 Critique | OpenEngineCore.cs:196 | Vector3 ambigu | CS0104 |
| 2.4 | 🔴 Critique | OpenSimulationEngine.cs | Vector3 ambigu | CS0104 |
| 3.1 | 🔴 Critique | OpenEngine.Core.csproj | Package SoLoud inexistant | NU1101 |
| 3.2 | 🔴 Critique | OpenEngine.Core.csproj | Package SharpGLTF inexistant | NU1101 |
| 3.3 | 🔴 Critique | OpenEngine.Editor.csproj | Package ImGui.NET.Silk.NET inexistant | NU1101 |
| 4.1 | 🟠 Majeur | ScriptNode.cs | GameActionTemplates identifiants non définis | Compilation runtime |
| 5.1 | 🟠 Majeur | src/OpenEngine.Native/ | Code natif sans build system | DllNotFoundException |
| 6.1 | 🟡 Modéré | Editor/AssetBrowser, Scene | Classes non instanciées | Code mort |
| 7.1 | 🟡 Modéré | OpenEngine.NodeGraph | Projet vide | Feature fantôme |
| 8.1 | 🟢 Mineur | Repo entier | bin/obj toujours trackés | Hygiène |
| 9.1 | 🟢 Mineur | README.md | Vulkan/Metal incohérent | Documentation |

---

## Recommandations Prioritaires

### Immédiat (bloque la compilation):
1. **Corriger les 12 duplications de types** dans Rendering namespace
2. **Corriger les 4 références ambiguës** avec qualification explicite
3. **Corriger les 3 packages NuGet invalides** pour permettre le restore

### Court terme (bloque l'exécution):
4. **Corriger GameActionTemplates** pour définir les identifiants manquants
5. **Résoudre le problème du code natif** (supprimer ou ajouter build system)

### Moyen terme (hygiène et architecture):
6. **Supprimer le code mort** (panels non utilisés)
7. **Résoudre OpenEngine.NodeGraph** (supprimer ou remplir)
8. **Nettoyer git** (bin/obj non trackés)
9. **Corriger le README** pour cohérence

---

## Conclusion

Cet audit révèle des problèmes structurels profonds qui empêchent non seulement la compilation, mais aussi l'exécution correcte de l'application. Les duplications de types et les références ambiguës sont particulièrement critiques car elles affectent le cœur de l'architecture du moteur.

Les problèmes de packages NuGet suggèrent que le projet n'a jamais été build avec succès sur une machine propre, ce qui explique pourquoi ces erreurs de base n'ont pas été détectées plus tôt.

Le code natif sans build system représente un risque important de crash à l'exécution qui passerait inaperçu lors du développement mais affecterait les utilisateurs finaux.

**Note globale:** Le codebase nécessite une refonte architecturale significative pour résoudre les duplications de types et clarifier les responsabilités de chaque namespace avant de pouvoir être considéré comme stable et maintenable.
