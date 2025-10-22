// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.OpenXR
{
    /// <summary>
    /// Provides methods to interact with OpenXR time, including retrieving the
    /// predicting display times, and converting XR time to Query Performance Counter (QPC) time.
    /// </summary>
    public class OpenXRTime
    {
        /// <summary>
        /// Get the current OpenXRTime.
        /// </summary>
        public static OpenXRTime Current => m_current;

        /// <summary>
        /// Retrieves the predicted display time for the current frame based on the specified frame timing. 
        /// </summary>
        /// <remarks>
        /// Will return 0 if called from Unity Editor.
        /// </remarks>
        /// <param name="frameTime">The time of a frame in pipelined rendering for which to retrieve the predicted display time. <see cref="FrameTime"/></param>
        /// <returns>The predicted display time if available, otherwise 0</returns>
        public long GetPredictedDisplayTimeInXrTime(FrameTime frameTime)
        {
            return NativeLib.GetPredictedDisplayTimeInXrTime(frameTime);
        }

        /// <summary>
        /// Converts a time value from XR time to Query Performance Counter (QPC) time.
        /// </summary>
        /// <remarks>
        /// Will return 0 if called from Unity Editor.
        /// </remarks>
        /// <param name="xrTime">The time in XR time units to be converted.</param>
        /// <returns>The equivalent time in QPC units. If the conversion cannot be performed the function returns 0.</returns>
        public long ConvertXrTimeToQpcTime(long xrTime)
        {
            return NativeLib.ConvertXrTimeToQpcTime(xrTime);
        }

        private OpenXRTime() { }// use static Current property instead.
        private static readonly OpenXRTime m_current = new OpenXRTime();
    }
}
