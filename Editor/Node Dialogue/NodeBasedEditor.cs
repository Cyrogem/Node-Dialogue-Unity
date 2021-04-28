using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeBasedEditor : EditorWindow
{
    static Node entranceNode;
    static List<Node> nodes;
    static List<Connection> connections;

    GUIStyle nodeStyle;
    GUIStyle selectedNodeStyle;
    GUIStyle inPointStyle;
    GUIStyle outPointStyle;
    GUIStyle entranceNodeStyle;
    GUIStyle entranceSelectedStyle;
    GUIStyle exitNodeStyle;
    GUIStyle exitSelectedStyle;

    public static GUIStyle speakerStyle;
    public static GUIStyle middleLeftTextStyle;

    ConnectionPoint selectedInPoint;
    ConnectionPoint selectedOutPoint;

    Vector2 offset;
    Vector2 drag;
    const float menuBarHeight = 20f;
    

    [MenuItem("ToolSak/Node Based Editor")]
    private static void OpenWindow()
    {
        NodeBasedEditor window = GetWindow<NodeBasedEditor>();
        window.titleContent = new GUIContent("Cyrogem's Dialogue Maker");
    }

    private void OnEnable()
    {
        nodeStyle = new GUIStyle();
        nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        nodeStyle.border = new RectOffset(12, 12, 12, 12);
        
        selectedNodeStyle = new GUIStyle();
        selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);

        entranceNodeStyle = new GUIStyle();
        entranceNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node3.png") as Texture2D;
        entranceNodeStyle.border = new RectOffset(12, 12, 12, 12);

        entranceSelectedStyle = new GUIStyle();
        entranceSelectedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node3 on.png") as Texture2D;
        entranceSelectedStyle.border = new RectOffset(12, 12, 12, 12);

        exitNodeStyle = new GUIStyle();
        exitNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node6.png") as Texture2D;
        exitNodeStyle.border = new RectOffset(12, 12, 12, 12);

        exitSelectedStyle = new GUIStyle();
        exitSelectedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node6 on.png") as Texture2D;
        exitSelectedStyle.border = new RectOffset(12, 12, 12, 12);

        inPointStyle = new GUIStyle();
        inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
        inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        inPointStyle.border = new RectOffset(4, 4, 12, 12);

        outPointStyle = new GUIStyle();
        outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
        outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
        outPointStyle.border = new RectOffset(4, 4, 12, 12);

        middleLeftTextStyle = new GUIStyle();
        middleLeftTextStyle.alignment = TextAnchor.MiddleLeft;

        if (entranceNode == null)
        {
            entranceNode = new Node(new Vector2(Mathf.Min(50, position.width / 2 - Node.defaultNodeWidth / 2), position.height / 2 - Node.defaultNodeHeight / 2), Node.defaultNodeWidth, Node.defaultNodeHeight, entranceNodeStyle, entranceSelectedStyle, outPointStyle, OnClickOutPoint);
            if (nodes == null)
            {
                nodes = new List<Node>();
            }
            nodes.Add(entranceNode);
        }
    }
    static float scale = 1;
    private void OnGUI()
    {
        if (speakerStyle == null)
        {
            speakerStyle = new GUIStyle(EditorStyles.textField);
            speakerStyle.alignment = TextAnchor.MiddleLeft;
        }
        if (Event.current.type == EventType.KeyDown)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.KeypadPlus:
                    if (scale <= 1)
                    {
                        scale *= 2;
                    } else
                    {
                        scale++;
                    }
                    GUI.changed = true;
                    break;
                case KeyCode.KeypadMinus:
                    if (scale <= 1)
                    {
                        scale /= 2;
                    }
                    else
                    {
                        scale--;
                    }
                    GUI.changed = true;
                    break;
            }
        }

        DrawBackground(Color.black);
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        DrawNodes();
        DrawConnections();

        DrawConnectionLine(Event.current);

        DrawMenuBar();

        ProcessMenuEvents(Event.current);
        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        if (GUI.changed || EditorGUIUtility.editingTextField) Repaint();
    }

    void DrawGrid(float gridSpacing, float gridOpacity, Color gridcolor)
    {
        float scaledGridSpacing = gridSpacing / scale;

        int widthDivs = Mathf.CeilToInt(position.width / scaledGridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / scaledGridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridcolor.r, gridcolor.g, gridcolor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % scaledGridSpacing, offset.y % scaledGridSpacing, 0);

        for (int i = 0; i <= widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(scaledGridSpacing * i, -scaledGridSpacing, 0) + newOffset,
                new Vector3(scaledGridSpacing * i, position.height * 1.5f, 0) + newOffset);
        }

        for (int j = 0; j <= heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-scaledGridSpacing, scaledGridSpacing * j, 0) + newOffset,
                new Vector3(position.width * 1.5f, scaledGridSpacing * j, 0) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    void DrawBackground(Color color)
    {
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), color);
    }

    void DrawMenuBar()
    {
        menu = new Rect(0, 0, position.width, menuBarHeight);

        GUILayout.BeginArea(menu, EditorStyles.toolbar);
        GUILayout.BeginHorizontal();


        if(GUILayout.Button(new GUIContent("Center View"), EditorStyles.toolbarButton, GUILayout.Width(75)))
        {
            OnDrag(new Vector2(position.width / 2 - entranceNode.rect.width / 2 - entranceNode.rect.position.x, position.height / 2 - entranceNode.rect.height / 2 - entranceNode.rect.position.y));
        }

        if (GUILayout.Button(new GUIContent("Save"), EditorStyles.toolbarButton, GUILayout.Width(75)))
        {
            SaveTheData();
        }

        GUILayout.Space(Mathf.Max(50, position.width - 420));
        
        data = (DialogueSaveData)EditorGUILayout.ObjectField(data, typeof(DialogueSaveData), true, GUILayout.Width(175));

        if (GUILayout.Button(new GUIContent("Load"), EditorStyles.toolbarButton, GUILayout.Width(75)))
        {
            LoadTheData();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
    Rect menu;
    DialogueSaveData data;

    private void DrawNodes()
    {
        if(nodes != null)
        {
            for(int i = 0; i < nodes.Count; i++)
            {
                if(nodes[i] != entranceNode) nodes[i].Draw();
            }
        }
        if (entranceNode != null) entranceNode.Draw();
    }

    void DrawConnections()
    {
        if (connections != null)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                connections[i].Draw();
            }
        }
    }

    private void ProcessEvents(Event e)
    {
        drag = Vector2.zero;

        switch (e.type)
        {
            case EventType.MouseDown:
                if(e.button == 1)
                {
                    ProcessContextMenu(e.mousePosition);
                }

                if(e.button == 0)
                {
                    selectedInPoint = null;
                    selectedOutPoint = null;
                    GUI.FocusControl(null);
                }
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && selectedOutPoint == null && selectedInPoint == null)
                {
                    OnDrag(e.delta);
                }
                break;
        }
    }

    void ProcessNodeEvents(Event e)
    {
        if (nodes != null)
        {
            for(int i = nodes.Count - 1; i >= 0; i--)
            {
                bool guiChanged = nodes[i].ProcessEvents(e);

                if (guiChanged)
                {
                    GUI.changed = true;
                }
            }
        }
    }

    void ProcessMenuEvents(Event e)
    {
        if (menu.Contains(e.mousePosition))
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    e.Use();
                    break;
            }
        }
    }

    void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add Node"), false, () => OnClickAddNode(mousePosition));
        genericMenu.AddItem(new GUIContent("Add Options"), false, () => OnClickAddOptionsNode(mousePosition));
        genericMenu.AddItem(new GUIContent("Add End Node"), false, () => OnClickAddEndNode(mousePosition));
        genericMenu.ShowAsContext();
    }

    void OnClickAddNode(Vector2 mousePosition)
    {
        if (nodes == null)
        {
            nodes = new List<Node>();
        }
        nodes.Add(new Node(mousePosition, 250, 120, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode));
    }

    void OnClickAddEndNode(Vector2 mousePosition)
    {
        if (nodes == null)
        {
            nodes = new List<Node>();
        }
        nodes.Add(new Node(mousePosition, 150, 75, exitNodeStyle, exitSelectedStyle, inPointStyle, OnClickInPoint, OnClickRemoveNode));
    }

    void OnClickAddOptionsNode(Vector2 mousePosition)
    {
        if (nodes == null)
        {
            nodes = new List<Node>();
        }
        nodes.Add(new Node(mousePosition, 250, 150, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, 60));
    }

    void OnClickInPoint(ConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;

        if (selectedOutPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection();
                ClearConnectionSelection();
            } else
            {
                ClearConnectionSelection();
            }
        }
    }

    void OnClickOutPoint(ConnectionPoint outPoint)
    {
        selectedOutPoint = outPoint;

        if (selectedInPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection();
                ClearConnectionSelection();
            } else
            {
                ClearConnectionSelection();
            }
        }
    }

    void OnClickRemoveConnection(Connection connection)
    {
        connections.Remove(connection);
    }

    void OnClickRemoveNode(Node node)
    {
        if(connections != null)
        {
            List<Connection> connectionsToRemove = new List<Connection>();

            for(int i = 0; i < connections.Count; i++)
            {
                if (connections[i].inPoint == node.inPoint || node.outPoints.Contains(connections[i].outPoint))
                {
                    connectionsToRemove.Add(connections[i]);
                }
            }

            for (int i = 0; i < connectionsToRemove.Count; i++)
            {
                connections.Remove(connectionsToRemove[i]);
            }

            connectionsToRemove = null;
        }

        nodes.Remove(node);
    }

    void CreateConnection()
    {
        if (connections == null)
        {
            connections = new List<Connection>();
        }

        for(int i = 0; i < connections.Count; i++)
        {
            if (connections[i].outPoint == selectedOutPoint)
            {
                if (connections[i].inPoint == selectedInPoint)
                {
                    connections.RemoveAt(i);
                    selectedInPoint.connectedTo = null;
                    selectedOutPoint.connectedTo = null;
                    return;
                }
                connections.RemoveAt(i);
                break;
            }
        }

        connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
        selectedInPoint.connectedTo = selectedOutPoint.node;
        selectedOutPoint.connectedTo = selectedInPoint.node;
    }

    void DrawConnectionLine(Event e)
    {
        if (selectedInPoint != null && selectedOutPoint == null)
        {
            Handles.DrawBezier(
                selectedInPoint.rect.center,
                e.mousePosition,
                selectedInPoint.rect.center + Vector2.left * 50f,
                e.mousePosition + Vector2.right * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }
        else if (selectedOutPoint != null && selectedInPoint == null)
        {
            Handles.DrawBezier(
                selectedOutPoint.rect.center,
                e.mousePosition,
                selectedOutPoint.rect.center + Vector2.right * 50f,
                e.mousePosition + Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }
    }

    void ClearConnectionSelection()
    {
        selectedInPoint = null;
        selectedOutPoint = null;
    }

    void OnDrag(Vector2 delta)
    {
        drag = delta;

        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Drag(delta);
            }
        }

        GUI.changed = true;
    }

    void SaveTheData(string customName = null)
    {
        DialogueSaveData saveData = (DialogueSaveData)CreateInstance(typeof(DialogueSaveData));
        saveData.target = new List<DialogueTarget>();
        // Time to save everything

        saveData.dialogueName = customName ?? (entranceNode.speaker ?? "Untitled Dialogue");
        if (saveData.dialogueName == "") saveData.dialogueName = "Untitled Dialogue";
        saveData.nodeCount = nodes.Count;
        for(int i = 0; i < nodes.Count; i++)
        {
            switch (nodes[i].type)
            {
                case NodeType.Dialogue:
                    saveData.speakers.Add((nodes[i].speaker ?? "")); // Speaker
                    saveData.lines.Add((nodes[i].line ?? "")); // Line
                    saveData.type.Add(1);                         // Type
                    saveData.target.Add(new DialogueTarget { target = new List<Vector2> { new Vector2(0, ((nodes[i].outPoints[0].connectedTo != null) ? nodes.IndexOf(nodes[i].outPoints[0].connectedTo) : -1)) } }); // Target
                    saveData.position.Add(nodes[i].rect.position); // Position
                    saveData.dimensions.Add(nodes[i].rect.size); // Dimensions
                    break;

                case NodeType.Start:
                    saveData.speakers.Add((nodes[i].speaker ?? "")); // Speaker
                    saveData.lines.Add((nodes[i].line ?? "")); // Line
                    saveData.type.Add(0);                         // Type
                    saveData.target.Add(new DialogueTarget { target = new List<Vector2> { new Vector2(0, ((nodes[i].outPoints[0].connectedTo != null) ? nodes.IndexOf(nodes[i].outPoints[0].connectedTo) : -1)) } }); // Target
                    saveData.position.Add(nodes[i].rect.position); // Position
                    saveData.dimensions.Add(nodes[i].rect.size); // Dimensions
                    break;

                case NodeType.End:
                    saveData.speakers.Add((nodes[i].speaker ?? "")); // Speaker
                    saveData.lines.Add((nodes[i].line ?? "")); // Line
                    saveData.type.Add(2);                         // Type
                    saveData.target.Add(new DialogueTarget { target = new List<Vector2> { new Vector2(0, -1) } }); // No target for end nodes
                    saveData.position.Add(nodes[i].rect.position); // Position
                    saveData.dimensions.Add(nodes[i].rect.size); // Dimensions
                    break;

                case NodeType.Option:
                    saveData.speakers.Add((nodes[i].speaker ?? "")); // Speaker
                    saveData.lines.Add((nodes[i].line ?? "")); // Line
                    saveData.type.Add(3 + nodes[i].options.Count + (saveData.optionLines.Count * Mathf.Pow(10, -6))); // Type

                    foreach(string s in nodes[i].options) // Option Lines
                    {
                        saveData.optionLines.Add(s);
                    }

                    List<Vector2> targets = new List<Vector2>();
                    for(int j = 0; j < nodes[i].outPoints.Count; j++)
                    {
                        targets.Add(new Vector2(j, ((nodes[i].outPoints[j].connectedTo != null) ? nodes.IndexOf(nodes[i].outPoints[j].connectedTo) : -1)));
                    }
                    saveData.target.Add(new DialogueTarget { target = targets }); // Targets
                    saveData.position.Add(nodes[i].rect.position); // Position
                    saveData.dimensions.Add(nodes[i].rect.size); // Dimensions
                    break;
            }
        }

        if (!AssetDatabase.IsValidFolder("Assets/Dialogue")) AssetDatabase.CreateFolder("Assets", "Dialogue");
        string name = "Assets/Dialogue/" + saveData.dialogueName + ".asset";
        int nameIteration = 0;
        while((ScriptableObject)AssetDatabase.LoadAssetAtPath(name, typeof(ScriptableObject)) != null)
        {
            nameIteration++;
            name = "Assets/Dialogue/" + saveData.dialogueName + " (" + nameIteration + ").asset";
        }
        AssetDatabase.CreateAsset(saveData, name);
        AssetDatabase.Refresh();
    }

    void LoadTheData()
    {
        if (data == null) return;
        SaveTheData("Autosave");

        if(connections != null) connections.Clear();
        if(nodes != null) nodes.Clear();
        entranceNode = null;
        selectedInPoint = null;
        selectedOutPoint = null;

        // Now load the old data
        // Start with creating all nodes, then connect them afterwards
        for(int i = 0; i < data.nodeCount; i++)
        {
            switch (data.type[i])
            {
                case 0: // Entrance node
                    nodes.Add(new Node(data.position[i], data.dimensions[i].x, data.dimensions[i].y, entranceNodeStyle, entranceSelectedStyle, outPointStyle, OnClickOutPoint));
                    nodes[i].speaker = data.speakers[i];
                    entranceNode = nodes[i];
                    break;

                case 1: // Dialogue node
                    nodes.Add(new Node(data.position[i], data.dimensions[i].x, data.dimensions[i].y, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode));
                    nodes[i].speaker = data.speakers[i];
                    nodes[i].line = data.lines[i];
                    break;

                case 2: // End node
                    nodes.Add(new Node(data.position[i], data.dimensions[i].x, data.dimensions[i].y, exitNodeStyle, exitSelectedStyle, inPointStyle, OnClickInPoint, OnClickRemoveNode));
                    nodes[i].speaker = data.speakers[i];
                    break;

                default: // Options node
                    nodes.Add(new Node(data.position[i], data.dimensions[i].x, data.dimensions[i].y, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, 60));
                    nodes[i].speaker = data.speakers[i];
                    nodes[i].line = data.lines[i];

                    // Add options
                    int initialOptionIndex = (int)((data.type[i] - Mathf.Floor(data.type[i])) * Mathf.Pow(10, 6));
                    for(int j = 0; j < Mathf.Floor(data.type[i] - 3); j++)
                    {
                        nodes[i].options.Add(data.optionLines[initialOptionIndex + j]);
                        nodes[i].outPoints.Add(new ConnectionPoint(nodes[i], ConnectionPointType.Out, outPointStyle, OnClickOutPoint));
                    }
                    break;
            }
        }
        // Now we connect them all
        for(int i = 0; i < data.target.Count; i++)
        {
            for(int j = 0; j < data.target[i].target.Count; j++)
            {
                if(data.target[i].target[j].y >= 0)
                {
                    selectedOutPoint = nodes[i].outPoints[j];
                    selectedInPoint = nodes[(int)data.target[i].target[j].y].inPoint;
                    CreateConnection();
                }
            }
        }
        // And we are done
    }

}
