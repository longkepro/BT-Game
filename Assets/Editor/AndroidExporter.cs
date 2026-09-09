#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public class AndroidExporter
{
    [MenuItem("Tools/Export Android Studio Project")]
    public static void ExportAndroidStudioProject()
    {
        Debug.Log("[AndroidExporter] Starting Android Studio project export...");

        // Ensure Android build settings
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.buildAppBundle = false;

        // Player Settings for Android
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.DefaultCompany.BTGame");
        PlayerSettings.productName = "BT-Game";
        PlayerSettings.companyName = "DefaultCompany";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

        // Point Android SDK, JDK, and NDK to installed locations
        string sdkPath = "C:/Users/ADMIN/AppData/Local/Android/Sdk";
        string jdkPath = "C:/Users/ADMIN/OpenJDK17";
        string ndkPath = "C:/Users/ADMIN/AndroidNDK/android-ndk-r23b";

        EditorPrefs.SetString("AndroidSdkRoot", sdkPath);
        EditorPrefs.SetBool("SdkUseEmbedded", false);
        EditorPrefs.SetString("JdkPath", jdkPath);
        EditorPrefs.SetBool("JdkUseEmbedded", false);
        EditorPrefs.SetString("AndroidNdkRoot", ndkPath);
        EditorPrefs.SetBool("NdkUseEmbedded", false);

        UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath = jdkPath;
        UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath = sdkPath;
        UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath = ndkPath;

        System.Environment.SetEnvironmentVariable("JAVA_HOME", jdkPath);
        System.Environment.SetEnvironmentVariable("ANDROID_HOME", sdkPath);
        System.Environment.SetEnvironmentVariable("ANDROID_SDK_ROOT", sdkPath);
        System.Environment.SetEnvironmentVariable("ANDROID_NDK_ROOT", ndkPath);
        System.Environment.SetEnvironmentVariable("ANDROID_NDK_HOME", ndkPath);

        string exportPath = "C:/Users/ADMIN/Desktop/BT-Game-Android";

        string[] scenes = new string[] { "Assets/A.unity" };

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = exportPath,
            target = BuildTarget.Android,
            options = BuildOptions.AcceptExternalModificationsToPlayer
        };

        Debug.Log("[AndroidExporter] Invoking BuildPipeline.BuildPlayer...");
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("[AndroidExporter] SUCCESS! Exported to: " + exportPath);
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("[AndroidExporter] FAILED! Total errors: " + summary.totalErrors);
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == LogType.Error || msg.type == LogType.Exception)
                    {
                        Debug.LogError($"[AndroidExporter Step: {step.name}] {msg.content}");
                    }
                }
            }
        }
    }
}
#endif
