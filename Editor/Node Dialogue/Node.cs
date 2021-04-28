using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public enum NodeType { Start, Option, Dialogue, End }

public class Node
{
    public Rect rect;
    public bool isDragged;
    public bool isSelected;
    public bool isScaling;
    public Rect rescaleIcon;
    public NodeType type;

    public string title;
    public string speaker;
    public string line;
    public List<string> options = new List<string>();
    public float optionsSpacing;

    const float rescaleIconSize = 15f;
    const float minNodeHeight = 50f;
    const float minNodeWidth = 100f;
    public const float defaultNodeHeight = 75;
    public const float defaultNodeWidth = 150;

    public ConnectionPoint inPoint;
    public List<ConnectionPoint> outPoints = new List<ConnectionPoint>();

    public GUIStyle style;
    public GUIStyle defaultNodeStyle;
    public GUIStyle selectedNodeStyle;
    private GUIStyle optionsConnectionPointStyle;
    private Action<ConnectionPoint> optionsConnectionClick;

    public Action<Node> OnRemoveNode;

    public Node(Vector2 position, float width, float height, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<Node> OnClickRemoveNode, NodeType nodeType = NodeType.Dialogue)
    {
        rect = new Rect(position.x, position.y, width, height);
        rescaleIcon = new Rect((width - rescaleIconSize) - 16, (height - rescaleIconSize) - 16, rescaleIconSize, rescaleIconSize);
        style = nodeStyle;
        inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
        outPoints.Add(new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint));
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;
        OnRemoveNode = OnClickRemoveNode;
        type = nodeType;
        Resize(Vector2.zero);
    }

    /// <summary>
    /// For Creating the Start node on any graph
    /// </summary>
    /// <param name="position"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="nodeStyle"></param>
    /// <param name="selectedStyle"></param>
    /// <param name="outPointStyle"></param>
    /// <param name="OnClickOutPoint"></param>
    /// <param name="nodeType"></param>
    public Node(Vector2 position, float width, float height, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickOutPoint, NodeType nodeType = NodeType.Start)
    {
        rect = new Rect(position.x, position.y, width, height);
        rescaleIcon = new Rect((width - rescaleIconSize) - 16, (height - rescaleIconSize) - 16, rescaleIconSize, rescaleIconSize);
        style = nodeStyle;
        outPoints.Add(new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint));
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;
        type = nodeType;
        Resize(Vector2.zero);
    }

    /// <summary>
    /// For Creating the End node on any graph
    /// </summary>
    /// <param name="position"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="nodeStyle"></param>
    /// <param name="selectedStyle"></param>
    /// <param name="inPointStyle"></param>
    /// <param name="OnClickInPoint"></param>
    /// <param name="OnClickRemoveNode"></param>
    /// <param name="nodeType"></param>
    public Node(Vector2 position, float width, float height, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, Action<ConnectionPoint> OnClickInPoint, Action<Node> OnClickRemoveNode, NodeType nodeType = NodeType.End)
    {
        rect = new Rect(position.x, position.y, width, height);
        rescaleIcon = new Rect((width - rescaleIconSize) - 16, (height - rescaleIconSize) - 16, rescaleIconSize, rescaleIconSize);
        style = nodeStyle;
        inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;
        type = nodeType;
        OnRemoveNode = OnClickRemoveNode;
        Resize(Vector2.zero);
    }

    /// <summary>
    /// For Creating the Options node on any graph
    /// </summary>
    /// <param name="position"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="nodeStyle"></param>
    /// <param name="selectedStyle"></param>
    /// <param name="inPointStyle"></param>
    /// <param name="outPointStyle"></param>
    /// <param name="OnClickInPoint"></param>
    /// <param name="OnClickOutPoint"></param>
    /// <param name="OnClickRemoveNode"></param>
    /// <param name="optionHeight"></param>
    /// <param name="nodeType"></param>
    public Node(Vector2 position, float width, float height, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<Node> OnClickRemoveNode, float optionHeight, NodeType nodeType = NodeType.Option)
    {
        rect = new Rect(position.x, position.y, width, height);
        rescaleIcon = new Rect((width - rescaleIconSize) - 16, (height - rescaleIconSize) - 16, rescaleIconSize, rescaleIconSize);
        style = nodeStyle;
        optionsSpacing = optionHeight;
        inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
        optionsConnectionPointStyle = outPointStyle;
        optionsConnectionClick = OnClickOutPoint;
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;
        OnRemoveNode = OnClickRemoveNode;
        type = nodeType;
        Resize(Vector2.zero);
    }

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }

    public void Resize(Vector2 delta)
    {
        rect.height += delta.y;
        rect.width += delta.x;
        if (rect.height < minNodeHeight) rect.height = minNodeHeight;
        if (rect.width < minNodeWidth) rect.width = minNodeWidth;
        rescaleIcon = new Rect((rect.width - rescaleIconSize) - 16, (rect.height - rescaleIconSize) - 16, rescaleIconSize, rescaleIconSize);
    }

    public void Draw()
    {
        if(inPoint != null) inPoint.Draw();
        if(type != NodeType.Option) foreach(ConnectionPoint outPoint in outPoints) outPoint.Draw();
        else { for (int i = 0; i < outPoints.Count; i++) outPoints[i].DrawOptionConnecters(Mathf.Min(optionsSpacing * i + 145, rect.height - 20)); }

        GUI.Box(rect, "", style);
        // ^^^ Keep these above the group ^^^

        nodeGUIGroup = new Rect(rect.x + 8, rect.y + 8, rect.width - 16, rect.height - 16);
        GUI.BeginGroup(nodeGUIGroup);
        GUI.BeginGroup(new Rect(0, 0, nodeGUIGroup.width, nodeGUIGroup.height - 20));
        // This holds everything for inside the nodes
        
        //GUI.Button(new Rect(0, 0, 20, 20), "T");
        switch (type)
        {
            case NodeType.Start:
                DrawStart();
                break;
            case NodeType.Dialogue:
                DrawDialogue();
                break;
            case NodeType.Option:
                DrawOptions();
                break;
            case NodeType.End:
                DrawEnd();
                break;
        }

        // End of a node
        GUI.EndGroup();
        GUI.DrawTexture(rescaleIcon, EditorGUIUtility.IconContent("d_ScaleTool On").image, ScaleMode.StretchToFill, true, 1, Color.white, Vector4.zero, Vector4.zero);
        GUI.EndGroup();
        // Debug for rect stats when I need to find numbers
        //if (type == NodeType.Option)
        //{
        //    if (GUI.Button(new Rect(50,50,100,100), "T"))
        //    {
        //        Debug.Log(rect);
        //    }
        //}
    }
    Rect nodeGUIGroup;

    void DrawStart()
    {
        EditorGUI.LabelField(new Rect(0, 0, nodeGUIGroup.width, 20), speaker ?? "Dialogue Name");
        speaker = EditorGUI.TextField(new Rect(0, 20, nodeGUIGroup.width, Mathf.Max(nodeGUIGroup.height - rescaleIcon.width - 20, 3)), speaker, EditorStyles.textArea);        
    }

    void DrawDialogue()
    {
        int step = 0; int stepDistance = 20;
        EditorGUI.LabelField(new Rect(0, step * stepDistance, nodeGUIGroup.width, stepDistance), "Talking: " + (speaker ?? "")); step++;
        EditorGUI.LabelField(new Rect(0, step * stepDistance, 100, stepDistance), "Speaker:", NodeBasedEditor.middleLeftTextStyle);
        speaker = EditorGUI.TextField(new Rect(50, step * stepDistance, Mathf.Max(nodeGUIGroup.width - 50, 4), stepDistance), speaker, NodeBasedEditor.speakerStyle); step++;
        EditorGUI.LabelField(new Rect(0, step * stepDistance, 100, stepDistance), "Line:", NodeBasedEditor.middleLeftTextStyle);
        line = EditorGUI.TextField(new Rect(50, step * stepDistance, Mathf.Max(nodeGUIGroup.width - 50, 4), stepDistance * 2), line, EditorStyles.textArea); step += 2;
    }

    void DrawOptions()
    {
        int step = 0; int stepDistance = 20;
        EditorGUI.LabelField(new Rect(0, step * stepDistance, nodeGUIGroup.width, stepDistance), "Options: " + (speaker ?? "")); step++;
        EditorGUI.LabelField(new Rect(0, step * stepDistance, 100, stepDistance), "Speaker:", NodeBasedEditor.middleLeftTextStyle);
        speaker = EditorGUI.TextField(new Rect(50, step * stepDistance, Mathf.Max(nodeGUIGroup.width - 50, 4), stepDistance), speaker, NodeBasedEditor.speakerStyle); step++;
        EditorGUI.LabelField(new Rect(0, step * stepDistance, 100, stepDistance), "Line:", NodeBasedEditor.middleLeftTextStyle);
        line = EditorGUI.TextField(new Rect(50, step * stepDistance, Mathf.Max(nodeGUIGroup.width - 50, 4), stepDistance * 2), line, EditorStyles.textArea); step += 2;

        // Now the options
        if(GUI.Button(new Rect(0, step * stepDistance, 120, stepDistance), "Add New Option"))
        {
            options.Add("");
            outPoints.Add(new ConnectionPoint(this, ConnectionPointType.Out, optionsConnectionPointStyle, optionsConnectionClick));
        }
        step++;
        for (int i = 0; i < options.Count; i++)
        {
            EditorGUI.LabelField(new Rect(0, step * stepDistance, nodeGUIGroup.width, stepDistance), "Option " + i, NodeBasedEditor.middleLeftTextStyle);
            options[i] = EditorGUI.TextField(new Rect(50, step * stepDistance, Mathf.Max(nodeGUIGroup.width - 50, 4), stepDistance * 2), options[i], EditorStyles.textArea); step += 2;
            if (GUI.Button(new Rect(0, step * stepDistance, 100, stepDistance), "Delete ^"))
            {
                options.RemoveAt(i);
                outPoints.RemoveAt(i);
                DrawOptions();
                return;
            }
            step++;
        }

    }

    void DrawEnd()
    {
        int step = 0; int stepDistance = 20;
        GUIStyle temp = new GUIStyle();
        temp.alignment = TextAnchor.MiddleLeft;
        EditorGUI.LabelField(new Rect(0, step * stepDistance, nodeGUIGroup.width, stepDistance), speaker ?? "End Flag:"); step++;
        speaker = EditorGUI.TextField(new Rect(0, 20, nodeGUIGroup.width, Mathf.Max(nodeGUIGroup.height - rescaleIcon.width - 20, 3)), speaker, EditorStyles.textArea);
    }

    public bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if (rescaleIcon.Contains(e.mousePosition - nodeGUIGroup.position))
                    {
                        isScaling = true;
                    }
                    if (rect.Contains(e.mousePosition))
                    {
                        isDragged = true;
                        GUI.changed = true;
                        isSelected = true;
                        style = selectedNodeStyle;
                    }
                    else
                    {
                        GUI.changed = true;
                        isSelected = false;
                        style = defaultNodeStyle;
                    }
                    //if (inPoint != null && inPoint.rect.Contains(e.mousePosition))
                    //{
                    //    inPoint.OnClickConnectionPoint(inPoint);
                    //}
                    //foreach (ConnectionPoint outPoint in outPoints)
                    //{
                    //    if (outPoint.rect.Contains(e.mousePosition))
                    //    {
                    //        outPoint.OnClickConnectionPoint(outPoint);
                    //    }
                    //}
                }

                if (e.button == 1 && isSelected && rect.Contains(e.mousePosition))
                {
                    ProcessContextMenu();
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                //if (inPoint != null && inPoint.rect.Contains(e.mousePosition))
                //{
                //    inPoint.OnClickConnectionPoint(inPoint);
                //}
                //foreach(ConnectionPoint outPoint in outPoints)
                //{
                //    if (outPoint.rect.Contains(e.mousePosition))
                //    {
                //        outPoint.OnClickConnectionPoint(outPoint);
                //    }
                //}
                isDragged = false;
                isScaling = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && isDragged)
                {
                    if (isScaling)
                    {
                        Resize(e.delta);
                        e.Use();
                        return true;
                    }
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }

        return false;
    }

    void ProcessContextMenu()
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
        genericMenu.ShowAsContext();
    }

    void OnClickRemoveNode()
    {
        if(OnRemoveNode != null)
        {
            OnRemoveNode(this);
        }
    }

}


