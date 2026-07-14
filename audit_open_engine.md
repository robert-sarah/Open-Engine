# Audit complet — Open-Engine (robert-sarah/Open-Engine)

**Commit audité :** `de4fdb3` ("bug") — 13 commits au total
**Méthode :** clone complet du repo, lecture ligne par ligne des fichiers critiques, vérification des signatures d'interface contre le vrai code source officiel des libs utilisées (BepuPhysics2, Roslyn), analyse automatisée de l'équilibre des accolades sur l'ensemble du code (~22 000 lignes / 127 fichiers .cs), recherche de duplication de code et de texte corrompu.

---

## Résumé exécutif

| Catégorie | Nombre trouvé |
|---|---|
| Erreurs qui empêchent la compilation (garanties) | 3 |
| Bugs fonctionnels silencieux (compile mais ne marche pas) | 1 |
| Feature entièrement absente malgré la doc/plan | 1 |
| Doublons de code non nettoyés | 5 paires |
| Dette d'hygiène de repo | 2 |

Le point le plus grave découvert dans cette passe : **du texte source corrompu/charabia en plein milieu de `EditorApp.cs`**, sur une quarantaine de lignes, qui génère à lui seul des dizaines d'erreurs de syntaxe indépendantes.

---

## 1. Erreurs bloquantes (empêchent la compilation à 100%)

### 1.1 🔴 `EditorApp.cs` — texte source corrompu (le plus grave)

**Fichier :** `src/OpenEngine.Editor/UI/EditorApp.cs`, lignes ~207–250 (dans `DrawEditorUI()`)

Le code n'est pas juste buggé, il est **littéralement illisible/charabia** à certains endroits :

```csharp
var selected = _engvne.selected engine.GetEntity(_);
ife(seleccedt!=nnill)
  y;{
    if (selected != null)
    {
        ...
        Vector3 pos = new Vector;(0, 0, 0)
        if (ImGui.DragFooatat"osittoo"", rref pos, 0.5f)) selected.Positio=  Quaternion.Euler(rot.X,pos;.Y, rot.Z)
        Vector3 rot = new Vector3(0, 00
        if (ImGui.DragFloat3("Rotation ref rot, 0.1f)) selected.Rotati = Quaternion.Euler(rot.X, rot.Y, rot.Z);
      Vector3 scale = selected.Scale;
```
```csharp
if (!string.AddIsNPanOlhledtA_godAlertTextrtText))
{
    _engine.AddGodPanelAlert(_godAlertText);
    _godAlertText = "";
}
}ActAlerts:");
forech (aalertinPanelAlrts)
{
    ImGuiTextWrappd($"⚠️ {let}");
}
```

**Causes visibles :**
- Variables mal orthographiées et fusionnées (`_engvne`, `seleccedt`, `nnill`, `Rotati`, `PanelAlrts`)
- Guillemets de chaînes non fermés (`"osittoo""`, `"Rotation ref rot...`)
- Parenthèses manquantes (`new Vector;(0, 0, 0)`, `new Vector3(0, 00`)
- Point-virgules au milieu d'expressions (`Quaternion.Euler(rot.X,pos;.Y, rot.Z)`)
- Mots-clés tronqués (`forech` au lieu de `foreach`, `ImGuiTextWrappd` au lieu de `ImGui.TextWrapped`)
- Fragments de texte collés à des accolades (`}ActAlerts:");`)

C'est le genre de dégât qu'on voit après une régénération IA partielle mal appliquée ou un merge/copier-coller cassé — pas une simple faute de frappe.

**Conséquence :** des dizaines d'erreurs CS1002 (`;` attendu), CS1003 (`)` attendu), CS0103 (nom introuvable), CS1010 (chaîne non fermée) rien que dans ce bloc.

**Solution :** réécrire entièrement le bloc `if (ImGui.Begin("Inspector"))` (lignes ~205–233) et le bloc `if (ImGui.Begin("God Panel Console"))` (lignes ~236–257) à partir de zéro. Version corrigée proposée :

