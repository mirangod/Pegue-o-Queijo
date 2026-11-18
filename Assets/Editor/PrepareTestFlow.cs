using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class PrepareTestFlow
{
    private const string GameplayFolder = "Assets/Gameplay";
    private const string PixelSpritePath = GameplayFolder + "/Pixel.png";
    private const string FinishScenePath = "Assets/Scenes/Gameplay - Chegada.unity";
    private const string CheeseScenePath = "Assets/Scenes/Gameplay - Queijo.unity";
    private static Sprite pixelSprite;
    private static Font uiFont;

    [MenuItem("Pegue o Queijo/Preparar fluxo jogável")]
    public static void Generate()
    {
        EnsureFolder(GameplayFolder);
        pixelSprite = CreatePixelSprite();
        uiFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Thaleah_PixelFont/Materials/ThaleahFat_TTF.ttf");
        if (uiFont == null)
            uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        TunePlayerPrefab();
        CreateGameplayScene(FinishScenePath, false);
        CreateGameplayScene(CheeseScenePath, true);
        ConfigureWorldMap();
        ConfigureMainMenu();
        ConfigureBuildScenes();

        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Fluxo jogável preparado: Menu > Mapa > duas fases > vitória/derrota > mapa.");
    }

    private static void TunePlayerPrefab()
    {
        const string prefabPath = "Assets/Characters/Playables/Felix.prefab";
        GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
        Rat rat = prefabContents.GetComponent<Rat>();
        if (rat == null)
            throw new System.InvalidOperationException("O prefab Felix precisa ter um componente Rat.");

        SerializedObject serializedRat = new SerializedObject(rat);
        SetFloat(serializedRat, "speed", 7f);
        SetFloat(serializedRat, "jumpForce", 7.5f);
        SetFloat(serializedRat, "itemMassMultiplier", 2f);
        SetFloat(serializedRat, "itemJumpMultiplier", 0.65f);
        SetFloat(serializedRat, "itemJumpHoldMultiplier", 0.65f);
        SetFloat(serializedRat, "itemAcceleration", 40f);
        SetFloat(serializedRat, "itemTurnAcceleration", 20f);
        SetFloat(serializedRat, "itemStopAcceleration", 30f);
        SetInt(serializedRat, "maxHealth", 3);
        SetFloat(serializedRat, "damageCooldown", 1f);
        serializedRat.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabContents);
    }

    private static void CreateGameplayScene(string scenePath, bool requiresCheese)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera();

        CreateGround("Ground - west", new Vector2(-6.25f, -3.6f), new Vector2(9.5f, 0.8f));
        CreateGround("Ground - east", new Vector2(6.25f, -3.6f), new Vector2(9.5f, 0.8f));
        CreateGround("Floating platform - lower", new Vector2(-0.1f, -1.05f), new Vector2(2.8f, 0.38f));
        CreateGround("Floating platform - upper", new Vector2(3.2f, 0.55f), new Vector2(2.6f, 0.38f));

        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Characters/Playables/Felix.prefab");
        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
        player.name = "Felix - Player";
        player.transform.position = new Vector3(-9.1f, -2.35f, 0f);

        CreateBouncePad("Spring pad", new Vector2(-6.1f, -2.93f));
        CreateBreakable("Breakable crate", new Vector2(-2.15f, -2.55f));
        CreateEnemy(scene, new Vector2(4.4f, -2.55f));
        CreateHazard(new Vector2(7.0f, -2.96f), new Vector2(1.25f, 0.25f));

        if (requiresCheese)
        {
            CreateHome(new Vector2(-9.1f, -1.75f));
            CreateCheese(new Vector2(9.0f, -2.55f));
            CreateFinish(new Vector2(9.7f, -1.75f)); // Wrong goal for this objective; it must not complete the phase.
        }
        else
        {
            CreateFinish(new Vector2(9.7f, -1.75f));
        }

        CreateCanvas(player.GetComponent<Rat>(), requiresCheese);
        EditorSceneManager.SaveScene(scene, scenePath);
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6f;
        camera.backgroundColor = new Color(0.12f, 0.17f, 0.27f);
        camera.clearFlags = CameraClearFlags.SolidColor;
    }

    private static void CreateGround(string name, Vector2 position, Vector2 size)
    {
        GameObject ground = CreateBlock(name, position, size, new Color(0.40f, 0.24f, 0.14f), true, -2);
        ground.GetComponent<BoxCollider2D>().sharedMaterial = null;
        CreateBlock(name + " - grass", new Vector2(position.x, position.y + size.y * 0.43f), new Vector2(size.x, 0.14f), new Color(0.30f, 0.68f, 0.25f), false, -1);
    }

    private static GameObject CreateBlock(string name, Vector2 position, Vector2 size, Color color, bool addCollider, int sortingOrder = 0)
    {
        GameObject block = new GameObject(name);
        block.transform.position = new Vector3(position.x, position.y, 0f);
        block.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = pixelSprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        if (addCollider)
        {
            BoxCollider2D collider = block.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
        }
        return block;
    }

    private static void CreateBouncePad(string name, Vector2 position)
    {
        GameObject pad = CreateBlock(name, position, new Vector2(1.25f, 0.34f), new Color(0.25f, 0.82f, 0.9f), true, 1);
        pad.tag = "Bounceable";
    }

    private static void CreateBreakable(string name, Vector2 position)
    {
        GameObject crate = CreateBlock(name, position, new Vector2(0.95f, 0.95f), new Color(0.70f, 0.36f, 0.13f), true, 2);
        crate.tag = "Bounceable";
        SpriteRenderer intactRenderer = crate.GetComponent<SpriteRenderer>();
        BreakableObject breakable = crate.AddComponent<BreakableObject>();
        GameObject cargo = CreateBlock("Hidden carryable item", position, new Vector2(0.48f, 0.48f), new Color(0.92f, 0.78f, 0.20f), true, 3);
        cargo.transform.SetParent(crate.transform, true);
        cargo.AddComponent<CarryableInteractable>();

        SerializedObject serializedBreakable = new SerializedObject(breakable);
        serializedBreakable.FindProperty("intactVisual").objectReferenceValue = intactRenderer;
        SerializedProperty contents = serializedBreakable.FindProperty("revealedContents");
        contents.arraySize = 1;
        contents.GetArrayElementAtIndex(0).objectReferenceValue = cargo;
        serializedBreakable.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateEnemy(Scene scene, Vector2 position)
    {
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Characters/Playables/Bandit.prefab");
        GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab, scene);
        enemy.name = "Enemy - Bounceable";
        enemy.tag = "Bounceable";
        enemy.transform.position = new Vector3(position.x, position.y, 0f);
        enemy.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

        Rigidbody2D body = enemy.GetComponent<Rigidbody2D>();
        if (body == null)
            body = enemy.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Static;
        body.gravityScale = 0f;

        Collider2D existingCollider = enemy.GetComponent<Collider2D>();
        if (existingCollider == null)
        {
            BoxCollider2D box = enemy.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1f, 1.2f);
        }

        SpriteRenderer renderer = enemy.GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.sortingOrder = 2;

        Enemy enemyBehaviour = enemy.AddComponent<Enemy>();
        SerializedObject serializedEnemy = new SerializedObject(enemyBehaviour);
        SetInt(serializedEnemy, "health", 2);
        SetInt(serializedEnemy, "damage", 1);
        serializedEnemy.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateHazard(Vector2 position, Vector2 size)
    {
        GameObject hazard = CreateBlock("Damage hazard", position, size, new Color(0.92f, 0.18f, 0.22f), true, 2);
        BoxCollider2D collider = hazard.GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
        DamageDealer damage = hazard.AddComponent<DamageDealer>();
        SerializedObject serializedDamage = new SerializedObject(damage);
        SetInt(serializedDamage, "damage", 1);
        serializedDamage.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateCheese(Vector2 position)
    {
        GameObject cheese = CreateBlock("Cheese - carryable", position, new Vector2(0.65f, 0.65f), Color.white, true, 3);
        Sprite cheeseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Main Menu/cheese.png");
        if (cheeseSprite != null)
            cheese.GetComponent<SpriteRenderer>().sprite = cheeseSprite;
        BoxCollider2D collider = cheese.GetComponent<BoxCollider2D>();
        collider.size = new Vector2(1.2f, 1.2f);
        cheese.AddComponent<CheeseInteractable>();
    }

    private static void CreateFinish(Vector2 position)
    {
        CreateBlock("Finish - flag pole", new Vector2(position.x + 0.7f, position.y + 0.2f), new Vector2(0.14f, 2.7f), new Color(0.9f, 0.9f, 0.85f), false, 0);
        CreateBlock("Finish - flag", new Vector2(position.x + 0.1f, position.y + 1.15f), new Vector2(1.1f, 0.72f), new Color(1f, 0.82f, 0.18f), false, 1);
        GameObject trigger = CreateBlock("Finish Goal Trigger", position, new Vector2(2.2f, 3.2f), Color.clear, true, -5);
        trigger.GetComponent<SpriteRenderer>().enabled = false;
        trigger.GetComponent<BoxCollider2D>().isTrigger = true;
        LevelGoalTrigger goal = trigger.AddComponent<LevelGoalTrigger>();
        SerializedObject serializedGoal = new SerializedObject(goal);
        serializedGoal.FindProperty("point").enumValueIndex = (int)LevelGoalPoint.Finish;
        serializedGoal.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateHome(Vector2 position)
    {
        CreateBlock("Home - burrow shadow", new Vector2(position.x - 0.25f, position.y - 0.42f), new Vector2(1.6f, 0.75f), new Color(0.16f, 0.09f, 0.08f), false, 0);
        CreateBlock("Home - grass roof", new Vector2(position.x - 0.25f, position.y + 0.02f), new Vector2(2f, 0.24f), new Color(0.30f, 0.68f, 0.25f), false, 1);
        GameObject trigger = CreateBlock("Home Goal Trigger", position, new Vector2(2.4f, 3.2f), Color.clear, true, -5);
        trigger.GetComponent<SpriteRenderer>().enabled = false;
        trigger.GetComponent<BoxCollider2D>().isTrigger = true;
        LevelGoalTrigger goal = trigger.AddComponent<LevelGoalTrigger>();
        SerializedObject serializedGoal = new SerializedObject(goal);
        serializedGoal.FindProperty("point").enumValueIndex = (int)LevelGoalPoint.Home;
        serializedGoal.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateCanvas(Rat rat, bool requiresCheese)
    {
        GameObject canvasObject = new GameObject("Gameplay Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystem.transform.SetAsLastSibling();

        Text health = CreateText(canvas.transform, "Health - label", "Vidas: 3/3", new Vector2(0f, 1f), new Vector2(42f, -32f), new Vector2(300f, 64f), 48, TextAnchor.UpperLeft, Color.white);
        RatHealthDisplay healthDisplay = health.gameObject.AddComponent<RatHealthDisplay>();
        SerializedObject serializedDisplay = new SerializedObject(healthDisplay);
        serializedDisplay.FindProperty("rat").objectReferenceValue = rat;
        serializedDisplay.FindProperty("label").objectReferenceValue = health;
        serializedDisplay.ApplyModifiedPropertiesWithoutUndo();

        string instruction = requiresCheese ? "PEGUE O QUEIJO E VOLTE À TOCA" : "CHEGUE ATÉ A BANDEIRA";
        CreateText(canvas.transform, "Objective - label", instruction, new Vector2(1f, 1f), new Vector2(-42f, -32f), new Vector2(760f, 64f), 42, TextAnchor.UpperRight, new Color(1f, 0.91f, 0.57f));

        CreateButton(canvas.transform, "Left Button", "ESQ", new Vector2(0f, 0f), new Vector2(150f, 145f), new Vector2(220f, 190f), new Color(0.15f, 0.24f, 0.38f, 0.78f), true);
        CreateButton(canvas.transform, "Right Button", "DIR", new Vector2(0f, 0f), new Vector2(410f, 145f), new Vector2(220f, 190f), new Color(0.15f, 0.24f, 0.38f, 0.78f), true);
        CreateButton(canvas.transform, "Interact Button", "AÇÃO", new Vector2(1f, 0f), new Vector2(-430f, 145f), new Vector2(220f, 190f), new Color(0.48f, 0.30f, 0.12f, 0.84f), true);
        CreateButton(canvas.transform, "Jump Button", "PULAR", new Vector2(1f, 0f), new Vector2(-160f, 145f), new Vector2(220f, 190f), new Color(0.16f, 0.42f, 0.30f, 0.84f), true);

        GameObject victoryPanel = CreatePanel(canvas.transform, "Victory Panel", "FASE CONCLUÍDA!", new Color(0.04f, 0.08f, 0.13f, 0.94f));
        Button victoryAgain = CreateButton(victoryPanel.transform, "Victory - Play Again", "JOGAR NOVAMENTE", new Vector2(0.5f, 0.5f), new Vector2(-245f, -95f), new Vector2(410f, 118f), new Color(0.24f, 0.62f, 0.32f, 1f), false);
        Button victoryMap = CreateButton(victoryPanel.transform, "Victory - Back to Map", "VOLTAR AO MAPA", new Vector2(0.5f, 0.5f), new Vector2(245f, -95f), new Vector2(410f, 118f), new Color(0.20f, 0.42f, 0.75f, 1f), false);

        GameObject defeatPanel = CreatePanel(canvas.transform, "Defeat Panel", "FIM DE JOGO", new Color(0.13f, 0.035f, 0.045f, 0.95f));
        Button defeatAgain = CreateButton(defeatPanel.transform, "Defeat - Play Again", "TENTAR NOVAMENTE", new Vector2(0.5f, 0.5f), new Vector2(-245f, -95f), new Vector2(410f, 118f), new Color(0.72f, 0.34f, 0.18f, 1f), false);
        Button defeatMap = CreateButton(defeatPanel.transform, "Defeat - Back to Map", "VOLTAR AO MAPA", new Vector2(0.5f, 0.5f), new Vector2(245f, -95f), new Vector2(410f, 118f), new Color(0.20f, 0.42f, 0.75f, 1f), false);

        LevelFlowController controller = new GameObject("Level Flow Controller").AddComponent<LevelFlowController>();
        SerializedObject serializedController = new SerializedObject(controller);
        SetBool(serializedController, "requiresCheese", requiresCheese);
        serializedController.FindProperty("victoryPanel").objectReferenceValue = victoryPanel;
        serializedController.FindProperty("defeatPanel").objectReferenceValue = defeatPanel;
        serializedController.FindProperty("playAgainButton").objectReferenceValue = victoryAgain;
        serializedController.FindProperty("backToSelectionButton").objectReferenceValue = victoryMap;
        serializedController.FindProperty("defeatPlayAgainButton").objectReferenceValue = defeatAgain;
        serializedController.FindProperty("defeatBackToSelectionButton").objectReferenceValue = defeatMap;
        serializedController.FindProperty("selectionSceneName").stringValue = "World";
        serializedController.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject CreatePanel(Transform parent, string name, string title, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = panel.GetComponent<Image>();
        image.sprite = pixelSprite;
        image.color = color;

        CreateText(panel.transform, name + " - title", title, new Vector2(0.5f, 0.5f), new Vector2(0f, 100f), new Vector2(1000f, 140f), 86, TextAnchor.MiddleCenter, Color.white);
        return panel;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 anchor, Vector2 position, Vector2 size, Color color, bool mobileControl)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = pixelSprite;
        image.color = color;
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.15f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        if (mobileControl)
            buttonObject.AddComponent<MobileControls>();

        Text text = CreateText(buttonObject.transform, name + " - label", label, new Vector2(0.5f, 0.5f), Vector2.zero, size * 0.86f, mobileControl ? 40 : 38, TextAnchor.MiddleCenter, Color.white);
        text.fontStyle = FontStyle.Bold;
        return button;
    }

    private static Text CreateText(Transform parent, string name, string value, Vector2 anchor, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;

        Text text = textObject.GetComponent<Text>();
        text.font = uiFont;
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static void ConfigureWorldMap()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/World.unity", OpenSceneMode.Single);
        SelecaoDeFase[] phases = Object.FindObjectsOfType<SelecaoDeFase>(true);
        foreach (SelecaoDeFase phase in phases)
        {
            bool cheesePhase = phase.name.Contains("2") || phase.name.Contains("Queijo");
            SerializedObject serializedPhase = new SerializedObject(phase);
            serializedPhase.FindProperty("nomeDaCena").stringValue = cheesePhase ? CheeseScenePath.Replace("Assets/Scenes/", "").Replace(".unity", "") : FinishScenePath.Replace("Assets/Scenes/", "").Replace(".unity", "");
            serializedPhase.FindProperty("faseDeQueijo").boolValue = cheesePhase;
            serializedPhase.ApplyModifiedPropertiesWithoutUndo();

            Collider2D phaseCollider = phase.GetComponent<Collider2D>();
            if (phaseCollider == null)
            {
                BoxCollider2D box = phase.gameObject.AddComponent<BoxCollider2D>();
                SpriteRenderer renderer = phase.GetComponent<SpriteRenderer>();
                if (renderer != null && renderer.sprite != null)
                    box.size = renderer.sprite.bounds.size;
                else
                    box.size = Vector2.one;
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    private static void ConfigureMainMenu()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Main Menu.unity", OpenSceneMode.Single);
        MainMenuController controller = Object.FindObjectOfType<MainMenuController>(true);
        if (controller == null)
            throw new System.InvalidOperationException("Main Menu precisa ter MainMenuController.");

        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("worldSelectionSceneName").stringValue = "World";
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        Button playButton = null;
        foreach (Button button in Object.FindObjectsOfType<Button>(true))
        {
            if (button.gameObject.name == "Jogar")
            {
                playButton = button;
                break;
            }
        }

        if (playButton == null)
            throw new System.InvalidOperationException("O botão Jogar não foi encontrado no Main Menu.");

        for (int i = playButton.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(playButton.onClick, i);
        UnityEventTools.AddPersistentListener(playButton.onClick, controller.Play);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    private static void ConfigureBuildScenes()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Main Menu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/World.unity", true),
            new EditorBuildSettingsScene(FinishScenePath, true),
            new EditorBuildSettingsScene(CheeseScenePath, true),
            new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true)
        };
    }

    private static Sprite CreatePixelSprite()
    {
        if (!File.Exists(PixelSpritePath))
        {
            Texture2D texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[64];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(255, 255, 255, 255);
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(PixelSpritePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        AssetDatabase.ImportAsset(PixelSpritePath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(PixelSpritePath) as TextureImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 8f;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(PixelSpritePath);
    }

    private static void EnsureFolder(string folder)
    {
        if (!AssetDatabase.IsValidFolder(folder))
        {
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            string name = Path.GetFileName(folder);
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
            throw new System.InvalidOperationException("Serialized float missing: " + propertyName);
        property.floatValue = value;
    }

    private static void SetInt(SerializedObject serializedObject, string propertyName, int value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
            throw new System.InvalidOperationException("Serialized int missing: " + propertyName);
        property.intValue = value;
    }

    private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
            throw new System.InvalidOperationException("Serialized bool missing: " + propertyName);
        property.boolValue = value;
    }
}
