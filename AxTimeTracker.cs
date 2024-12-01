//Written by Aexadev on 16/08/2020
//ver 1.0

using UnityEditor;
using UnityEngine;
using System;


namespace editor
{
    [InitializeOnLoad]
    public class ProjectTimeTracker
    {
        private static readonly string TotalTimeKey = "_ProjectTotalTime";
        private static readonly string TodayTimeKey = "_ProjectTodayTime";
        private static readonly string ThisSessionTimeKey = "_ProjectThisSessionTime";
        private static readonly string LastTrackedDateKey = "_ProjectLastTrackedDate";

        private static DateTime _startTime;
        private static double _totalTime;
        private static double _todayTime;
        private static double _thisSessionTime;
        private static bool _isEditorFocused = true;

        private static Vector2 _displayPosition = new Vector2(10, 10);
        private static PositionPreset _currentPreset = PositionPreset.BottomLeft;

        private static bool _forceRedrawSceneView = false;
        private static bool _showTotalTimeInSceneView = true;

        public enum PositionPreset
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            TopCenter,
            BottomCenter
        }

        static ProjectTimeTracker()
        {
            LoadTimeData();
            _startTime = DateTime.Now;
            EditorApplication.update += UpdateTime;
            EditorApplication.quitting += OnEditorQuitting;
            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            if (_forceRedrawSceneView)
            {
                EditorApplication.update += ForceRedrawSceneView;
            }
        }

        [MenuItem("Tools/Time Tracker Options")]
        public static void ShowWindow()
        {
            TimeTrackerOptionsWindow window =
                (TimeTrackerOptionsWindow)EditorWindow.GetWindow(typeof(TimeTrackerOptionsWindow), true,
                    "Time Tracker Options");
            window.Show();
        }

        private static void UpdateTime()
        {
            if (_isEditorFocused)
            {
                double deltaTime = (DateTime.Now - _startTime).TotalSeconds;
                _totalTime += deltaTime;

                if (DateTime.Now.Date != DateTime
                            .Parse(EditorPrefs.GetString(LastTrackedDateKey, DateTime.Now.ToString("yyyy-MM-dd"))).Date)
                {
                    _todayTime = 0;
                    EditorPrefs.SetString(LastTrackedDateKey, DateTime.Now.ToString("yyyy-MM-dd"));
                }

                _todayTime += deltaTime;
                _thisSessionTime += deltaTime;
                _startTime = DateTime.Now;

                EditorPrefs.SetFloat(TotalTimeKey, (float)_totalTime);
                EditorPrefs.SetFloat(TodayTimeKey, (float)_todayTime);
                EditorPrefs.SetFloat(ThisSessionTimeKey, (float)_thisSessionTime);
            }
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!_showTotalTimeInSceneView) return;

            Handles.BeginGUI();
            TimeSpan timeSpan = TimeSpan.FromSeconds(_totalTime);
            string timeText = $"Time Spent: {timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 10;

            _displayPosition = _currentPreset switch
            {
                PositionPreset.TopLeft => new Vector2(10, 10),
                PositionPreset.TopRight => new Vector2(sceneView.position.width - 210, 10),
                PositionPreset.BottomLeft => new Vector2(10, sceneView.position.height - 60),
                PositionPreset.BottomRight => new Vector2(sceneView.position.width - 210, sceneView.position.height - 60),
                PositionPreset.TopCenter => new Vector2((sceneView.position.width / 2) - 100, 10),
                PositionPreset.BottomCenter => new Vector2((sceneView.position.width / 2) - 100,
                    sceneView.position.height - 60),
                _ => throw new ArgumentOutOfRangeException()
            };

            GUILayout.BeginArea(new Rect(_displayPosition.x, _displayPosition.y, 200, 30));
            GUILayout.Label(timeText, style);
            GUILayout.EndArea();