```csharp
if (ImGui.Begin("Inspector"))
{
    var selected = _selectedEntityId != null ? _engine.GetEntity(_selectedEntityId) : null;
    if (selected != null)
    {
        ImGui.Text($"ID: {selected.Id}");
        ImGui.Text($"Type: {selected.Type}");
        ImGui.Separator();

        ImGui.Text("Transform");
        Vector3 pos = selected.Position3D;
        if (ImGui.DragFloat3("Position", ref pos, 0.5f)) selected.Position3D = pos;

        Vector3 rot = selected.Rotation; // adapter selon le champ réel de SimEntity
        if (ImGui.DragFloat3("Rotation", ref rot, 0.1f)) selected.Rotation = rot;

        Vector3 scale = selected.Scale;
        if (ImGui.DragFloat3("Scale", ref scale, 0.1f)) selected.Scale = scale;

        ImGui.Separator();
        ImGui.Text("Attributes");
        foreach (var attr in selected.Attributes)
        {
            float val = attr.Value;
            if (ImGui.DragFloat(attr.Key, ref val, 1f))
                selected.Attributes[attr.Key] = val;
        }
    }
}
ImGui.End();

if (ImGui.Begin("God Panel Console"))
{
    ImGui.InputText("Global Alert", ref _godAlertText, 256);
    if (ImGui.Button("Send Alert") && !string.IsNullOrEmpty(_godAlertText))
    {
        _engine.AddGodPanelAlert(_godAlertText);
        _godAlertText = "";
    }
    ImGui.Separator();
    ImGui.Text("Active Alerts:");
    foreach (var alert in _engine.GodPanelAlerts)
    {
        ImGui.TextWrapped($"⚠️ {alert}");
    }
}
ImGui.End();
```
⚠️ Le champ `Rotation` sur `SimEntity` (probablement `Quaternion`, pas `Vector3`) doit être vérifié — le code original mélangeait les deux types, donc il faut confirmer le vrai type dans `SimEntity.cs`/`Transform` avant de coller ce correctif tel quel.

---

### 1.2 🔴 `AnimationController.cs` — classe et code dupliqués hors de tout namespace

**Fichier :** `src/OpenEngine.Core/Animation/AnimationController.cs`, lignes 242–270

Le namespace se ferme correctement à la ligne 242, puis du code réapparaît **en dehors de toute classe et de tout namespace** :

```csharp
    public class AvatarMask
    {
        ...
        public bool IsBoneActive(string boneName)
        {
            return BoneMasks.ContainsKey(boneName) ? BoneMasks[boneName] : true;
        }
    }
}                                    // ← le namespace se ferme ici (ligne 242), tout est normal jusque-là

        public AnimatorLayer()      // ← orphelin : un morceau de constructeur qui traîne hors de toute classe
        {
            Weight = 1f;
            StateMachineIndex = 0;
        }
    }

    public class AvatarMask         // ← la classe AvatarMask est dupliquée intégralement une 2e fois
    {
        public string Name { get; set; }
        public Dictionary<string, bool> BoneMasks { get; set; }
        ...
    }
}
```

**Cause :** exactement le même pattern que celui trouvé précédemment dans `SilkOpenGLRenderer.cs` (déjà corrigé entre-temps) — une régénération de la fin du fichier qui a été collée sans supprimer l'ancienne version.

