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

        // Check if custom paths exist (for local machine setup), otherwise fallback to Unity's default installed Android tools
        string customJdk = "C:/Users/ADMIN/OpenJDK17";
        string customNdk = "C:/Users/ADMIN/AndroidNDK/android-ndk-r23b";
        string customSdk = "C:/Users/ADMIN/AppData/Local/Android/Sdk";

        if (Directory.Exists(customJdk) && Directory.Exists(customNdk) && Directory.Exists(customSdk))
        {
            EditorPrefs.SetString("AndroidSdkRoot", customSdk);
            EditorPrefs.SetBool("SdkUseEmbedded", false);
            EditorPrefs.SetString("JdkPath", customJdk);
            EditorPrefs.SetBool("JdkUseEmbedded", false);
            EditorPrefs.SetString("AndroidNdkRoot", customNdk);
            EditorPrefs.SetBool("NdkUseEmbedded", false);

            UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath = customJdk;
            UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath = customSdk;
            UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath = customNdk;

            System.Environment.SetEnvironmentVariable("JAVA_HOME", customJdk);
            System.Environment.SetEnvironmentVariable("ANDROID_HOME", customSdk);
            System.Environment.SetEnvironmentVariable("ANDROID_SDK_ROOT", customSdk);
            System.Environment.SetEnvironmentVariable("ANDROID_NDK_ROOT", customNdk);
            System.Environment.SetEnvironmentVariable("ANDROID_NDK_HOME", customNdk);
        }

        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
        if (string.IsNullOrEmpty(desktopPath)) desktopPath = "C:/Users/ADMIN/Desktop";
        string exportPath = Path.Combine(desktopPath, "BT-Game-Android").Replace("\\", "/");

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
