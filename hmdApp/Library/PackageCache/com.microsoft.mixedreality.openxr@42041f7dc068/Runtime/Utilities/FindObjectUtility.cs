// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using System;

namespace Microsoft.MixedReality.OpenXR
{
    // A static utility used to avoid deprecated Find Object functions in favor of replacements introduced in Unity >= 2021.3.18. 
    internal static class FindObjectUtility
    {

        // Returns the first object matching the specified type.
        // If Unity >= 2021.3.18, calls FindFirstObjectByType. Otherwise calls FindObjectOfType.
        // includeInactive - If true, inactive objects will be included in the search. False by default.
        internal static T FindFirstObjectByType<T>(bool includeInactive = false) where T : Component
        {
#if UNITY_2021_3_18_OR_NEWER
            return UnityEngine.Object.FindFirstObjectByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
#else
            return UnityEngine.Object.FindObjectOfType<T>(includeInactive);
#endif
        }

        // Returns an object matching the specified type. 
        // If Unity >= 2021.3.18, calls FindAnyObjectByType. Otherwise calls FindObjectOfType.
        // includeInactive - If true, inactive objects will be included in the search. False by default.
        internal static T FindAnyObjectByType<T>(bool includeInactive = false) where T : Component
        {
#if UNITY_2021_3_18_OR_NEWER
            return UnityEngine.Object.FindAnyObjectByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
#else
            return UnityEngine.Object.FindObjectOfType<T>(includeInactive);
#endif
        }

        // Returns all objects matching the specified type.
        // If Unity >= 2021.3.18, calls FindObjectsByType. Otherwise calls FindObjectsOfType.
        // includeInactive - If true, inactive objects will be included in the search. False by default.
        // sort - If false, results will not sorted by InstanceID. True by default.
        internal static T[] FindObjectsByType<T>(bool includeInactive = false, bool sort = true) where T : Component
        {
#if UNITY_2021_3_18_OR_NEWER
            return UnityEngine.Object.FindObjectsByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, sort ? FindObjectsSortMode.InstanceID : FindObjectsSortMode.None);
#else
            return UnityEngine.Object.FindObjectsOfType<T>(includeInactive);
#endif
        }

        // Returns all objects matching the specified type.
        // If Unity >= 2021.3.18, calls FindObjectsByType. Otherwise calls FindObjectsOfType.
        // includeInactive - If true, inactive objects will be included in the search. False by default.
        // sort - If false, results will not sorted by InstanceID. True by default.
        // type - The type to search for.
        internal static UnityEngine.Object[] FindObjectsByType(Type type, bool includeInactive = false, bool sort = true)
        {
#if UNITY_2021_3_18_OR_NEWER
            return UnityEngine.Object.FindObjectsByType(type, includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, sort ? FindObjectsSortMode.InstanceID : FindObjectsSortMode.None);
#else
            return UnityEngine.Object.FindObjectsOfType(type, includeInactive);
#endif
        }
    }
}