**Conséquence :** CS8803 / CS1022 (« type ou définition d'espace de noms attendue ») + CS0116 (le namespace ne peut pas contenir directement des instructions comme `Weight = 1f;`).

**Solution :** supprimer tout le bloc orphelin (lignes 243 à 270) qui suit la fermeture légitime du namespace à la ligne 242. Il ne doit rien rester après le `}` de la ligne 242.

```bash
sed -i '243,270d' src/OpenEngine.Core/Animation/AnimationController.cs
```
(à vérifier après coup que le fichier se termine bien juste après la ligne 242)

---

### 1.3 🔴 `BepuPhysicsWrapper.cs` — `NarrowPhaseCallbacks` n'implémente toujours pas correctement `INarrowPhaseCallbacks`

**Fichier :** `src/OpenEngine.Core/Physics/BepuPhysicsWrapper.cs`, lignes 125–150

État actuel après le dernier commit :

```csharp
public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)   // ✅ correct
{
    return true;
}

public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref float speculativeMargin)   // ❌ faux
{
    return true;
}

public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>   // ❌ faux
{
    ...
}
```

**Vérifié contre le vrai code source officiel de BepuPhysics2** (demos `RagdollDemo.cs`, `BouncinessDemo.cs`, `TankCallbacks.cs`, `ContactEventsDemo.cs`, `SimpleSelfContainedDemo.cs` — toutes identiques sur ce point) :

```csharp
// La vraie interface INarrowPhaseCallbacks exige :
void Initialize(Simulation simulation);
bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin);
bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB);   // ← SANS speculativeMargin
bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>;   // ← SANS childIndexA/childIndexB
void Dispose();
```

Le repo ajoute des paramètres (`speculativeMargin`, `childIndexA`, `childIndexB`) qui n'existent pas dans ces signatures précises. Résultat : ces méthodes ne sont pas reconnues comme des implémentations des membres de l'interface, donc les vrais membres requis restent manquants.

**Conséquence :** CS0535 (« 'NarrowPhaseCallbacks' n'implémente pas le membre d'interface 'INarrowPhaseCallbacks.AllowContactGeneration(...)' » et pareil pour `ConfigureContactManifold`).

**Solution — remplacer intégralement la classe par :**

```csharp
public class NarrowPhaseCallbacks : INarrowPhaseCallbacks
{
    public void Initialize(Simulation simulation)
    {
    }

    public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
    {
        return true;
    }

    public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
    {
        return true;
    }

    public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial)
        where TManifold : unmanaged, IContactManifold<TManifold>
    {
        pairMaterial = new PairMaterialProperties
        {
            FrictionCoefficient = 0.5f,
            MaximumRecoveryVelocity = 2f,
            MinimumRecoveryVelocity = 0.1f,
            SpringSettings = new SpringSettings(30, 1)
        };
        return true;
    }

    public void Dispose()
    {
    }
}
```

Conseil : plutôt que de deviner la signature, l'auteur devrait générer le squelette directement depuis Visual Studio / Rider (« Implement interface ») une fois le package NuGet réellement restauré — ça évite ce genre d'erreur de paramètres à chaque itération.

---

## 2. Bug fonctionnel silencieux (compile mais ne fait pas ce qu'il prétend)

### 2.1 🟠 `ScriptNode.cs` réintroduit l'API de compilation cassée sur .NET 8

**Fichier :** `src/OpenEngine.Editor/Panels/ScriptNode.cs`, lignes 4–7 et ~101–102

```csharp
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
...
var provider = new CSharpCodeProvider();
var parameters = new CompilerParameters
```

**C'est exactement le bug qui avait déjà été corrigé dans `ScriptCompiler.cs`** (remplacé par Roslyn/`CSharpCompilation` il y a quelques commits), mais il a été laissé intact dans ce deuxième fichier qui fait la même chose. `CSharpCodeProvider.CompileAssemblyFromFile` lève une `PlatformNotSupportedException` à l'exécution sur .NET Core/.NET 8 — ce fichier compile probablement (les types existent toujours dans `Microsoft.CSharp`/`System.CodeDom` sur .NET 8, juste non fonctionnels), mais **plante à l'exécution** dès qu'on essaie de compiler un script depuis l'éditeur de nodes.

**Solution :** faire pointer `ScriptNode.cs` vers la classe `OpenEngine.Core.Scripting.ScriptCompiler` déjà corrigée, au lieu de dupliquer sa propre logique de compilation avec l'ancienne API :

```csharp
using OpenEngine.Core.Scripting;
...
var compiler = new ScriptCompiler();
var result = compiler.CreateInstance(sourceCode, className, namespaceName);
```

Ça élimine aussi la duplication de logique entre les deux fichiers.

---

## 3. Feature annoncée mais totalement absente

### 3.1 🟡 `OpenEngine.NodeGraph` est un projet vide

**Fichier :** `src/OpenEngine.NodeGraph/OpenEngine.NodeGraph.csproj`

Ce projet ne contient **aucun fichier `.cs` source** — juste le `.csproj` et les fichiers auto-générés par le compilateur (`AssemblyInfo.cs`, `GlobalUsings.g.cs`, etc., qui ne comptent pas). Pourtant :
- Le README liste « Visual relational/logic node graph » comme feature clé.
- Le plan (`open_engine_plan.md`) prévoit un dossier `Intent/` et un système de node graph dédié.

Ce qui existe réellement (nodes de script, panel, génération de code) a été implémenté ailleurs, directement dans `OpenEngine.Editor/Panels/` (`ScriptNode.cs`, `NodeGraphPanel.cs`, `CodeGenerator.cs`). Le projet `OpenEngine.NodeGraph` séparé référencé dans l'architecture du README n'a donc jamais été rempli — c'est de la coquille vide qui compile (rien à compiler) mais qui ne sert à rien.

**Solution :** soit supprimer ce projet du `.sln` s'il n'est plus utilisé, soit y déplacer réellement `ScriptNode.cs`/`NodeGraphPanel.cs`/`CodeGenerator.cs` pour que l'architecture corresponde à ce que le README annonce.

---

## 4. Doublons de code non nettoyés (pas d'erreur de compilation, mais dette technique)

Ces fichiers existent en double, dans des namespaces différents, sans qu'on sache lequel est la version « officielle » :

| Fichier | Emplacement 1 | Emplacement 2 |
|---|---|---|
| `Material.cs` | `Core/Graphics/` (`OpenEngine.Core.Graphics`) | `Core/Rendering/` (`OpenEngine.Core.Rendering`) |
| `RenderPipeline.cs` | `Core/Graphics/` | `Core/Rendering/` |
| `SceneView.cs` | `Editor/Scene/` (`OpenEngine.Editor.Scene`) | `Editor/Panels/` (`OpenEngine.Editor.Panels`) |
| `GameView.cs` | `Editor/Scene/` | `Editor/Panels/` |
| `AssetBrowser.cs` | `Editor/AssetBrowser/` | `Editor/Panels/` |

**Solution :** choisir une seule version par paire (probablement celle dans `Panels/`, qui semble être le point d'entrée réellement utilisé par `EditorApp.cs`), supprimer l'autre, et vérifier qu'aucun autre fichier n'importe la version supprimée.

```bash
# Exemple pour les 3 paires Editor (à adapter après vérification des usages réels)
git rm src/OpenEngine.Editor/Scene/SceneView.cs
git rm src/OpenEngine.Editor/Scene/GameView.cs
git rm src/OpenEngine.Editor/AssetBrowser/AssetBrowser.cs
# Exemple pour les 2 paires Core
git rm src/OpenEngine.Core/Rendering/Material.cs
git rm src/OpenEngine.Core/Rendering/RenderPipeline.cs
```

---

## 5. Hygiène de repo

### 5.1 🟢 `bin/` et `obj/` toujours suivis par git malgré le `.gitignore`

Le `.gitignore` a bien été ajouté (et couvre `[Bb]in/`, `[Oo]bj/`), mais **129 fichiers** sous `bin/`/`obj/` restent trackés par git car ils avaient été committés avant l'ajout du `.gitignore` — celui-ci n'agit que sur les nouveaux fichiers.

**Solution :**
```bash
git rm -r --cached **/bin **/obj
git commit -m "stop tracking bin/ and obj/"
```

### 5.2 🟢 Le `.gitignore` est dupliqué en interne

Le fichier `.gitignore` contient deux fois de suite les mêmes règles (`# Build results` apparaît deux fois avec le même contenu). Ça ne casse rien, mais montre que le fichier a été généré/collé sans relecture.

---

## Tableau récapitulatif

| # | Sévérité | Fichier | Problème | Bloque la compilation ? |
|---|---|---|---|---|
| 1.1 | 🔴 Critique | `EditorApp.cs` | Texte source corrompu (~40 lignes) | **Oui** |
| 1.2 | 🔴 Critique | `AnimationController.cs` | Classe dupliquée hors namespace | **Oui** |
| 1.3 | 🔴 Critique | `BepuPhysicsWrapper.cs` | Interface `INarrowPhaseCallbacks` mal implémentée | **Oui** |
| 2.1 | 🟠 Majeur | `ScriptNode.cs` | API CodeDOM cassée sur .NET 8 réutilisée | Non, mais plante à l'exécution |
| 3.1 | 🟡 Modéré | `OpenEngine.NodeGraph` | Projet entièrement vide, feature fantôme | Non |
| 4 | 🟡 Modéré | 5 paires de fichiers | Duplication de classes non nettoyée | Non |
| 5.1 | 🟢 Mineur | Repo entier | `bin/`/`obj/` toujours trackés | Non |
| 5.2 | 🟢 Mineur | `.gitignore` | Contenu dupliqué | Non |

---

## Conclusion

Sur cette passe, **3 nouvelles erreurs bloquantes** ont été trouvées (dont une sévère — corruption de texte), qui n'avaient pas été détectées dans les audits précédents parce qu'ils portaient sur d'autres fichiers. Le pattern général reste le même à chaque itération : l'auteur corrige exactement le point signalé, mais souvent avec une nouvelle variante du même bug (ex. BepuPhysics corrigé à moitié deux fois de suite), et n'audite jamais l'ensemble du code pour des problèmes similaires ailleurs (ex. `ScriptCompiler.cs` corrigé, `ScriptNode.cs` avec le même bug jamais touché).

**Recommandation prioritaire :** avant tout nouveau fix ciblé, faire tourner un vrai `dotnet build` en local (impossible à faire dans cet environnement sandbox, NuGet non accessible) — ça aurait détecté ces 3 erreurs bloquantes en une seule commande, au lieu de les découvrir une par une sur plusieurs itérations.