            Handles.EndGUI();
        }

        private static void OnEditorQuitting()
        {
            EditorPrefs.SetFloat(TotalTimeKey, (float)_totalTime);
            EditorPrefs.SetFloat(TodayTimeKey, (float)_todayTime);
            EditorPrefs.SetFloat(ThisSessionTimeKey, (float)0.0);
            EditorPrefs.SetInt("TimeTrackerPositionPreset", (int)_currentPreset);
            EditorPrefs.SetString(LastTrackedDateKey, DateTime.Now.ToString("yyyy-MM-dd"));
        }

        private static void LoadTimeData()
        {
            if (EditorPrefs.HasKey(TotalTimeKey))
            {
                _totalTime = EditorPrefs.GetFloat(TotalTimeKey);
            }
            else
            {
                _totalTime = 0.0;
            }

            if (EditorPrefs.HasKey(TodayTimeKey))
            {
                _todayTime = EditorPrefs.GetFloat(TodayTimeKey);
            }
            else
            {
                _todayTime = 0.0;
            }
            
            if (EditorPrefs.HasKey("TimeTrackerPositionPreset"))
            {
                _currentPreset = (PositionPreset)EditorPrefs.GetInt("TimeTrackerPositionPreset");
            }

            _thisSessionTime = 0.0;

            _forceRedrawSceneView = EditorPrefs.GetBool("TimeTrackerForceRedrawSceneView", false);
            _showTotalTimeInSceneView = EditorPrefs.GetBool("TimeTrackerShowTotalTimeInSceneView", true);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode)
            {
                _isEditorFocused = true;
                _startTime = DateTime.Now;
            }
            else if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
            {
                _isEditorFocused = false;
                UpdateTime();
            }
        }

        public static void ResetTimer()
        {
            _totalTime = 0.0;
            _todayTime = 0.0;
            _thisSessionTime = 0.0;
            EditorPrefs.SetFloat(TotalTimeKey, (float)_totalTime);
            EditorPrefs.SetFloat(TodayTimeKey, (float)_todayTime);
            EditorPrefs.SetFloat(ThisSessionTimeKey, (float)_thisSessionTime);
        }

        public static void ModifyTime(double seconds)
        {
            _totalTime += seconds;
            _todayTime += seconds;
            _thisSessionTime += seconds;
            EditorPrefs.SetFloat(TotalTimeKey, (float)_totalTime);
            EditorPrefs.SetFloat(TodayTimeKey, (float)_todayTime);
            EditorPrefs.SetFloat(ThisSessionTimeKey, (float)_thisSessionTime);
        }

        public static void SetPositionPreset(PositionPreset preset)
        {
            _currentPreset = preset;
            EditorPrefs.SetInt("TimeTrackerPositionPreset", (int)preset);
        }

        public static void SetForceRedrawSceneView(bool value)
        {
            _forceRedrawSceneView = value;
            EditorPrefs.SetBool("TimeTrackerForceRedrawSceneView", value);

            if (_forceRedrawSceneView)
            {
                EditorApplication.update += ForceRedrawSceneView;
            }
            else
            {
                EditorApplication.update -= ForceRedrawSceneView;
            }
        }

        public static void SetShowTotalTimeInSceneView(bool value)
        {
            _showTotalTimeInSceneView = value;
            EditorPrefs.SetBool("TimeTrackerShowTotalTimeInSceneView", value);
        }

        private static void ForceRedrawSceneView()
        {
            SceneView.RepaintAll();
        }

        public static TimeSpan GetTodayTimeSpan()
        {
            return TimeSpan.FromSeconds(_todayTime);
        }
        
        public static TimeSpan ThisSessionTimeSpan()
        {
            return TimeSpan.FromSeconds(_thisSessionTime);
        }

        public static TimeSpan GetTotalTimeSpan()
        {
            return TimeSpan.FromSeconds(_totalTime);
        }
    }

    public class TimeTrackerOptionsWindow : EditorWindow
    {
        private double _customTime = 0.0;
        private ProjectTimeTracker.PositionPreset _selectedPreset;

        private bool _forceRedrawSceneView;
        private bool _showTotalTimeInSceneView;

        void OnEnable()
        {
            _selectedPreset = ProjectTimeTracker.PositionPreset.BottomLeft;
            _forceRedrawSceneView = EditorPrefs.GetBool("TimeTrackerForceRedrawSceneView", false);
            _showTotalTimeInSceneView = EditorPrefs.GetBool("TimeTrackerShowTotalTimeInSceneView", true);
            EditorApplication.update += OnEditorUpdate;
        }

        void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        void OnEditorUpdate()
        {
            Repaint();
        }

        void OnGUI()
        {
            GUILayout.Label("Time Tracker Options", EditorStyles.boldLabel);

            if (GUILayout.Button("Reset Timer"))
            {
                ProjectTimeTracker.ResetTimer();
            }

            GUILayout.Space(10);

            GUILayout.Label("Add/Subtract Time (in seconds):");
            _customTime = EditorGUILayout.DoubleField("Time:", _customTime);

            if (GUILayout.Button("Modify Time"))
            {
                ProjectTimeTracker.ModifyTime(_customTime);
            }

            GUILayout.Space(10);

            GUILayout.Label("Select Display Position:");
            _selectedPreset =
                (ProjectTimeTracker.PositionPreset)EditorGUILayout.EnumPopup("Position Preset:", _selectedPreset);

            if (GUILayout.Button("Set Position"))
            {
                ProjectTimeTracker.SetPositionPreset(_selectedPreset);
            }

            GUILayout.Space(10);

            GUILayout.Label("Scene View Settings:", EditorStyles.boldLabel);
            _forceRedrawSceneView = EditorGUILayout.Toggle("Force Redraw Scene View", _forceRedrawSceneView);
            _showTotalTimeInSceneView =
                EditorGUILayout.Toggle("Show Total Time in Scene View", _showTotalTimeInSceneView);

            if (GUILayout.Button("Apply Settings"))
            {
                ProjectTimeTracker.SetForceRedrawSceneView(_forceRedrawSceneView);
                ProjectTimeTracker.SetShowTotalTimeInSceneView(_showTotalTimeInSceneView);
            }

            GUILayout.Space(20);

            GUILayout.Label("Today's Time Spent:", EditorStyles.boldLabel);
            TimeSpan todayTimeSpan = ProjectTimeTracker.GetTodayTimeSpan();
            GUILayout.Label(
                $"Hours: {todayTimeSpan.Hours:D2} | Minutes: {todayTimeSpan.Minutes:D2} | Seconds: {todayTimeSpan.Seconds:D2}");

            GUILayout.Space(10);
            
            GUILayout.Label("Session's Time Spent:", EditorStyles.boldLabel);
            TimeSpan sessionTimeSpan = ProjectTimeTracker.ThisSessionTimeSpan();
            GUILayout.Label(
                $"Hours: {sessionTimeSpan.Hours:D2} | Minutes: {sessionTimeSpan.Minutes:D2} | Seconds: {sessionTimeSpan.Seconds:D2}");

            GUILayout.Space(10);

            GUILayout.Label("Total Time Spent:", EditorStyles.boldLabel);
            TimeSpan totalTimeSpan = ProjectTimeTracker.GetTotalTimeSpan();
            GUILayout.Label(
                $"Hours: {totalTimeSpan.Hours:D2} | Minutes: {totalTimeSpan.Minutes:D2} | Seconds: {totalTimeSpan.Seconds:D2}");

            GUILayout.FlexibleSpace();

            GUILayout.Label("© 2024 Aexadev ver 1.0", EditorStyles.centeredGreyMiniLabel);
        }
    }
}